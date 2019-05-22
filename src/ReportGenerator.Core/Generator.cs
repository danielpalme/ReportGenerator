using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Parser.FileReading;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using Palmmedia.ReportGenerator.Core.Plugin;
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
                var configuration = this.GetConfiguration();

                var settings = new Settings();
                configuration.GetSection("settings").Bind(settings);

                var riskHotspotsAnalysisThresholds = new RiskHotspotsAnalysisThresholds();
                configuration.GetSection("riskHotspotsAnalysisThresholds").Bind(riskHotspotsAnalysisThresholds);

                return this.GenerateReport(reportConfiguration, settings, riskHotspotsAnalysisThresholds);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.GetExceptionMessageForDisplay());
                Logger.Error(ex.StackTrace);

#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Console.ReadKey();
                }
#endif

                return false;
            }
        }

        /// <summary>
        /// Executes the report generation.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="riskHotspotsAnalysisThresholds">The risk hotspots analysis thresholds.</param>
        /// <returns><c>true</c> if report was generated successfully; otherwise <c>false</c>.</returns>
        public bool GenerateReport(
            IReportConfiguration reportConfiguration,
            Settings settings,
            RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds)
        {
            if (reportConfiguration == null)
            {
                throw new ArgumentNullException(nameof(reportConfiguration));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (riskHotspotsAnalysisThresholds == null)
            {
                throw new ArgumentNullException(nameof(riskHotspotsAnalysisThresholds));
            }

            try
            {
                var reportContext = new ReportContext(reportConfiguration, settings);

                var pluginLoader = new ReflectionPluginLoader(reportConfiguration.Plugins);

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

                var parserResult = new CoverageReportParser(
                    reportContext.Settings.NumberOfReportsParsedInParallel,
                    reportConfiguration.SourceDirectories,
                    new DefaultFilter(reportContext.ReportConfiguration.AssemblyFilters),
                    new DefaultFilter(reportContext.ReportConfiguration.ClassFilters),
                    new DefaultFilter(reportContext.ReportConfiguration.FileFilters))
                        .ParseFiles(reportContext.ReportConfiguration.ReportFiles);

                Logger.DebugFormat(Resources.ReportParsingTook, stopWatch.ElapsedMilliseconds / 1000d);

                reportContext.RiskHotspotAnalysisResult = new RiskHotspotsAnalyzer(riskHotspotsAnalysisThresholds, reportContext.Settings.DisableRiskHotspots)
                    .PerformRiskHotspotAnalysis(parserResult.Assemblies);

                var overallHistoricCoverages = new System.Collections.Generic.List<Parser.Analysis.HistoricCoverage>();
                var historyStorage = new HistoryStorageFactory(pluginLoader).GetHistoryStorage(reportContext.ReportConfiguration);

                if (historyStorage != null)
                {
                    new HistoryParser(historyStorage, reportContext.Settings.MaximumNumberOfHistoricCoverageFiles)
                            .ApplyHistoricCoverage(parserResult.Assemblies, overallHistoricCoverages);

                    reportContext.OverallHistoricCoverages = overallHistoricCoverages;
                }

                new Reporting.ReportGenerator(
                    new CachingFileReader(reportContext.Settings.CachingDuringOfRemoteFilesInMinutes),
                    parserResult,
                    reportBuilderFactory.GetReportBuilders(reportContext))
                        .CreateReport(reportContext.ReportConfiguration.HistoryDirectory != null, overallHistoricCoverages, executionTime, reportContext.ReportConfiguration.Tag);

                if (historyStorage != null)
                {
                    new HistoryReportGenerator(historyStorage)
                            .CreateReport(parserResult.Assemblies, executionTime, reportContext.ReportConfiguration.Tag);
                }

                stopWatch.Stop();
                Logger.InfoFormat(Resources.ReportGenerationTook, stopWatch.ElapsedMilliseconds / 1000d);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.GetExceptionMessageForDisplay());
                Logger.Error(ex.StackTrace);

#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    Console.ReadKey();
                }
#endif

                return false;
            }
        }

        /// <summary>
        /// Get the <see cref="IConfigurationRoot"/>.
        /// </summary>
        /// <returns>The configuration.</returns>
        private IConfigurationRoot GetConfiguration()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(new FileInfo(this.GetType().Assembly.Location).DirectoryName)
                    .AddJsonFile("appsettings.json")
                    .AddCommandLine(Environment.GetCommandLineArgs());

                return builder.Build();
            }
            catch (FileLoadException)
            {
                // This can happen when excuted within MSBuild (dotnet msbuild): JSON configuration gets ignored
                var builder = new ConfigurationBuilder()
                    .SetBasePath(new FileInfo(this.GetType().Assembly.Location).DirectoryName)
                    .AddCommandLine(Environment.GetCommandLineArgs());

                return builder.Build();
            }
        }
    }
}
