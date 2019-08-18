using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using Palmmedia.ReportGenerator.Core.Parser.Preprocessing;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Parser
{
    /// <summary>
    /// Initiates the corresponding parser to the given report file.
    /// </summary>
    public class CoverageReportParser
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(CoverageReportParser));

        /// <summary>
        /// The number reports that are parsed and processed in parallel.
        /// </summary>
        private readonly int numberOfReportsParsedInParallel;

        /// <summary>
        /// The number reports that are merged in parallel.
        /// </summary>
        private readonly int numberOfReportsMergedInParallel;

        /// <summary>
        /// The number reports that are read in memory in parallel.
        /// </summary>
        private readonly int numberOfReportsReadOnMemoryInParallel;

        /// <summary>
        /// The source directories.
        /// </summary>
        private readonly IEnumerable<string> sourceDirectories;

        /// <summary>
        /// The assembly filter.
        /// </summary>
        private readonly IFilter assemblyFilter;

        /// <summary>
        /// The class filter.
        /// </summary>
        private readonly IFilter classFilter;

        /// <summary>
        /// The file filter.
        /// </summary>
        private readonly IFilter fileFilter;

        /// <summary>
        /// The current merge count.
        /// </summary>
        private int mergeCount;

        /// <summary>
        /// The current file parser count.
        /// </summary>
        private int fileParserCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverageReportParser" /> class.
        /// </summary>
        /// <param name="numberOfReportsParsedInParallel">The number reports that are parsed and processed in parallel.</param>
        /// <param name="numberOfReportsMergedInParallel">The number reports that are merged in parallel.</param>
        /// <param name="numberOfReportsReadOnMemoryInParallel">The number reports that are read in memory in parallel.</param>
        /// <param name="sourceDirectories">The source directories.</param>
        /// <param name="assemblyFilter">The assembly filter.</param>
        /// <param name="classFilter">The class filter.</param>
        /// <param name="fileFilter">The file filter.</param>
        public CoverageReportParser(int numberOfReportsParsedInParallel, int numberOfReportsMergedInParallel, int numberOfReportsReadOnMemoryInParallel, IEnumerable<string> sourceDirectories, IFilter assemblyFilter, IFilter classFilter, IFilter fileFilter)
        {
            this.numberOfReportsParsedInParallel = Math.Max(1, numberOfReportsParsedInParallel);
            this.numberOfReportsMergedInParallel = Math.Max(1, numberOfReportsMergedInParallel);
            this.numberOfReportsReadOnMemoryInParallel = Math.Max(1, numberOfReportsReadOnMemoryInParallel);
            this.sourceDirectories = sourceDirectories ?? throw new ArgumentNullException(nameof(sourceDirectories));
            this.assemblyFilter = assemblyFilter ?? throw new ArgumentNullException(nameof(assemblyFilter));
            this.classFilter = classFilter ?? throw new ArgumentNullException(nameof(classFilter));
            this.fileFilter = fileFilter ?? throw new ArgumentNullException(nameof(fileFilter));
        }

        /// <summary>
        /// Tries to initiate the correct parsers for the given reports.
        /// </summary>
        /// <param name="reportFiles">The report files to parse.</param>
        /// <returns>
        /// The IParser instance.
        /// </returns>
        public ParserResult ParseFiles(IReadOnlyCollection<string> reportFiles)
        {
            if (reportFiles == null)
            {
                throw new ArgumentNullException(nameof(reportFiles));
            }

            List<Task<ParserResult>> mergeConsumers = new List<Task<ParserResult>>();
            List<Task<long>> parserConsumer = new List<Task<long>>();
            try
            {
                using (BlockingCollection<Tuple<string, string>> filesContentCollection = new BlockingCollection<Tuple<string, string>>(int.MaxValue))
                {
                    using (BlockingCollection<ParserResult> parserResults = new BlockingCollection<ParserResult>())
                    {
                        Task<long> fileContentProducer = this.CreateFileReadProducer(reportFiles, filesContentCollection);
                        foreach (var item in Enumerable.Range(0, this.numberOfReportsMergedInParallel))
                        {
                            mergeConsumers.Add(this.CreateConsumer(parserResults));
                        }

                        foreach (var item in Enumerable.Range(0, this.numberOfReportsParsedInParallel))
                        {
                            parserConsumer.Add(this.CreateFileParserProducer(filesContentCollection, parserResults, reportFiles.Count));
                        }

                        Task.WaitAll(parserConsumer.Concat(new[] { fileContentProducer }).ToArray());
                        List<long> average = parserConsumer.Select(t => t.Result).Where(t => t != 0).ToList();
                        if (average.Any())
                        {
                            long overallAverage = average.Sum() / average.Count;
                            Logger.Info($"Overall average of parsing time is {overallAverage / 1000d:f1} seconds");
                        }

                        if (fileContentProducer.Result != 0)
                        {
                            Logger.Info($"Overall average of loading files into memory is {fileContentProducer.Result / 1000d:f1} seconds");
                        }

                        parserResults.CompleteAdding();
                        Task.WaitAll(mergeConsumers.ToArray());
                    }
                }
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.Flatten().InnerExceptions)
                {
                    if (e is UnsupportedParserException)
                    {
                        throw e;
                    }

                    Console.WriteLine(e.StackTrace);
                }

                throw;
            }

            List<ParserResult> results = mergeConsumers.Select(t => t.Result).ToList();
            ParserResult finalResult = results.First();
            foreach (ParserResult toBeMerged in results.Skip(1))
            {
                this.MergeResults(finalResult, toBeMerged);
            }

            return finalResult;
        }

        private Task<long> CreateFileReadProducer(IReadOnlyCollection<string> reportFiles, BlockingCollection<Tuple<string, string>> collection)
        {
            return Task.Factory.StartNew(() =>
            {
                long average = 0;
                try
                {
                    foreach (string reportFile in reportFiles)
                    {
                        while (collection.Count > this.numberOfReportsReadOnMemoryInParallel)
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(1));
                            Logger.Warn($"Waiting for parser because file content queue already contains {collection.Count} items.");
                        }

                        if (File.Exists(reportFile))
                        {
                            int number = Interlocked.Increment(ref this.fileParserCount);
                            Logger.InfoFormat(Resources.LoadingReport, reportFile, number, reportFiles.Count);
                            Stopwatch stopWatch = new Stopwatch();
                            stopWatch.Start();
                            collection.Add(new Tuple<string, string>(reportFile, File.ReadAllText(reportFile)));
                            stopWatch.Stop();
                            Interlocked.Exchange(ref average, average += stopWatch.ElapsedMilliseconds);
                            Logger.InfoFormat(Resources.FinishedLoadingReport, reportFile, number, reportFiles.Count, stopWatch.ElapsedMilliseconds / 1000d);
                        }
                    }
                }
                finally
                {
                    Logger.Info($"Finished loading files in memory...");
                    collection.CompleteAdding();
                }

                return this.fileParserCount > 0 ? average / this.fileParserCount : 0;
            });
        }

        /// <summary>
        /// Creates the consumer which merges the results.
        /// </summary>
        /// <param name="collection">The collection to pick results from.</param>
        /// <returns>The task containing merged results or null in case this consumer has not merged any result.</returns>
        private Task<ParserResult> CreateConsumer(BlockingCollection<ParserResult> collection)
        {
            return Task.Factory.StartNew(() =>
            {
                ParserResult result = new ParserResult();
                foreach (ParserResult parserResult in collection.GetConsumingEnumerable())
                {
                    this.MergeResults(result, parserResult);
                }

                return result;
            });
        }

        /// <summary>
        /// Merges the result1 with the result2.
        /// </summary>
        /// <param name="result1">The first result.</param>
        /// <param name="result2">The second result.</param>
        private void MergeResults(ParserResult result1, ParserResult result2)
        {
            Interlocked.Increment(ref this.mergeCount);
            int currentProgress = this.mergeCount;
            Logger.Info($"Start merge result {currentProgress}");
            result1.Merge(result2);
            Logger.Info($"Finished merge result {currentProgress}");
        }

        /// <summary>
        /// Creates the producer which parses the files in parallel and creates parser results out of it.
        /// </summary>
        /// <param name="fileContentCollection">The blocking collection to get the file content from.</param>
        /// <param name="collection">The block collection to add the parsed results to.</param>
        /// <param name="fileCount">The overall file count.</param>
        /// <returns>The Task.</returns>
        private Task<long> CreateFileParserProducer(BlockingCollection<Tuple<string, string>> fileContentCollection, BlockingCollection<ParserResult> collection, int fileCount)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    int count = 0;
                    long average = 0;
                    foreach (Tuple<string, string> content in fileContentCollection.GetConsumingEnumerable())
                    {
                        string reportFile = content.Item1;
                        string fileContent = content.Item2;
                        try
                        {
                            Logger.Info($"Start parsing content of {reportFile}...");
                            Stopwatch stopWatch = new Stopwatch();
                            stopWatch.Start();
                            IEnumerable<ParserResult> parserResults = fileContent.Trim().StartsWith("<")
                                ? this.ParseXmlFile(XDocument.Parse(fileContent))
                                : this.ParseTextFile(fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None)).ToList();
                            foreach (ParserResult parserResult in parserResults)
                            {
                                collection.Add(parserResult);
                            }

                            stopWatch.Stop();
                            count++;
                            average += stopWatch.ElapsedMilliseconds;
                            Logger.Info($"Finished parsing content of {reportFile} in {stopWatch.ElapsedMilliseconds / 1000d:f1} seconds...");
                        }
                        catch (Exception ex) when (!(ex is UnsupportedParserException))
                        {
                            Logger.ErrorFormat(" " + Resources.ErrorDuringReadingReport, reportFile, GetHumanReadableFileSize(reportFile), ex.GetExceptionMessageForDisplay());
                        }
                    }

                    return count > 0 ? average / count : 0;
                }
                finally
                {
                    Logger.Info($"Parsing of {fileCount} files completed.");
                }
            });
        }

        /// <summary>
        /// Tries to initiate the correct parsers for the given XML report. The result is merged into the given result.
        /// The report may contain several reports. For every report an extra parser is initiated.
        /// </summary>
        /// <param name="report">The report file to parse.</param>
        /// <returns>The parser result.</returns>
        private IEnumerable<ParserResult> ParseXmlFile(XDocument report)
        {
            if (report.Descendants("PartCoverReport").Any())
            {
                throw new UnsupportedParserException(Resources.ErrorPartCover);
            }

            var elements = report.Descendants("CoverageSession").ToArray();

            if (elements.Length > 0)
            {
                foreach (var item in elements)
                {
                    Logger.Debug(" " + Resources.PreprocessingReport);
                    new OpenCoverReportPreprocessor().Execute(item);

                    Logger.DebugFormat(" " + Resources.InitiatingParser, "OpenCover");
                    yield return new OpenCoverParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                }

                yield break;
            }

            elements = report.Descendants("Root").Where(e => e.Attribute("ReportType") != null && e.Attribute("ReportType").Value == "DetailedXml").ToArray();

            if (elements.Length > 0)
            {
                foreach (var item in elements)
                {
                    Logger.Debug(" " + Resources.PreprocessingReport);
                    new DotCoverReportPreprocessor().Execute(item);

                    Logger.DebugFormat(" " + Resources.InitiatingParser, "dotCover");
                    yield return new DotCoverParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                }

                yield break;
            }

            elements = report.Descendants("report").Where(e => e.Attribute("name") != null).ToArray();

            if (elements.Length > 0)
            {
                foreach (var item in elements)
                {
                    Logger.Debug(" " + Resources.PreprocessingReport);
                    new JaCoCoReportPreprocessor(this.sourceDirectories).Execute(item);

                    Logger.DebugFormat(" " + Resources.InitiatingParser, "JaCoCo");
                    var result = new JaCoCoParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);

                    foreach (var sourceDirectory in this.sourceDirectories)
                    {
                        result.AddSourceDirectory(sourceDirectory);
                    }

                    yield return result;
                }

                yield break;
            }

            var ns = report.Root.GetDefaultNamespace();
            elements = report.Descendants(ns + "coverage").ToArray();

            if (elements.Length > 0)
            {
                foreach (var item in elements)
                {
                    if (item.Attribute("profilerVersion") != null)
                    {
                        Logger.DebugFormat(" " + Resources.InitiatingParser, "NCover");
                        yield return new NCoverParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                    }
                    else if (item.Attribute("clover") != null)
                    {
                        Logger.Debug(" " + Resources.PreprocessingReport);
                        new CloverReportPreprocessor(this.sourceDirectories).Execute(item);

                        Logger.DebugFormat(" " + Resources.InitiatingParser, "Clover");
                        var result = new CloverParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);

                        foreach (var sourceDirectory in this.sourceDirectories)
                        {
                            result.AddSourceDirectory(sourceDirectory);
                        }

                        yield return result;
                    }
                    else if (item.Attributes().Count() > 1)
                    {
                        Logger.Debug(" " + Resources.PreprocessingReport);
                        new CoberturaReportPreprocessor().Execute(item);

                        Logger.DebugFormat(" " + Resources.InitiatingParser, "Cobertura");
                        yield return new CoberturaParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                    }
                    else
                    {
                        Logger.DebugFormat(" " + Resources.InitiatingParser, "mprof");
                        yield return new MProfParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                    }
                }

                yield break;
            }

            elements = report.Descendants("CoverageDSPriv").ToArray();

            if (elements.Length > 0)
            {
                foreach (var item in elements)
                {
                    Logger.DebugFormat(" " + Resources.InitiatingParser, "Visual Studio");
                    new VisualStudioReportPreprocessor().Execute(item);

                    yield return new VisualStudioParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                }

                yield break;
            }

            elements = report.Descendants("results").ToArray();

            if (elements.Length > 0)
            {
                foreach (var item in elements)
                {
                    if (item.Element("modules") != null)
                    {
                        Logger.DebugFormat(" " + Resources.InitiatingParser, "Dynamic Code Coverage");
                        new DynamicCodeCoverageReportPreprocessor().Execute(item);

                        yield return new DynamicCodeCoverageParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                    }
                }

                yield break;
            }
        }

        /// <summary>
        /// Tries to initiate the correct parsers for the given text based report. The result is merged into the given result.
        /// The report may contain several reports. For every report an extra parser is initiated.
        /// </summary>
        /// <param name="lines">The file's lines.</param>
        /// <returns>The parser result.</returns>
        private IEnumerable<ParserResult> ParseTextFile(string[] lines)
        {
            if (lines.Length == 0)
            {
                yield break;
            }

            if (lines[0].StartsWith("TN:") || lines[0].StartsWith("SF:"))
            {
                Logger.DebugFormat(" " + Resources.InitiatingParser, "LCov");

                yield return new LCovParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(lines);
            }
            else if (lines[0].Contains(GCovParser.SourceElementInFirstLine))
            {
                Logger.Debug(" " + Resources.PreprocessingReport);
                new GCovReportPreprocessor(this.sourceDirectories).Execute(lines);

                Logger.DebugFormat(" " + Resources.InitiatingParser, "GCov");
                var result = new GCovParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(lines);

                foreach (var sourceDirectory in this.sourceDirectories)
                {
                    result.AddSourceDirectory(sourceDirectory);
                }

                yield return result;
            }
        }

        /// <summary>
        /// Get the file size in human readable format.
        /// If size information is not available (e.g. IOException), '-' is returned.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The file size.</returns>
        private static string GetHumanReadableFileSize(string fileName)
        {
            try
            {
                long byteCount = new FileInfo(fileName).Length;

                string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

                if (byteCount == 0)
                {
                    return "0" + suffixes[0];
                }

                long bytes = Math.Abs(byteCount);
                int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
                double num = Math.Round(bytes / Math.Pow(1024, place), 1);
                return (Math.Sign(byteCount) * num).ToString() + suffixes[place];
            }
            catch (Exception)
            {
                return "-";
            }
        }
    }
}
