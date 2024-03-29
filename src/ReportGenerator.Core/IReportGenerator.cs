using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Reporting;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Provides report generation capabilities.
    /// </summary>
    public interface IReportGenerator
    {
        /// <summary>
        /// Generates a report using given configuration.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <returns>True if the report generation succeeded, otherwise false.</returns>
        bool GenerateReport(IReportConfiguration reportConfiguration);

        /// <summary>
        /// Generates a report using given configuration.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="riskHotspotsAnalysisThresholds">The risk hotspots analysis thresholds.</param>
        /// <returns><c>true</c> if report was generated successfully; otherwise <c>false</c>.</returns>
        bool GenerateReport(
            IReportConfiguration reportConfiguration,
            Settings settings,
            RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds);

        /// <summary>
        /// Generates a report using given configuration.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="minimumCoverageThresholds">The minimum coverage thresholds.</param>
        /// <param name="riskHotspotsAnalysisThresholds">The risk hotspots analysis thresholds.</param>
        /// <returns><c>true</c> if report was generated successfully; otherwise <c>false</c>.</returns>
        bool GenerateReport(
            IReportConfiguration reportConfiguration,
            Settings settings,
            MinimumCoverageThresholds minimumCoverageThresholds,
            RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds);

        /// <summary>
        /// Executes the report generation.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <param name="parserResult">The parser result generated by <see cref="CoverageReportParser"/>.</param>
        void GenerateReport(
            IReportConfiguration reportConfiguration,
            ParserResult parserResult);

        /// <summary>
        /// Executes the report generation.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="riskHotspotsAnalysisThresholds">The risk hotspots analysis thresholds.</param>
        /// <param name="parserResult">The parser result generated by <see cref="CoverageReportParser"/>.</param>
        void GenerateReport(
            IReportConfiguration reportConfiguration,
            Settings settings,
            RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds,
            ParserResult parserResult);

        /// <summary>
        /// Executes the report generation.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="minimumCoverageThresholds">The minimum coverage thresholds.</param>
        /// <param name="riskHotspotsAnalysisThresholds">The risk hotspots analysis thresholds.</param>
        /// <param name="parserResult">The parser result generated by <see cref="CoverageReportParser"/>.</param>
        void GenerateReport(
            IReportConfiguration reportConfiguration,
            Settings settings,
            MinimumCoverageThresholds minimumCoverageThresholds,
            RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds,
            ParserResult parserResult);
    }
}
