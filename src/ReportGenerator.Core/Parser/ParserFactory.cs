using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Preprocessing;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Parser
{
    /// <summary>
    /// Initiates the corresponding parser to the given report file.
    /// </summary>
    internal static class ParserFactory
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(ParserFactory));

        /// <summary>
        /// Tries to initiate the correct parsers for the given reports.
        /// </summary>
        /// <param name="reportFiles">The report files to parse.</param>
        /// <returns>
        /// The IParser instance.
        /// </returns>
        internal static IParser CreateParser(IEnumerable<string> reportFiles)
        {
            if (reportFiles == null)
            {
                throw new ArgumentNullException(nameof(reportFiles));
            }

            var multiReportParser = new MultiReportParser();

            foreach (var report in reportFiles)
            {
                foreach (var parser in GetParsersOfFile(report))
                {
                    multiReportParser.AddParser(parser);
                }
            }

            return multiReportParser;
        }

        /// <summary>
        /// Tries to initiate the correct parsers for the given report. An empty list is returned if no parser has been found.
        /// The report may contain several reports. For every report an extra parser is initiated.
        /// </summary>
        /// <param name="reportFile">The report file to parse.</param>
        /// <returns>
        /// The IParser instances or an empty list if no matching parser has been found.
        /// </returns>
        private static IEnumerable<IParser> GetParsersOfFile(string reportFile)
        {
            var parsers = new List<IParser>();

            XContainer report = null;
            try
            {
                Logger.InfoFormat(Resources.LoadingReport, reportFile);
                report = XDocument.Load(reportFile);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat(" " + Resources.ErrorDuringReadingReport, reportFile, ex.Message);
                return parsers;
            }

            if (report.Descendants("CoverageSession").Any())
            {
                foreach (var item in report.Descendants("CoverageSession"))
                {
                    Logger.Debug(" " + Resources.PreprocessingReport);
                    new OpenCoverReportPreprocessor(item).Execute();
                    Logger.DebugFormat(" " + Resources.InitiatingParser, "OpenCover");
                    parsers.Add(new OpenCoverParser(item));
                }
            }
            else if (report.Descendants("Root").Where(e => e.Attribute("ReportType") != null && e.Attribute("ReportType").Value == "DetailedXml").Any())
            {
                foreach (var item in report.Descendants("Root").Where(e => e.Attribute("ReportType") != null && e.Attribute("ReportType").Value == "DetailedXml"))
                {
                    Logger.Debug(" " + Resources.PreprocessingReport);
                    new DotCoverReportPreprocessor(item).Execute();
                    Logger.DebugFormat(" " + Resources.InitiatingParser, "dotCover");
                    parsers.Add(new DotCoverParser(item));
                }
            }
            else if (report.Descendants("PartCoverReport").Any())
            {
                throw new InvalidOperationException(Resources.ErrorPartCover);
            }
            else if (report.Descendants("coverage").Any())
            {
                foreach (var item in report.Descendants("coverage"))
                {
                    if (item.Attribute("profilerVersion") != null)
                    {
                        Logger.DebugFormat(" " + Resources.InitiatingParser, "NCover");
                        parsers.Add(new NCoverParser(item));
                    }
                    else if (item.Attributes().Count() > 1)
                    {
                        Logger.Debug(" " + Resources.PreprocessingReport);
                        new CoberturaReportPreprocessor(item).Execute();
                        Logger.DebugFormat(" " + Resources.InitiatingParser, "Cobertura");
                        parsers.Add(new CoberturaParser(item));
                    }
                    else
                    {
                        Logger.DebugFormat(" " + Resources.InitiatingParser, "mprof");
                        parsers.Add(new MProfParser(item));
                    }
                }
            }
            else if (report.Descendants("CoverageDSPriv").Any())
            {
                foreach (var item in report.Descendants("CoverageDSPriv"))
                {
                    Logger.DebugFormat(" " + Resources.InitiatingParser, "Visual Studio");
                    new VisualStudioReportPreprocessor(item).Execute();
                    parsers.Add(new VisualStudioParser(item));
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
                        parsers.Add(new DynamicCodeCoverageParser(item));
                    }
                }
            }

            return parsers;
        }
    }
}
