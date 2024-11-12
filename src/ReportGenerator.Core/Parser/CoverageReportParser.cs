using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Licensing;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using Palmmedia.ReportGenerator.Core.Parser.Preprocessing;
using Palmmedia.ReportGenerator.Core.Properties;
using Palmmedia.ReportGenerator.Core.Reporting;

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
        /// The report context.
        /// </summary>
        private readonly IReportContext reportContext;

        /// <summary>
        /// The number reports that are parsed and processed in parallel.
        /// </summary>
        private readonly int numberOfReportsParsedInParallel;

        /// <summary>
        /// The number reports that are merged in parallel.
        /// </summary>
        private readonly int numberOfReportsMergedInParallel;

        /// <summary>
        /// Indicates whether test projects should be included.
        /// </summary>
        private readonly bool excludeTestProjects;

        /// <summary>
        /// The source directories.
        /// </summary>
        private readonly IEnumerable<string> sourceDirectories;

        /// <summary>
        /// The default assembly name.
        /// </summary>
        private readonly string defaultAssemblyName = "Default";

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
        /// Indicates the raw mode is not supported under current license.
        /// </summary>
        private bool rawModeProhibited;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverageReportParser" /> class.
        /// </summary>
        /// <param name="numberOfReportsParsedInParallel">The number reports that are parsed and processed in parallel.</param>
        /// <param name="numberOfReportsMergedInParallel">The number reports that are merged in parallel.</param>
        /// <param name="sourceDirectories">The source directories.</param>
        /// <param name="assemblyFilter">The assembly filter.</param>
        /// <param name="classFilter">The class filter.</param>
        /// <param name="fileFilter">The file filter.</param>
        public CoverageReportParser(
            int numberOfReportsParsedInParallel,
            int numberOfReportsMergedInParallel,
            IEnumerable<string> sourceDirectories,
            IFilter assemblyFilter,
            IFilter classFilter,
            IFilter fileFilter)
        {
            this.numberOfReportsParsedInParallel = Math.Max(1, numberOfReportsParsedInParallel);
            this.numberOfReportsMergedInParallel = Math.Max(1, numberOfReportsMergedInParallel);
            this.sourceDirectories = sourceDirectories ?? throw new ArgumentNullException(nameof(sourceDirectories));
            this.assemblyFilter = assemblyFilter ?? throw new ArgumentNullException(nameof(assemblyFilter));
            this.classFilter = classFilter ?? throw new ArgumentNullException(nameof(classFilter));
            this.fileFilter = fileFilter ?? throw new ArgumentNullException(nameof(fileFilter));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverageReportParser" /> class.
        /// </summary>
        /// <param name="numberOfReportsParsedInParallel">The number reports that are parsed and processed in parallel.</param>
        /// <param name="numberOfReportsMergedInParallel">The number reports that are merged in parallel.</param>
        /// <param name="excludeTestProjects">Indicates whether test projects should be included.</param>
        /// <param name="sourceDirectories">The source directories.</param>
        /// <param name="assemblyFilter">The assembly filter.</param>
        /// <param name="classFilter">The class filter.</param>
        /// <param name="fileFilter">The file filter.</param>
        public CoverageReportParser(
            int numberOfReportsParsedInParallel,
            int numberOfReportsMergedInParallel,
            bool excludeTestProjects,
            IEnumerable<string> sourceDirectories,
            IFilter assemblyFilter,
            IFilter classFilter,
            IFilter fileFilter)
        {
            this.numberOfReportsParsedInParallel = Math.Max(1, numberOfReportsParsedInParallel);
            this.numberOfReportsMergedInParallel = Math.Max(1, numberOfReportsMergedInParallel);
            this.excludeTestProjects = excludeTestProjects;
            this.sourceDirectories = sourceDirectories ?? throw new ArgumentNullException(nameof(sourceDirectories));
            this.assemblyFilter = assemblyFilter ?? throw new ArgumentNullException(nameof(assemblyFilter));
            this.classFilter = classFilter ?? throw new ArgumentNullException(nameof(classFilter));
            this.fileFilter = fileFilter ?? throw new ArgumentNullException(nameof(fileFilter));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverageReportParser" /> class.
        /// </summary>
        /// <param name="numberOfReportsParsedInParallel">The number reports that are parsed and processed in parallel.</param>
        /// <param name="numberOfReportsMergedInParallel">The number reports that are merged in parallel.</param>
        /// <param name="excludeTestProjects">Indicates whether test projects should be included.</param>
        /// <param name="sourceDirectories">The source directories.</param>
        /// <param name="defaultAssemblyName">The default assembly name.</param>
        /// <param name="assemblyFilter">The assembly filter.</param>
        /// <param name="classFilter">The class filter.</param>
        /// <param name="fileFilter">The file filter.</param>
        public CoverageReportParser(
            int numberOfReportsParsedInParallel,
            int numberOfReportsMergedInParallel,
            bool excludeTestProjects,
            string defaultAssemblyName,
            IEnumerable<string> sourceDirectories,
            IFilter assemblyFilter,
            IFilter classFilter,
            IFilter fileFilter)
        {
            this.numberOfReportsParsedInParallel = Math.Max(1, numberOfReportsParsedInParallel);
            this.numberOfReportsMergedInParallel = Math.Max(1, numberOfReportsMergedInParallel);
            this.excludeTestProjects = excludeTestProjects;
            this.defaultAssemblyName = defaultAssemblyName ?? throw new ArgumentNullException(nameof(defaultAssemblyName));
            this.sourceDirectories = sourceDirectories ?? throw new ArgumentNullException(nameof(sourceDirectories));
            this.assemblyFilter = assemblyFilter ?? throw new ArgumentNullException(nameof(assemblyFilter));
            this.classFilter = classFilter ?? throw new ArgumentNullException(nameof(classFilter));
            this.fileFilter = fileFilter ?? throw new ArgumentNullException(nameof(fileFilter));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoverageReportParser" /> class.
        /// </summary>
        /// <param name="reportContext">The report context.</param>
        public CoverageReportParser(IReportContext reportContext)
        {
            this.reportContext = reportContext ?? throw new ArgumentNullException(nameof(reportContext));

            this.numberOfReportsParsedInParallel = Math.Max(1, reportContext.Settings.NumberOfReportsParsedInParallel);
            this.numberOfReportsMergedInParallel = Math.Max(1, reportContext.Settings.NumberOfReportsMergedInParallel);
            this.excludeTestProjects = reportContext.Settings.ExcludeTestProjects;
            this.defaultAssemblyName = reportContext.Settings.DefaultAssemblyName ?? throw new ArgumentNullException(nameof(reportContext.Settings.DefaultAssemblyName));
            this.sourceDirectories = reportContext.ReportConfiguration.SourceDirectories ?? throw new ArgumentNullException(nameof(reportContext.ReportConfiguration.SourceDirectories));
            this.assemblyFilter = new DefaultFilter(reportContext.ReportConfiguration.AssemblyFilters);
            this.classFilter = new DefaultFilter(reportContext.ReportConfiguration.ClassFilters);
            this.fileFilter = new DefaultFilter(reportContext.ReportConfiguration.FileFilters, true);
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

            if (this.reportContext != null
                && this.reportContext.Settings.RawMode
                && this.reportContext.ReportConfiguration.License.DetermineLicenseType() != LicenseType.Pro)
            {
                Logger.Warn(Resources.RawModeProVersion);
                this.rawModeProhibited = true;
            }

            List<Task<ParserResult>> consumers = new List<Task<ParserResult>>();

            try
            {
                using (BlockingCollection<ParserResult> blockingCollection = new BlockingCollection<ParserResult>())
                {
                    foreach (var item in Enumerable.Range(0, this.numberOfReportsMergedInParallel))
                    {
                        consumers.Add(this.CreateConsumer(blockingCollection));
                    }

                    Task producer = this.CreateProducer(reportFiles, blockingCollection);
                    Task.WaitAll(consumers.Concat(new[] { producer }).ToArray());
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
                }

                throw;
            }

            List<ParserResult> results = consumers.Select(t => t.Result).ToList();
            ParserResult finalResult = results.First();
            foreach (ParserResult toBeMerged in results.Skip(1))
            {
                this.MergeResults(finalResult, toBeMerged);
            }

            return finalResult;
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
            Logger.DebugFormat(Resources.StartingMergingResult, currentProgress);
            result1.Merge(result2);
            Logger.DebugFormat(Resources.FinishedMergingResult, currentProgress);
        }

        /// <summary>
        /// Creates the producer which parses the files in parallel and creates parser results out of it.
        /// </summary>
        /// <param name="reportFiles">The files to parse.</param>
        /// <param name="collection">The block collection to add the parsed results to.</param>
        /// <returns>The Task.</returns>
        private Task CreateProducer(IReadOnlyCollection<string> reportFiles, BlockingCollection<ParserResult> collection)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    int counter = 0;
                    Parallel.ForEach(
                    reportFiles,
                    new ParallelOptions { MaxDegreeOfParallelism = this.numberOfReportsParsedInParallel },
                    reportFile =>
                    {
                        int number = Interlocked.Increment(ref counter);
                        Logger.DebugFormat(Resources.LoadingReport, reportFile, number, reportFiles.Count);
                        try
                        {
                            bool isXml = false;
                            using (var sr = File.OpenText(reportFile))
                            {
                                // We need to read first non-space char in the file
                                var buf = new char[120];
                                while (sr.Read(buf, 0, buf.Length) > 0)
                                {
                                    string block = new string(buf).TrimStart();
                                    if (block.Length > 0)
                                    {
                                        isXml = block.StartsWith("<");
                                        break;
                                    }
                                }
                            }

                            List<ParserResult> parserResults = isXml
                                ? this.ParseXmlFile(reportFile).ToList()
                                : this.ParseTextFile(File.ReadAllLines(reportFile)).ToList();
                            foreach (ParserResult parserResult in parserResults)
                            {
                                collection.Add(parserResult);
                            }

                            if (!parserResults.Any() && reportFile.EndsWith(".coverage", StringComparison.OrdinalIgnoreCase))
                            {
                                Logger.WarnFormat(Resources.ErrorCoverageFormat, reportFile);
                            }

                            Logger.DebugFormat(Resources.FinishedParsingFile, reportFile, number, reportFiles.Count);
                        }
                        catch (Exception ex) when (!(ex is UnsupportedParserException))
                        {
                            Logger.ErrorFormat(Resources.ErrorDuringReadingReport, reportFile, GetHumanReadableFileSize(reportFile), ex.GetExceptionMessageForDisplay());
                        }
                    });
                }
                finally
                {
                    Logger.DebugFormat(Resources.ParsingCompleted, reportFiles.Count);
                    collection.CompleteAdding();
                }
            });
        }

        /// <summary>
        /// Load elements in memory balanced manner.
        /// </summary>
        /// <param name="filePath">The filepath of the covergae file to load.</param>
        /// <param name="elementName">The name of the elemens to load.</param>
        /// <returns>The elements matchig the name.</returns>
        private IEnumerable<XElement> GetXElements(string filePath, string elementName)
        {
            var readerSettings = new XmlReaderSettings() { DtdProcessing = DtdProcessing.Parse, XmlResolver = null };
            using (XmlReader reader = XmlReader.Create(filePath, readerSettings))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element &&
                        reader.Name == elementName)
                    {
                        if (XNode.ReadFrom(reader) is XElement element)
                        {
                            yield return element;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tries to initiate the correct parsers for the given XML report. The result is merged into the given result.
        /// The report may contain several reports. For every report an extra parser is initiated.
        /// </summary>
        /// <param name="filePath">The report file path to parse.</param>
        /// <returns>The parser result.</returns>
        private IEnumerable<ParserResult> ParseXmlFile(string filePath)
        {
            if (this.GetXElements(filePath, "PartCoverReport").Any())
            {
                throw new UnsupportedParserException(Resources.ErrorPartCover);
            }

            var elements = this.GetXElements(filePath, "CoverageSession").ToArray();

            if (elements.Length > 0)
            {
                foreach (var item in elements)
                {
                    Logger.Debug(Resources.PreprocessingReport);
                    new OpenCoverReportPreprocessor().Execute(item);

                    Logger.DebugFormat(Resources.InitiatingParser, "OpenCover");
                    yield return new OpenCoverParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                }

                yield break;
            }

            elements = this.GetXElements(filePath, "Root").Where(e => e.Attribute("ReportType") != null).ToArray();

            if (elements.Length > 0)
            {
                foreach (var item in elements)
                {
                    Logger.Debug(Resources.PreprocessingReport);
                    new DotCoverReportPreprocessor().Execute(item);

                    Logger.DebugFormat(Resources.InitiatingParser, "dotCover");
                    yield return new DotCoverParser(this.assemblyFilter, this.classFilter, this.fileFilter)
                    {
                        RawMode = !this.rawModeProhibited && this.reportContext?.Settings.RawMode == true
                    }.Parse(item);
                }

                yield break;
            }

            elements = this.GetXElements(filePath, "report").Where(e => e.Attribute("name") != null).ToArray();

            if (elements.Length > 0)
            {
                foreach (var item in elements)
                {
                    Logger.Debug(Resources.PreprocessingReport);
                    new JaCoCoReportPreprocessor(this.sourceDirectories).Execute(item);

                    Logger.DebugFormat(Resources.InitiatingParser, "JaCoCo");
                    var result = new JaCoCoParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);

                    foreach (var sourceDirectory in this.sourceDirectories)
                    {
                        result.AddSourceDirectory(sourceDirectory);
                    }

                    yield return result;
                }

                yield break;
            }

            elements = this.GetXElements(filePath, "coverage").ToArray();

            if (elements.Length > 0)
            {
                foreach (var item in elements)
                {
                    if (item.Attribute("profilerVersion") != null)
                    {
                        Logger.DebugFormat(Resources.InitiatingParser, "NCover");
                        yield return new NCoverParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                    }
                    else if (item.Attribute("clover") != null || item.Attribute("generated") != null)
                    {
                        Logger.Debug(Resources.PreprocessingReport);
                        new CloverReportPreprocessor(this.sourceDirectories, this.defaultAssemblyName).Execute(item);

                        Logger.DebugFormat(Resources.InitiatingParser, "Clover");
                        var result = new CloverParser(this.assemblyFilter, this.classFilter, this.fileFilter, this.excludeTestProjects).Parse(item);

                        foreach (var sourceDirectory in this.sourceDirectories)
                        {
                            result.AddSourceDirectory(sourceDirectory);
                        }

                        yield return result;
                    }
                    else if (item.Attributes().Count() > 1
                        || item.Elements("packages").Any())
                    {
                        Logger.Debug(Resources.PreprocessingReport);
                        new CoberturaReportPreprocessor().Execute(item);

                        Logger.DebugFormat(Resources.InitiatingParser, "Cobertura");
                        yield return new CoberturaParser(this.assemblyFilter, this.classFilter, this.fileFilter)
                        {
                            RawMode = !this.rawModeProhibited && this.reportContext?.Settings.RawMode == true
                        }.Parse(item);
                    }
                    else
                    {
                        Logger.DebugFormat(Resources.InitiatingParser, "mprof");
                        yield return new MProfParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                    }
                }

                yield break;
            }

            elements = this.GetXElements(filePath, "CoverageDSPriv").ToArray();

            if (elements.Length > 0)
            {
                foreach (var item in elements)
                {
                    Logger.DebugFormat(Resources.InitiatingParser, "Visual Studio");
                    new VisualStudioReportPreprocessor().Execute(item);

                    yield return new VisualStudioParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                }

                yield break;
            }

            elements = this.GetXElements(filePath, "results").ToArray();

            if (elements.Length > 0)
            {
                foreach (var item in elements)
                {
                    if (item.Element("modules") != null)
                    {
                        Logger.DebugFormat(Resources.InitiatingParser, "Dynamic Code Coverage");
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
                Logger.DebugFormat(Resources.InitiatingParser, "LCov");

                yield return new LCovParser(this.assemblyFilter, this.classFilter, this.fileFilter, this.defaultAssemblyName).Parse(lines);
            }
            else if (lines[0].Contains(GCovParser.SourceElementInFirstLine))
            {
                Logger.Debug(Resources.PreprocessingReport);
                new GCovReportPreprocessor(this.sourceDirectories).Execute(lines);

                Logger.DebugFormat(Resources.InitiatingParser, "GCov");
                var result = new GCovParser(this.assemblyFilter, this.classFilter, this.fileFilter, this.defaultAssemblyName).Parse(lines);

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
