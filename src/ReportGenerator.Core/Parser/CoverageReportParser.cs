using System;
using System.Collections.Generic;
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
        /// Lock object to prevent parallel merges.
        /// </summary>
        private readonly object mergeLock = new object();

        /// <summary>
        /// The number reports that are parsed and processed in parallel.
        /// </summary>
        private readonly int numberOfReportsParsedInParallel;

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
        /// Initializes a new instance of the <see cref="CoverageReportParser" /> class.
        /// </summary>
        /// <param name="numberOfReportsParsedInParallel">The number reports that are parsed and processed in parallel.</param>
        /// <param name="sourceDirectories">The source directories.</param>
        /// <param name="assemblyFilter">The assembly filter.</param>
        /// <param name="classFilter">The class filter.</param>
        /// <param name="fileFilter">The file filter.</param>
        public CoverageReportParser(int numberOfReportsParsedInParallel, IEnumerable<string> sourceDirectories, IFilter assemblyFilter, IFilter classFilter, IFilter fileFilter)
        {
            this.numberOfReportsParsedInParallel = Math.Max(1, numberOfReportsParsedInParallel);
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

            var result = new ParserResult();

            int counter = 0;

            try
            {
                Parallel.ForEach(
                reportFiles,
                new ParallelOptions() { MaxDegreeOfParallelism = this.numberOfReportsParsedInParallel },
                reportFile =>
                {
                    int number = Interlocked.Increment(ref counter);
                    Logger.InfoFormat(Resources.LoadingReport, reportFile, number, reportFiles.Count);

                    try
                    {
                        var report = XDocument.Load(reportFile);

                        foreach (var parserResult in this.ParseFile(report))
                        {
                            lock (this.mergeLock)
                            {
                                result.Merge(parserResult);
                            }
                        }
                    }
                    catch (Exception ex) when (!(ex is UnsupportedParserException))
                    {
                        Logger.ErrorFormat(" " + Resources.ErrorDuringReadingReport, reportFile, GetHumanReadableFileSize(reportFile), ex.GetExceptionMessageForDisplay());
                    }
                });
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

            return result;
        }

        /// <summary>
        /// Tries to initiate the correct parsers for the given report. The result is merged into the given result.
        /// The report may contain several reports. For every report an extra parser is initiated.
        /// </summary>
        /// <param name="report">The report file to parse.</param>
        /// <returns>The parser result.</returns>
        private IEnumerable<ParserResult> ParseFile(XContainer report)
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

            elements = report.Descendants("coverage").ToArray();

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
