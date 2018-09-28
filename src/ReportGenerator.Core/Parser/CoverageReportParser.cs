using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    internal class CoverageReportParser
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(CoverageReportParser));

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
        /// <param name="assemblyFilter">The assembly filter.</param>
        /// <param name="classFilter">The class filter.</param>
        /// <param name="fileFilter">The file filter.</param>
        internal CoverageReportParser(IFilter assemblyFilter, IFilter classFilter, IFilter fileFilter)
        {
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
        internal ParserResult ParseFiles(IReadOnlyCollection<string> reportFiles)
        {
            if (reportFiles == null)
            {
                throw new ArgumentNullException(nameof(reportFiles));
            }

            var result = new ParserResult();

            int counter = 0;

            foreach (var report in reportFiles)
            {
                Logger.InfoFormat(Resources.LoadingReport, report, ++counter, reportFiles.Count);
                this.ParseFile(report, result);
            }

            return result;
        }

        /// <summary>
        /// Tries to initiate the correct parsers for the given report. The result is merged into the given result.
        /// The report may contain several reports. For every report an extra parser is initiated.
        /// </summary>
        /// <param name="reportFile">The report file to parse.</param>
        /// <param name="result">The current parser result.</param>
        private void ParseFile(string reportFile, ParserResult result)
        {
            XContainer report = null;
            try
            {
                report = XDocument.Load(reportFile);

                if (report.Descendants("CoverageSession").Any())
                {
                    foreach (var item in report.Descendants("CoverageSession"))
                    {
                        Logger.Debug(" " + Resources.PreprocessingReport);
                        new OpenCoverReportPreprocessor(item).Execute();
                        Logger.DebugFormat(" " + Resources.InitiatingParser, "OpenCover");

                        var newResult = new OpenCoverParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                        result.Merge(newResult);
                    }
                }
                else if (report.Descendants("Root").Where(e => e.Attribute("ReportType") != null && e.Attribute("ReportType").Value == "DetailedXml").Any())
                {
                    foreach (var item in report.Descendants("Root").Where(e => e.Attribute("ReportType") != null && e.Attribute("ReportType").Value == "DetailedXml"))
                    {
                        Logger.Debug(" " + Resources.PreprocessingReport);
                        new DotCoverReportPreprocessor(item).Execute();
                        Logger.DebugFormat(" " + Resources.InitiatingParser, "dotCover");

                        var newResult = new DotCoverParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                        result.Merge(newResult);
                    }
                }
                else if (report.Descendants("PartCoverReport").Any())
                {
                    throw new UnsupportedParserException(Resources.ErrorPartCover);
                }
                else if (report.Descendants("coverage").Any())
                {
                    foreach (var item in report.Descendants("coverage"))
                    {
                        if (item.Attribute("profilerVersion") != null)
                        {
                            Logger.DebugFormat(" " + Resources.InitiatingParser, "NCover");

                            var newResult = new NCoverParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                            result.Merge(newResult);
                        }
                        else if (item.Attributes().Count() > 1)
                        {
                            Logger.Debug(" " + Resources.PreprocessingReport);
                            new CoberturaReportPreprocessor(item).Execute();
                            Logger.DebugFormat(" " + Resources.InitiatingParser, "Cobertura");

                            var newResult = new CoberturaParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                            result.Merge(newResult);
                        }
                        else
                        {
                            Logger.DebugFormat(" " + Resources.InitiatingParser, "mprof");

                            var newResult = new MProfParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                            result.Merge(newResult);
                        }
                    }
                }
                else if (report.Descendants("CoverageDSPriv").Any())
                {
                    foreach (var item in report.Descendants("CoverageDSPriv"))
                    {
                        Logger.DebugFormat(" " + Resources.InitiatingParser, "Visual Studio");
                        new VisualStudioReportPreprocessor(item).Execute();

                        var newResult = new VisualStudioParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                        result.Merge(newResult);
                    }
                }
                else if (report.Descendants("results").Any())
                {
                    foreach (var item in report.Descendants("results"))
                    {
                        if (item.Element("modules") != null)
                        {
                            Logger.DebugFormat(" " + Resources.InitiatingParser, "Dynamic Code Coverage");
                            new DynamicCodeCoverageReportPreprocessor(item).Execute();

                            var newResult = new DynamicCodeCoverageParser(this.assemblyFilter, this.classFilter, this.fileFilter).Parse(item);
                            result.Merge(newResult);
                        }
                    }
                }
            }
            catch (Exception ex) when (!(ex is UnsupportedParserException))
            {
                Logger.ErrorFormat(" " + Resources.ErrorDuringReadingReport, reportFile, GetHumanReadableFileSize(reportFile), ex.GetExceptionMessageForDisplay());
            }
        }

        /// <summary>
        /// Get the file size in human readable format.
        /// If size information is not available (e.g. IOException), '-' is returned.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The file size.</returns>
        private static String GetHumanReadableFileSize(string fileName)
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
