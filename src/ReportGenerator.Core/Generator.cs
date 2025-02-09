using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

                var minimumCoverageThresholds = new MinimumCoverageThresholds();
                configuration.GetSection("minimumCoverageThresholds").Bind(minimumCoverageThresholds);

                var riskHotspotsAnalysisThresholds = new RiskHotspotsAnalysisThresholds();
                configuration.GetSection("riskHotspotsAnalysisThresholds").Bind(riskHotspotsAnalysisThresholds);

                return this.GenerateReport(reportConfiguration, settings, minimumCoverageThresholds, riskHotspotsAnalysisThresholds);
            }
            catch (LowCoverageException ex)
            {
                Logger.Error(ex.GetExceptionMessageForDisplay());
                return false;
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
            return this.GenerateReport(
                reportConfiguration,
                settings,
                new MinimumCoverageThresholds(),
                riskHotspotsAnalysisThresholds);
        }

        /// <summary>
        /// Generates a report using given configuration.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="minimumCoverageThresholds">The minimum coverage thresholds.</param>
        /// <param name="riskHotspotsAnalysisThresholds">The risk hotspots analysis thresholds.</param>
        /// <returns><c>true</c> if report was generated successfully; otherwise <c>false</c>.</returns>
        public bool GenerateReport(
            IReportConfiguration reportConfiguration,
            Settings settings,
            MinimumCoverageThresholds minimumCoverageThresholds,
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

            if (minimumCoverageThresholds == null)
            {
                throw new ArgumentNullException(nameof(minimumCoverageThresholds));
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

                Logger.Debug($"{Resources.Executable}: {AppContext.BaseDirectory}");
                Logger.Debug($"{Resources.WorkingDirectory}: {Directory.GetCurrentDirectory()}");

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

                try
                {
                    string settingsAsJson = System.Text.Json.JsonSerializer.Serialize(settings);
                    Logger.Debug(Resources.Settings);
                    Logger.Debug(" " + settingsAsJson);
                    Logger.Debug(" " + System.Text.Json.JsonSerializer.Serialize(minimumCoverageThresholds));
                    Logger.Debug(" " + System.Text.Json.JsonSerializer.Serialize(riskHotspotsAnalysisThresholds));
                }
                catch (InvalidOperationException)
                {
                    // Json serialization may fail in AOT scenarios
                }

                var stopWatch = Stopwatch.StartNew();

                var parserResult = new CoverageReportParser(
                    new ReportContext(reportConfiguration, settings))
                        .ParseFiles(reportConfiguration.ReportFiles);

                Logger.DebugFormat(Resources.ReportParsingTook, stopWatch.ElapsedMilliseconds / 1000d);

                this.GenerateReport(
                    reportConfiguration,
                    settings,
                    minimumCoverageThresholds,
                    riskHotspotsAnalysisThresholds,
                    parserResult);

                stopWatch.Stop();
                Logger.InfoFormat(Resources.ReportGenerationTook, stopWatch.ElapsedMilliseconds / 1000d);

                return true;
            }
            catch (LowCoverageException ex)
            {
                Logger.Error(ex.GetExceptionMessageForDisplay());
                return false;
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

            var minimumCoverageThresholds = new MinimumCoverageThresholds();
            configuration.GetSection("minimumCoverageThresholds").Bind(minimumCoverageThresholds);

            var riskHotspotsAnalysisThresholds = new RiskHotspotsAnalysisThresholds();
            configuration.GetSection("riskHotspotsAnalysisThresholds").Bind(riskHotspotsAnalysisThresholds);

            this.GenerateReport(reportConfiguration, settings, minimumCoverageThresholds, riskHotspotsAnalysisThresholds, parserResult);
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
            this.GenerateReport(
                reportConfiguration,
                settings,
                new MinimumCoverageThresholds(),
                riskHotspotsAnalysisThresholds,
                parserResult);
        }

        /// <summary>
        /// Executes the report generation.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="minimumCoverageThresholds">The minimum coverage thresholds.</param>
        /// <param name="riskHotspotsAnalysisThresholds">The risk hotspots analysis thresholds.</param>
        /// <param name="parserResult">The parser result generated by <see cref="CoverageReportParser"/>.</param>
        public void GenerateReport(
            IReportConfiguration reportConfiguration,
            Settings settings,
            MinimumCoverageThresholds minimumCoverageThresholds,
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

            if (minimumCoverageThresholds == null)
            {
                throw new ArgumentNullException(nameof(minimumCoverageThresholds));
            }

            if (riskHotspotsAnalysisThresholds == null)
            {
                throw new ArgumentNullException(nameof(riskHotspotsAnalysisThresholds));
            }

            if (parserResult == null)
            {
                throw new ArgumentNullException(nameof(parserResult));
            }

            MathExtensions.MaximumDecimalPlaces = settings.MaximumDecimalPlacesForCoverageQuotas;

            var reportContext = new ReportContext(reportConfiguration, settings);

            var pluginLoader = new ReflectionPluginLoader(reportConfiguration.Plugins);
            IReportBuilderFactory reportBuilderFactory = new ReportBuilderFactory(pluginLoader);

            reportContext.RiskHotspotAnalysisResult = new RiskHotspotsAnalyzer(
                riskHotspotsAnalysisThresholds,
                settings.DisableRiskHotspots,
                new DefaultFilter(reportContext.ReportConfiguration.RiskHotspotAssemblyFilters),
                new DefaultFilter(reportContext.ReportConfiguration.RiskHotspotClassFilters))
                .PerformRiskHotspotAnalysis(parserResult.Assemblies);

            var overallHistoricCoverages = new List<Parser.Analysis.HistoricCoverage>();
            var historyStorage = new HistoryStorageFactory(pluginLoader).GetHistoryStorage(reportConfiguration);

            if (historyStorage != null)
            {
                new HistoryParser(historyStorage, settings.MaximumNumberOfHistoricCoverageFiles, settings.NumberOfReportsParsedInParallel)
                    .ApplyHistoricCoverage(parserResult.Assemblies, overallHistoricCoverages);

                reportContext.OverallHistoricCoverages = overallHistoricCoverages;
            }

            DateTime executionTime = DateTime.Now;

            new Reporting.ReportGenerator(
                new CachingFileReader(new LocalFileReader(reportConfiguration.SourceDirectories), settings.CachingDurationOfRemoteFilesInMinutes, settings.CustomHeadersForRemoteFiles),
                parserResult,
                reportBuilderFactory.GetReportBuilders(reportContext))
                    .CreateReport(reportConfiguration.HistoryDirectory != null, overallHistoricCoverages, executionTime, reportConfiguration.Tag);

            if (historyStorage != null)
            {
                new HistoryReportGenerator(historyStorage, settings.HistoryFileNamePrefix)
                    .CreateReport(parserResult.Assemblies, executionTime, reportConfiguration.Tag);
            }

            new MinimumCoverageThresholdsValidator(minimumCoverageThresholds)
                .Validate(parserResult);
        }

        /// <summary>
        /// Get the <see cref="IConfigurationRoot"/>.
        /// </summary>
        /// <returns>The configuration.</returns>
        private IConfigurationRoot GetConfiguration()
        {
            var args = Environment.GetCommandLineArgs()
                .Where(a => !a.StartsWith("-property:"))
                .Where(a => !a.StartsWith("-p:"))
                .Where(a => !(a.StartsWith("/") && a.EndsWith(".dll"))) // Filter path in arguments like /home/user/.dotnet/tools/.store/dotnet-reportgenerator-globaltool/x.y.z/dotnet-reportgenerator-globaltool/x.y.z/tools/net8.0/any/ReportGenerator.dll (appears in global tool)
                .Where(a => !CommandLineArgumentNames.CommandLineParameterRegex.IsMatch(a))
                .ToArray();

            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .AddCommandLine(args);

                return builder.Build();
            }
            catch (IOException)
            {
                // This can happen when excuted within MSBuild (dotnet msbuild): JSON configuration gets ignored
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddCommandLine(args);

                return builder.Build();
            }
        }
    }
}
