using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Licensing;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates summary report in Markdown format (no reports for classes are generated).
    /// Shows difference in coverage between the current and latest history run.
    /// </summary>
    public class MarkdownDeltaSummaryReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(MarkdownDeltaSummaryReportBuilder));

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        public string ReportType => "MarkdownDeltaSummary";

        /// <summary>
        /// Gets or sets the report context.
        /// </summary>
        /// <value>
        /// The report context.
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

            HistoricCoverage[] historicCoverages = HistoricCoverages.GetOverallHistoricCoverages(this.ReportContext.OverallHistoricCoverages)
                .TakeLast(2)
                .ToArray();

            if (historicCoverages.Length != 2)
            {
                Logger.ErrorFormat(Resources.ErrorDeltaReport, this.ReportType);
                return;
            }

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

            string targetPath = Path.Combine(targetDirectory, "DeltaSummary.md");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            bool proVersion = this.ReportContext.ReportConfiguration.License.DetermineLicenseType() == LicenseType.Pro;

            using (var reportTextWriter = File.CreateText(targetPath))
            {
                HistoricCoverage previous = historicCoverages[0];
                HistoricCoverage current = historicCoverages[1];

                if (this.ReportContext.ReportConfiguration.Title != null)
                {
                    reportTextWriter.WriteLine("# {0} - {1}", ReportResources.DeltaSummary, this.ReportContext.ReportConfiguration.Title);
                }
                else
                {
                    reportTextWriter.WriteLine("# {0}", ReportResources.DeltaSummary);
                }

                reportTextWriter.WriteLine("|||||");
                reportTextWriter.WriteLine("|:---|---:|---:|---:|");

                reportTextWriter.WriteLine("| {0} | {1} | | |", ReportResources.GeneratedOn, this.ReportTime(DateTime.Now));
                reportTextWriter.WriteLine("| {0} | **{1}** | **{2}** | **{3}** |", ReportResources.Description, ReportResources.Previous, ReportResources.Current, ReportResources.Delta);

                reportTextWriter.WriteLine("| {0} | {1} | {2} | |", ReportResources.CoverageDate, this.ReportTime(previous.ExecutionTime), this.ReportTime(current.ExecutionTime));

                if (previous.Tag != null || current.Tag != null)
                {
                    reportTextWriter.WriteLine("| {0} | {1} | {2} | |", ReportResources.Tag, previous.Tag, current.Tag);
                }

                reportTextWriter.WriteLine("| **{0}** | {1} |", ReportResources.Coverage2, this.ReportCoverageQuota(previous.CoverageQuota, current.CoverageQuota));
                reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.CoveredLines, this.ReportValues(previous.CoveredLines, current.CoveredLines));
                reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.CoverableLines, this.ReportValues(previous.CoverableLines, current.CoverableLines));
                reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.TotalLines, this.ReportValues(previous.TotalLines, current.TotalLines));
                reportTextWriter.WriteLine("| **{0}** | {1} |", ReportResources.BranchCoverage2, this.ReportCoverageQuota(previous.BranchCoverageQuota, current.BranchCoverageQuota));
                reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.CoveredBranches2, this.ReportValues(previous.CoveredBranches, current.CoveredBranches));
                reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.TotalBranches, this.ReportValues(previous.TotalBranches, current.TotalBranches));

                if (proVersion)
                {
                    reportTextWriter.WriteLine("| **{0}** | {1} |", ReportResources.CodeElementCoverageQuota2, this.ReportCoverageQuota(previous.CodeElementCoverageQuota, current.CodeElementCoverageQuota));
                    reportTextWriter.WriteLine("| **{0}** | {1} |", ReportResources.FullCodeElementCoverageQuota2, this.ReportCoverageQuota(previous.FullCodeElementCoverageQuota, current.FullCodeElementCoverageQuota));
                    reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.CoveredCodeElements, this.ReportValues(previous.CoveredCodeElements, current.CoveredCodeElements));
                    reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.FullCoveredCodeElements, this.ReportValues(previous.FullCoveredCodeElements, current.FullCoveredCodeElements));
                    reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.TotalCodeElements, this.ReportValues(previous.TotalCodeElements, current.TotalCodeElements));
                }
                else
                {
                    reportTextWriter.WriteLine("| **{0}** | [{1}](https://reportgenerator.io/pro) |", ReportResources.CodeElementCoverageQuota2, ReportResources.MethodCoverageProVersion);
                }

                reportTextWriter.Flush();
            }
        }

        private string ReportTime(DateTime dateTime)
        {
            return dateTime.ToShortDateString() + " - " + dateTime.ToShortTimeString();
        }

        private string ReportCoverageQuota(decimal? optionalPreviousQuota, decimal? optionalCurrentQuota)
        {
            string previousQuota = optionalPreviousQuota.HasValue ? $"{optionalPreviousQuota.Value.ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable;
            string currentQuota = optionalCurrentQuota.HasValue ? $"{optionalCurrentQuota.Value.ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable;
            string deltaQuota = (optionalPreviousQuota.HasValue && optionalCurrentQuota.HasValue) ? $"{(optionalCurrentQuota.Value - optionalPreviousQuota.Value).ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable;
            return string.Format(CultureInfo.InvariantCulture, @"{0} | {1} | {2}", previousQuota, currentQuota, deltaQuota);
        }

        private string ReportValues(int previousValue, int currentValue)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} | {1} | {2}", previousValue, currentValue, currentValue - previousValue);
        }

        private string ReportValues(int? previousValue, int? currentValue)
        {
            return this.ReportValues(previousValue.GetValueOrDefault(), currentValue.GetValueOrDefault());
        }
    }
}
