using System;
using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Parser.Analysis;
using Palmmedia.ReportGenerator.Properties;
using Palmmedia.ReportGenerator.Reporting.CodeAnalysis;
using Palmmedia.ReportGenerator.Reporting.Rendering;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Creates HTML with chart component only (no reports for classes are generated).
    /// </summary>
    [System.ComponentModel.Composition.Export(typeof(IReportBuilder))]
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

            reportRenderer.BeginSummaryReport(this.ReportConfiguration.TargetDirectory, "CoverageHistory.htm", ReportResources.Summary);

            var historicCoverages = this.GetOverallHistoricCoverages(this.ReportConfiguration.OverallHistoricCoverages);
            if (historicCoverages.Any(h => h.CoverageQuota.HasValue || h.BranchCoverageQuota.HasValue))
            {
                reportRenderer.Chart(historicCoverages);
            }

            reportRenderer.CustomSummary(summaryResult.Assemblies, new List<RiskHotspot>(), summaryResult.SupportsBranchCoverage);

            reportRenderer.SaveSummaryReport(this.ReportConfiguration.TargetDirectory);
        }
    }
}
