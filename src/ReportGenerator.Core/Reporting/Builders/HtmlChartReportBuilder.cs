using System;
using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;
using Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates HTML with chart component only (no reports for classes are generated).
    /// </summary>
    public class HtmlChartReportBuilder : HtmlSummaryReportBuilder
    {
        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        public override string ReportType => "HtmlChart";

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="reportRenderer">The report renderer.</param>
        /// <param name="summaryResult">The summary result.</param>
        public override void CreateSummaryReport(IReportRenderer reportRenderer, SummaryResult summaryResult)
        {
            if (reportRenderer == null)
            {
                throw new ArgumentNullException(nameof(reportRenderer));
            }

            if (summaryResult == null)
            {
                throw new ArgumentNullException(nameof(summaryResult));
            }

            reportRenderer.BeginSummaryReport(this.ReportContext.ReportConfiguration.TargetDirectory, "CoverageHistory.htm", ReportResources.Summary);

            var historicCoverages = this.GetOverallHistoricCoverages(this.ReportContext.OverallHistoricCoverages);
            if (historicCoverages.Any(h => h.CoverageQuota.HasValue || h.BranchCoverageQuota.HasValue))
            {
                reportRenderer.Chart(historicCoverages, this.ReportContext.Settings.RenderPngFallBackImagesForHistoryCharts);
            }

            reportRenderer.CustomSummary(summaryResult.Assemblies, new List<RiskHotspot>(), summaryResult.SupportsBranchCoverage);

            reportRenderer.SaveSummaryReport(this.ReportContext.ReportConfiguration.TargetDirectory);
        }
    }
}
