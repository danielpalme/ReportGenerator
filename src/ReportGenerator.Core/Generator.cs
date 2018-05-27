using System;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Properties;
using Palmmedia.ReportGenerator.Core.Reporting;
using Palmmedia.ReportGenerator.Core.Reporting.History;

namespace Palmmedia.ReportGenerator.Core
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
        public bool GenerateReport(IReportConfiguration reportConfiguration)
        {
            if (reportConfiguration == null)
            {
                throw new ArgumentNullException(nameof(reportConfiguration));
            }

            try
            {
                var reportContext = new ReportContext(reportConfiguration);
                var pluginLoader = new ReflectionPluginLoader();

                IReportBuilderFactory reportBuilderFactory = new ReportBuilderFactory(pluginLoader);

                // Set log level before validation is performed
                LoggerFactory.VerbosityLevel = reportContext.ReportConfiguration.VerbosityLevel;

                if (!new ReportConfigurationValidator(reportBuilderFactory).Validate(reportContext.ReportConfiguration))
                {
#if DEBUG
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        Console.ReadKey();
                    }
#endif

                    return false;
                }

                var stopWatch = new System.Diagnostics.Stopwatch();
                stopWatch.Start();
                DateTime executionTime = DateTime.Now;

                var parserResult = ParserFactory.ParseFiles(reportContext.ReportConfiguration.ReportFiles);

                var overallHistoricCoverages = new System.Collections.Generic.List<Parser.Analysis.HistoricCoverage>();
                var historyStorage = new HistoryStorageFactory(pluginLoader).GetHistoryStorage(reportContext.ReportConfiguration);

                if (historyStorage != null)
                {
                    new HistoryParser(historyStorage)
                            .ApplyHistoricCoverage(parserResult.Assemblies, overallHistoricCoverages);

                    reportContext.OverallHistoricCoverages = overallHistoricCoverages;
                }

                var filteredAssemblies = new Reporting.ReportGenerator(
                    parserResult,
                    new DefaultFilter(reportContext.ReportConfiguration.AssemblyFilters),
                    new DefaultFilter(reportContext.ReportConfiguration.ClassFilters),
                    new DefaultFilter(reportContext.ReportConfiguration.FileFilters),
                    reportBuilderFactory.GetReportBuilders(reportContext))
                        .CreateReport(reportContext.ReportConfiguration.HistoryDirectory != null, overallHistoricCoverages, executionTime, reportContext.ReportConfiguration.Tag);

                if (historyStorage != null)
                {
                    new HistoryReportGenerator(historyStorage)
                            .CreateReport(filteredAssemblies, executionTime, reportContext.ReportConfiguration.Tag);
                }

                stopWatch.Stop();
                Logger.InfoFormat(Resources.ReportGenerationTook, stopWatch.ElapsedMilliseconds / 1000d);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.GetExceptionMessageForDisplay());

#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Console.ReadKey();
                }
#endif

                return false;
            }
        }
    }
}
