using System;
using System.Collections.Generic;
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
        /// Generates a report using given configuration.
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
        /// Generates a report using given configuration.
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
                var pluginLoader = new ReflectionPluginLoader(reportConfiguration.Plugins);
                IReportBuilderFactory reportBuilderFactory = new ReportBuilderFactory(pluginLoader);

                // Set log level before validation is performed
                LoggerFactory.VerbosityLevel = reportConfiguration.VerbosityLevel;

                if (!new ReportConfigurationValidator(reportBuilderFactory).Validate(reportConfiguration))
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

                var parserResult = new CoverageReportParser(
                    settings.NumberOfReportsParsedInParallel,
                    reportConfiguration.SourceDirectories,
                    new DefaultFilter(reportConfiguration.AssemblyFilters),
                    new DefaultFilter(reportConfiguration.ClassFilters),
                    new DefaultFilter(reportConfiguration.FileFilters))
                        .ParseFiles(reportConfiguration.ReportFiles);

                Logger.DebugFormat(Resources.ReportParsingTook, stopWatch.ElapsedMilliseconds / 1000d);

                this.GenerateReport(
                    reportConfiguration,
                    settings,
                    riskHotspotsAnalysisThresholds,
                    parserResult);

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
        /// Executes the report generation.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <param name="parserResult">The parser result generated by <see cref="CoverageReportParser"/>.</param>
        public void GenerateReport(
            IReportConfiguration reportConfiguration,
            ParserResult parserResult)
        {
            if (reportConfiguration == null)
            {
                throw new ArgumentNullException(nameof(reportConfiguration));
            }

            if (parserResult == null)
            {
                throw new ArgumentNullException(nameof(parserResult));
            }

            var configuration = this.GetConfiguration();

            var settings = new Settings();
            configuration.GetSection("settings").Bind(settings);

            var riskHotspotsAnalysisThresholds = new RiskHotspotsAnalysisThresholds();
            configuration.GetSection("riskHotspotsAnalysisThresholds").Bind(riskHotspotsAnalysisThresholds);

            this.GenerateReport(reportConfiguration, settings, riskHotspotsAnalysisThresholds, parserResult);
        }

        /// <summary>
        /// Executes the report generation.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="riskHotspotsAnalysisThresholds">The risk hotspots analysis thresholds.</param>
        /// <param name="parserResult">The parser result generated by <see cref="CoverageReportParser"/>.</param>
        public void GenerateReport(
            IReportConfiguration reportConfiguration,
            Settings settings,
            RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds,
            ParserResult parserResult)
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

            if (parserResult == null)
            {
                throw new ArgumentNullException(nameof(parserResult));
            }

            var reportContext = new ReportContext(reportConfiguration, settings);

            var pluginLoader = new ReflectionPluginLoader(reportConfiguration.Plugins);
            IReportBuilderFactory reportBuilderFactory = new ReportBuilderFactory(pluginLoader);

            reportContext.RiskHotspotAnalysisResult = new RiskHotspotsAnalyzer(riskHotspotsAnalysisThresholds, settings.DisableRiskHotspots)
                .PerformRiskHotspotAnalysis(parserResult.Assemblies);

            var overallHistoricCoverages = new List<Parser.Analysis.HistoricCoverage>();
            var historyStorage = new HistoryStorageFactory(pluginLoader).GetHistoryStorage(reportConfiguration);

            if (historyStorage != null)
            {
                new HistoryParser(historyStorage, settings.MaximumNumberOfHistoricCoverageFiles)
                    .ApplyHistoricCoverage(parserResult.Assemblies, overallHistoricCoverages);

                reportContext.OverallHistoricCoverages = overallHistoricCoverages;
            }

            DateTime executionTime = DateTime.Now;

            new Reporting.ReportGenerator(
                new CachingFileReader(settings.CachingDuringOfRemoteFilesInMinutes),
                parserResult,
                reportBuilderFactory.GetReportBuilders(reportContext))
                    .CreateReport(reportConfiguration.HistoryDirectory != null, overallHistoricCoverages, executionTime, reportConfiguration.Tag);

            if (historyStorage != null)
            {
                new HistoryReportGenerator(historyStorage)
                    .CreateReport(parserResult.Assemblies, executionTime, reportConfiguration.Tag);
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
