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
    /// Creates summary report in Markdown format (assemblies only).
    /// </summary>
    public class MarkdownAssembliesSummaryReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(MarkdownAssembliesSummaryReportBuilder));

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        public string ReportType => "MarkdownAssembliesSummary";

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

            string targetPath = Path.Combine(targetDirectory, "Summary.md");

            bool proVersion = this.ReportContext.ReportConfiguration.License.DetermineLicenseType() == LicenseType.Pro;

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            using (var reportTextWriter = File.CreateText(targetPath))
            {
                if (this.ReportContext.ReportConfiguration.Title != null)
                {
                    reportTextWriter.WriteLine("# {0} - {1}", ReportResources.Summary, this.ReportContext.ReportConfiguration.Title);
                }
                else
                {
                    reportTextWriter.WriteLine("# {0}", ReportResources.Summary);
                }

                reportTextWriter.WriteLine();

                reportTextWriter.Write($"![{ReportResources.Coverage}](https://img.shields.io/badge/lines-{(summaryResult.CoverageQuota.HasValue ? summaryResult.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%25" : "-")}-{(summaryResult.CoverageQuota.GetValueOrDefault() < 80 ? "C10909" : "0AAD0A")})");

                if (summaryResult.SupportsBranchCoverage)
                {
                    reportTextWriter.Write($" ![{ReportResources.BranchCoverage}](https://img.shields.io/badge/branches-{(summaryResult.BranchCoverageQuota.HasValue ? summaryResult.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%25" : "-")}-{(summaryResult.BranchCoverageQuota.GetValueOrDefault() < 80 ? "C10909" : "0AAD0A")})");
                }

                if (proVersion)
                {
                    reportTextWriter.Write($" ![{ReportResources.CodeElementCoverageQuota}](https://img.shields.io/badge/methods-{(summaryResult.CodeElementCoverageQuota.HasValue ? summaryResult.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%25" : "-")}-{(summaryResult.CodeElementCoverageQuota.GetValueOrDefault() < 80 ? "C10909" : "0AAD0A")})");

                    reportTextWriter.Write($" ![{ReportResources.FullCodeElementCoverageQuota}](https://img.shields.io/badge/methods-{(summaryResult.FullCodeElementCoverageQuota.HasValue ? summaryResult.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%25" : "-")}-{(summaryResult.FullCodeElementCoverageQuota.GetValueOrDefault() < 80 ? "C10909" : "0AAD0A")})");
                }

                reportTextWriter.WriteLine();
                reportTextWriter.WriteLine();

                var assembliesWithClasses = summaryResult.Assemblies
                    .Where(a => a.Classes.Any())
                    .ToArray();

                if (assembliesWithClasses.Any())
                {
                    reportTextWriter.Write(
                        "|**{0}**|**{1}**|",
                        ReportResources.Assembly2,
                        ReportResources.Coverage);

                    if (summaryResult.SupportsBranchCoverage)
                    {
                        reportTextWriter.Write(
                            "**{0}**|",
                            ReportResources.BranchCoverage);
                    }

                    if (proVersion)
                    {
                        reportTextWriter.Write(
                            "**{0}**|",
                            ReportResources.CodeElementCoverageQuota);

                        reportTextWriter.Write(
                            "**{0}**|",
                            ReportResources.FullCodeElementCoverageQuota);
                    }

                    reportTextWriter.WriteLine();

                    reportTextWriter.Write("|:---|---:|");

                    if (summaryResult.SupportsBranchCoverage)
                    {
                        reportTextWriter.Write("---:|");
                    }

                    if (proVersion)
                    {
                        reportTextWriter.Write("---:|");
                        reportTextWriter.Write("---:|");
                    }

                    reportTextWriter.WriteLine();

                    foreach (var assembly in assembliesWithClasses)
                    {
                        reportTextWriter.Write("|**{0}**", assembly.Name);
                        reportTextWriter.Write("|**{0}**", assembly.CoverageQuota.HasValue ? assembly.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);

                        if (summaryResult.SupportsBranchCoverage)
                        {
                            reportTextWriter.Write("|**{0}**", assembly.BranchCoverageQuota.HasValue ? assembly.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                        }

                        if (proVersion)
                        {
                            reportTextWriter.Write("|**{0}**", assembly.CodeElementCoverageQuota.HasValue ? assembly.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                            reportTextWriter.Write("|**{0}**", assembly.FullCodeElementCoverageQuota.HasValue ? assembly.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                        }

                        reportTextWriter.WriteLine("|");
                    }
                }
                else
                {
                    reportTextWriter.WriteLine(ReportResources.NoCoveredAssemblies);
                }

                reportTextWriter.Flush();
            }
        }
    }
}
