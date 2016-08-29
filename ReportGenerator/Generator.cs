using System;
using Palmmedia.ReportGenerator.Logging;
using Palmmedia.ReportGenerator.Parser;
using Palmmedia.ReportGenerator.Properties;
using Palmmedia.ReportGenerator.Reporting;

namespace Palmmedia.ReportGenerator
{
    /// <summary>
    /// The report generator implementation.
    /// </summary>
    public class Generator : IReportGenerator
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(Program));

        /// <summary>
        /// Executes the report generation.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <returns><c>true</c> if report was generated successfully; otherwise <c>false</c>.</returns>
        public bool GenerateReport(ReportConfiguration reportConfiguration)
        {
            if (reportConfiguration == null)
            {
                throw new ArgumentNullException(nameof(reportConfiguration));
            }

            // set it before validate
            LoggerFactory.VerbosityLevel = reportConfiguration.VerbosityLevel;
            if (!reportConfiguration.Validate())
            {
                return false;
            }

            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            DateTime executionTime = DateTime.Now;

            var parser = ParserFactory.CreateParser(reportConfiguration.ReportFiles, reportConfiguration.SourceDirectories);

            if (reportConfiguration.HistoryDirectory != null)
            {
                new Reporting.HistoryParser(
                    parser.Assemblies,
                    reportConfiguration.HistoryDirectory)
                        .ApplyHistoricCoverage();
            }

            new Reporting.ReportGenerator(
                parser,
                new DefaultFilter(reportConfiguration.AssemblyFilters),
                new DefaultFilter(reportConfiguration.ClassFilters),
                reportConfiguration.ReportBuilderFactory.GetReportBuilders(reportConfiguration.TargetDirectory, reportConfiguration.ReportTypes))
                    .CreateReport(reportConfiguration.HistoryDirectory != null, executionTime);

            if (reportConfiguration.HistoryDirectory != null)
            {
                new Reporting.HistoryReportGenerator(
                    parser,
                    reportConfiguration.HistoryDirectory)
                        .CreateReport(executionTime);
            }

            stopWatch.Stop();
            Logger.InfoFormat(Resources.ReportGenerationTook, stopWatch.ElapsedMilliseconds / 1000d);

            return true;
        }
    }
}
