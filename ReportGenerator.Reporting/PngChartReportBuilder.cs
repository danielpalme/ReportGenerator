using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using Palmmedia.ReportGenerator.Parser.Analysis;
using Palmmedia.ReportGenerator.Properties;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Creates HTML with chart component only (no reports for classes are generated).
    /// </summary>
    [System.ComponentModel.Composition.Export(typeof(IReportBuilder))]
    public class PngChartReportBuilder : IReportBuilder
    {
        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        public string ReportType => "PngChart";

        /// <summary>
        /// Gets or sets the target directory where reports are stored.
        /// </summary>
        /// <value>
        /// The target directory.
        /// </value>
        public string TargetDirectory { get; set; }

        /// <summary>
        /// Creates a class report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        public void CreateClassReport(Palmmedia.ReportGenerator.Parser.Analysis.Class @class, IEnumerable<Palmmedia.ReportGenerator.Parser.Analysis.FileAnalysis> fileAnalyses)
        {
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        public void CreateSummaryReport(Palmmedia.ReportGenerator.Parser.Analysis.SummaryResult summaryResult)
        {
            var historicCoverages = this.GetOverallHistoricCoverages(summaryResult.Assemblies.SelectMany(a => a.Classes));

            historicCoverages = this.FilterHistoricCoverages(historicCoverages, 100);

            if (historicCoverages.Any(h => h.CoverageQuota.HasValue || h.BranchCoverageQuota.HasValue))
            {
                Chart chart = new Chart()
                {
                    Size = new Size(900, 300),
                };

                chart.ChartAreas.Add("Default");
                chart.ChartAreas[0].AxisX.LineColor = Color.LightGray;
                chart.ChartAreas[0].AxisX.MajorTickMark.LineColor = Color.LightGray;
                chart.ChartAreas[0].AxisX.MajorTickMark.TickMarkStyle = TickMarkStyle.None;
                chart.ChartAreas[0].AxisX.MajorTickMark.Enabled = false;
                chart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                chart.ChartAreas[0].AxisX.LabelStyle.Enabled = false;

                chart.ChartAreas[0].AxisY.LineColor = Color.LightGray;
                chart.ChartAreas[0].AxisY.MajorTickMark.LineColor = Color.LightGray;
                chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                chart.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
                chart.ChartAreas[0].AxisY.Minimum = 0;
                chart.ChartAreas[0].AxisY.Maximum = 100;

                chart.Legends.Add(new Legend("Default")
                {
                    Docking = Docking.Bottom,
                    Alignment = StringAlignment.Center,
                    BorderWidth = 1,
                    BorderColor = Color.LightGray
                });

                chart.Series.Add(ReportResources.Coverage2);
                chart.Series[ReportResources.Coverage2].ChartType = SeriesChartType.Line;
                chart.Series[ReportResources.Coverage2].Color = Color.Red;
                chart.Series[ReportResources.Coverage2].MarkerSize = 5;
                chart.Series[ReportResources.Coverage2].MarkerColor = Color.Red;
                chart.Series[ReportResources.Coverage2].MarkerStyle = MarkerStyle.Circle;

                if (historicCoverages.Any(h => h.BranchCoverageQuota.HasValue))
                {
                    chart.Series.Add(ReportResources.BranchCoverage2);
                    chart.Series[ReportResources.BranchCoverage2].ChartType = SeriesChartType.Line;
                    chart.Series[ReportResources.BranchCoverage2].Color = Color.Blue;
                    chart.Series[ReportResources.BranchCoverage2].MarkerSize = 5;
                    chart.Series[ReportResources.BranchCoverage2].MarkerColor = Color.Blue;
                    chart.Series[ReportResources.BranchCoverage2].MarkerStyle = MarkerStyle.Circle;
                }

                int counter = 0;
                foreach (var historicCoverage in historicCoverages)
                {
                    chart.Series[ReportResources.Coverage2].Points.AddXY(counter, historicCoverage.CoverageQuota.GetValueOrDefault());

                    if (historicCoverages.Any(h => h.BranchCoverageQuota.HasValue))
                    {
                        chart.Series[ReportResources.BranchCoverage2].Points.AddXY(counter, historicCoverage.BranchCoverageQuota.GetValueOrDefault());
                    }

                    counter++;
                }

                chart.SaveImage(Path.Combine(this.TargetDirectory, "CoverageHistory.png"), ChartImageFormat.Png);
            }
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

        /// <summary>
        /// Filters the historic coverages (equal elements are removed).
        /// </summary>
        /// <param name="historicCoverages">The historic coverages.</param>
        /// <param name="maximum">The maximum.</param>
        /// <returns>The filtered historic coverages.</returns>
        private IEnumerable<HistoricCoverage> FilterHistoricCoverages(IEnumerable<HistoricCoverage> historicCoverages, int maximum)
        {
            var result = new List<HistoricCoverage>();

            foreach (var historicCoverage in historicCoverages)
            {
                if (result.Count == 0 || !result[result.Count - 1].Equals(historicCoverage))
                {
                    result.Add(historicCoverage);
                }
            }

            result.RemoveRange(0, Math.Max(0, result.Count - maximum));

            return result;
        }
    }
}
