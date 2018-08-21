﻿using System;
using Palmmedia.ReportGenerator.Logging;
using Palmmedia.ReportGenerator.Parser;
using Palmmedia.ReportGenerator.Properties;
using Palmmedia.ReportGenerator.Reporting;
using Palmmedia.ReportGenerator.Reporting.History;

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

            var parserOptions = new ParserOptions()
            {
                ImplicitBranchCoverage = reportConfiguration.IncludeImplicitBranches
            };

            var parser = ParserFactory.CreateParser(reportConfiguration.ReportFiles, reportConfiguration.SourceDirectories, parserOptions);

            var overallHistoricCoverages = new System.Collections.Generic.List<Parser.Analysis.HistoricCoverage>();
            var historyStorage = new MefHistoryStorageFactory().GetHistoryStorage(reportConfiguration);

            if (historyStorage != null)
            {
                new HistoryParser(historyStorage)
                        .ApplyHistoricCoverage(parser.Assemblies, overallHistoricCoverages);

                reportConfiguration.OverallHistoricCoverages = overallHistoricCoverages;
            }

            var filteredAssemblies = new Reporting.ReportGenerator(
                parser,
                new DefaultFilter(reportConfiguration.AssemblyFilters),
                new DefaultFilter(reportConfiguration.ClassFilters),
                new DefaultFilter(reportConfiguration.FileFilters),
                reportConfiguration.ReportBuilderFactory.GetReportBuilders(reportConfiguration))
                    .CreateReport(reportConfiguration.HistoryDirectory != null, overallHistoricCoverages, executionTime, reportConfiguration.Tag);

            if (historyStorage != null)
            {
                new HistoryReportGenerator(historyStorage)
                        .CreateReport(filteredAssemblies, executionTime, reportConfiguration.Tag);
            }

            stopWatch.Stop();
            Logger.InfoFormat(Resources.ReportGenerationTook, stopWatch.ElapsedMilliseconds / 1000d);

            return true;
        }
    }
}
