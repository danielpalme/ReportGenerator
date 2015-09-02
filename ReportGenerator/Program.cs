using System;
using System.Linq;
using Palmmedia.ReportGenerator.Logging;
using Palmmedia.ReportGenerator.Parser;
using Palmmedia.ReportGenerator.Properties;
using Palmmedia.ReportGenerator.Reporting;

namespace Palmmedia.ReportGenerator
{
    /// <summary>
    /// Command line access to the ReportBuilder.
    /// </summary>
    internal class Program
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
        internal static bool Execute(ReportConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (!configuration.Validate())
            {
                return false;
            }

            LoggerFactory.VerbosityLevel = configuration.VerbosityLevel;

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

        /// <summary>
        /// The main method.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Return code indicating success/failure.</returns>
        internal static int Main(string[] args)
        {
            var reportConfigurationBuilder = new ReportConfigurationBuilder(new MefReportBuilderFactory());

            if (args.Length < 2)
            {
                reportConfigurationBuilder.ShowHelp();
                return 1;
            }

            args = args.Select(a => a.EndsWith("\"", StringComparison.OrdinalIgnoreCase) ? a.TrimEnd('\"') + "\\" : a).ToArray();

            ReportConfiguration configuration = reportConfigurationBuilder.Create(args);

            return Execute(configuration) ? 0 : 1;
        }
    }
}
