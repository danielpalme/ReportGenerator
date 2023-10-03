using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Licensing;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;
using Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates history chart in SVG format.
    /// </summary>
    public class SvgChartReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(SvgChartReportBuilder));

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        public string ReportType => "SvgChart";

        /// <summary>
        /// Gets or sets the report configuration.
        /// </summary>
        /// <value>
        /// The report configuration.
        /// </value>
        public IReportContext ReportContext { get; set; }

        /// <summary>
        /// Creates a class report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        public void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses)
        {
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        public void CreateSummaryReport(SummaryResult summaryResult)
        {
            if (summaryResult == null)
            {
                throw new ArgumentNullException(nameof(summaryResult));
            }

            bool proVersion = this.ReportContext.ReportConfiguration.License.DetermineLicenseType() == LicenseType.Pro;

            var historicCoverages = HistoricCoverages.GetOverallHistoricCoverages(this.ReportContext.OverallHistoricCoverages);

            var filteredHistoricCoverages = this.FilterHistoricCoverages(historicCoverages, 100);

            if (filteredHistoricCoverages.Any(h => h.CoverageQuota.HasValue || h.BranchCoverageQuota.HasValue))
            {
                string image = SvgHistoryChartRenderer.RenderHistoryChart(filteredHistoricCoverages, proVersion);

                string targetDirectory = this.ReportContext.ReportConfiguration.TargetDirectory;

                if (this.ReportContext.Settings.CreateSubdirectoryForAllReportTypes)
                {
                    targetDirectory = Path.Combine(targetDirectory, this.ReportType);

                    if (!Directory.Exists(targetDirectory))
                    {
                        try
                        {
                            Directory.CreateDirectory(targetDirectory);
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorFormat(Resources.TargetDirectoryCouldNotBeCreated, targetDirectory, ex.GetExceptionMessageForDisplay());
                            return;
                        }
                    }
                }

                string targetPath = Path.Combine(targetDirectory, "CoverageHistory.svg");

                Logger.InfoFormat(Resources.WritingReportFile, targetPath);

                File.WriteAllText(targetPath, image);
            }
        }

        /// <summary>
        /// Filters the historic coverages (equal elements are removed).
        /// </summary>
        /// <param name="historicCoverages">The historic coverages.</param>
        /// <param name="maximum">The maximum.</param>
        /// <returns>The filtered historic coverages.</returns>
        private List<HistoricCoverage> FilterHistoricCoverages(IEnumerable<HistoricCoverage> historicCoverages, int maximum)
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