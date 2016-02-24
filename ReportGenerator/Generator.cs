using Palmmedia.ReportGenerator.Logging;
using Palmmedia.ReportGenerator.Parser;
using Palmmedia.ReportGenerator.Properties;
using Palmmedia.ReportGenerator.Reporting;
using System;

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
        /// <param name="configuration">The configuration.</param>
        /// <returns><c>true</c> if report was generated successfully; otherwise <c>false</c>.</returns>
        public bool GenerateReport(ReportConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // set it before validate
            LoggerFactory.VerbosityLevel = configuration.VerbosityLevel;
            if (!configuration.Validate())
            {
                return false;
            }

            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            DateTime executionTime = DateTime.Now;

            var parser = ParserFactory.CreateParser(configuration.ReportFiles, configuration.SourceDirectories);

            if (configuration.HistoryDirectory != null)
            {
                new Reporting.HistoryParser(
                    parser.Assemblies,
                    configuration.HistoryDirectory)
                        .ApplyHistoricCoverage();
            }

            new Reporting.ReportGenerator(
                parser,
                new DefaultFilter(configuration.AssemblyFilters),
                new DefaultFilter(configuration.ClassFilters),
                configuration.ReportBuilderFactory.GetReportBuilders(configuration.TargetDirectory, configuration.ReportTypes))
                    .CreateReport(configuration.HistoryDirectory != null, executionTime);

            if (configuration.HistoryDirectory != null)
            {
                new Reporting.HistoryReportGenerator(
                    parser,
                    configuration.HistoryDirectory)
                        .CreateReport(executionTime);
            }

            stopWatch.Stop();
            Logger.InfoFormat(Resources.ReportGenerationTook, stopWatch.ElapsedMilliseconds / 1000d);

            return true;
        }
    }
}
