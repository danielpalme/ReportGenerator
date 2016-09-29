using System;
using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Parser.Analysis;
using Palmmedia.ReportGenerator.Properties;
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

            reportRenderer.BeginSummaryReport(this.TargetDirectory, "CoverageHistory.htm", ReportResources.Summary);

            var historicCoverages = this.GetOverallHistoricCoverages(summaryResult.Assemblies.SelectMany(a => a.Classes));
            if (historicCoverages.Any(h => h.CoverageQuota.HasValue || h.BranchCoverageQuota.HasValue))
            {
                reportRenderer.Chart(historicCoverages);
            }

            reportRenderer.CustomSummary(summaryResult.Assemblies, summaryResult.SupportsBranchCoverage);

            reportRenderer.SaveSummaryReport(this.TargetDirectory);
        }

        /// <summary>
        /// Gets the overall historic coverages from all classes.
        /// </summary>
        /// <param name="classes">The classes.</param>
        /// <returns>
        /// The overall historic coverages from all classes.
        /// </returns>
        private IEnumerable<HistoricCoverage> GetOverallHistoricCoverages(IEnumerable<Class> classes)
        {
            var historicCoverages = classes
                .SelectMany(c => c.HistoricCoverages);

            var executionTimes = historicCoverages
                .Select(h => h.ExecutionTime)
                .Distinct();

            var result = new List<HistoricCoverage>();

            foreach (var executionTime in executionTimes)
            {
                var historicCoveragesOfExecutionTime = historicCoverages
                    .Where(h => h.ExecutionTime.Equals(executionTime))
                    .ToArray();

                result.Add(new HistoricCoverage(executionTime)
                {
                    CoveredLines = historicCoveragesOfExecutionTime.Sum(h => h.CoveredLines),
                    CoverableLines = historicCoveragesOfExecutionTime.Sum(h => h.CoverableLines),
                    CoveredBranches = historicCoveragesOfExecutionTime.Sum(h => h.CoveredBranches),
                    TotalBranches = historicCoveragesOfExecutionTime.Sum(h => h.TotalBranches),
                    TotalLines = historicCoveragesOfExecutionTime.Sum(h => h.TotalLines)
                });
            }

            return result;
        }
    }
}
