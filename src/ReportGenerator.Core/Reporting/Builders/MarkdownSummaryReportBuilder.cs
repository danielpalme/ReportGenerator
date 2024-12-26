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
    /// </summary>
    public class MarkdownSummaryReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(MarkdownSummaryReportBuilder));

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        public string ReportType => "MarkdownSummary";

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
                var assembliesWithClasses = summaryResult.Assemblies
                    .Where(a => a.Classes.Any())
                    .ToArray();

                if (this.ReportContext.ReportConfiguration.Title != null)
                {
                    reportTextWriter.WriteLine("# {0} - {1}", ReportResources.Summary, this.ReportContext.ReportConfiguration.Title);
                }
                else
                {
                    reportTextWriter.WriteLine("# {0}", ReportResources.Summary);
                }

                reportTextWriter.WriteLine("|||");
                reportTextWriter.WriteLine("|:---|:---|");
                reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.GeneratedOn, DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString());

                if (summaryResult.MinimumTimeStamp.HasValue || summaryResult.MaximumTimeStamp.HasValue)
                {
                    reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.CoverageDate, summaryResult.CoverageDate());
                }

                reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.Parser, summaryResult.UsedParser);
                reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.Assemblies2, assembliesWithClasses.Count().ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.Classes, assembliesWithClasses.SelectMany(a => a.Classes).Count().ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.Files2, assembliesWithClasses.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count().ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine("| **{0}** | {1} |", ReportResources.Coverage2, summaryResult.CoverageQuota.HasValue ? $"{summaryResult.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}% ({summaryResult.CoveredLines.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {summaryResult.CoverableLines.ToString(CultureInfo.InvariantCulture)})" : string.Empty);
                reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.CoveredLines, summaryResult.CoveredLines.ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.UncoveredLines, (summaryResult.CoverableLines - summaryResult.CoveredLines).ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.CoverableLines, summaryResult.CoverableLines.ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.TotalLines, summaryResult.TotalLines.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));

                if (summaryResult.SupportsBranchCoverage)
                {
                    if (summaryResult.CoveredBranches.HasValue && summaryResult.TotalBranches.HasValue)
                    {
                        decimal? branchCoverage = summaryResult.BranchCoverageQuota;

                        if (branchCoverage.HasValue)
                        {
                            reportTextWriter.WriteLine("| **{0}** | {1} |", ReportResources.BranchCoverage2, $"{branchCoverage.Value.ToString(CultureInfo.InvariantCulture)}% ({summaryResult.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {summaryResult.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)})");
                        }

                        reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.CoveredBranches2, summaryResult.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
                        reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.TotalBranches, summaryResult.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
                    }
                }

                if (proVersion)
                {
                    reportTextWriter.WriteLine("| **{0}** | {1} |", ReportResources.CodeElementCoverageQuota2, summaryResult.CodeElementCoverageQuota.HasValue ? $"{summaryResult.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}% ({summaryResult.CoveredCodeElements.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {summaryResult.TotalCodeElements.ToString(CultureInfo.InvariantCulture)})" : string.Empty);
                    reportTextWriter.WriteLine("| **{0}** | {1} |", ReportResources.FullCodeElementCoverageQuota2, summaryResult.FullCodeElementCoverageQuota.HasValue ? $"{summaryResult.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}% ({summaryResult.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {summaryResult.TotalCodeElements.ToString(CultureInfo.InvariantCulture)})" : string.Empty);
                    reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.CoveredCodeElements, summaryResult.CoveredCodeElements.ToString(CultureInfo.InvariantCulture));
                    reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.FullCoveredCodeElements, summaryResult.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture));
                    reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.TotalCodeElements, summaryResult.TotalCodeElements.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    reportTextWriter.WriteLine("| **{0}** | [{1}](https://reportgenerator.io/pro) |", ReportResources.CodeElementCoverageQuota2, ReportResources.MethodCoverageProVersion);
                }

                if (this.ReportContext.ReportConfiguration.Tag != null)
                {
                    reportTextWriter.WriteLine("| {0} | {1} |", ReportResources.Tag, this.ReportContext.ReportConfiguration.Tag);
                }

                reportTextWriter.WriteLine();

                if (assembliesWithClasses.Any())
                {
                    reportTextWriter.Write(
                        "|**{0}**|**{1}**|**{2}**|**{3}**|**{4}**|**{5}**|",
                        ReportResources.Name,
                        ReportResources.Covered,
                        ReportResources.Uncovered,
                        ReportResources.Coverable,
                        ReportResources.Total,
                        ReportResources.Coverage);

                    if (summaryResult.SupportsBranchCoverage)
                    {
                        reportTextWriter.Write(
                            "**{0}**|**{1}**|**{2}**|",
                            ReportResources.Covered,
                            ReportResources.Total,
                            ReportResources.BranchCoverage);
                    }

                    if (proVersion)
                    {
                        reportTextWriter.Write(
                            "**{0}**|**{1}**|**{2}**|**{3}**|",
                            ReportResources.Covered,
                            ReportResources.Total,
                            ReportResources.CodeElementCoverageQuota,
                            ReportResources.FullCodeElementCoverageQuota);
                    }

                    reportTextWriter.WriteLine();

                    reportTextWriter.Write("|:---|---:|---:|---:|---:|---:|");

                    if (summaryResult.SupportsBranchCoverage)
                    {
                        reportTextWriter.Write("---:|---:|---:|");
                    }

                    if (proVersion)
                    {
                        reportTextWriter.Write("---:|---:|---:|---:|");
                    }

                    reportTextWriter.WriteLine();

                    foreach (var assembly in assembliesWithClasses)
                    {
                        reportTextWriter.Write("|**{0}**", assembly.Name);
                        reportTextWriter.Write("|**{0}**", assembly.CoveredLines);
                        reportTextWriter.Write("|**{0}**", assembly.CoverableLines - assembly.CoveredLines);
                        reportTextWriter.Write("|**{0}**", assembly.CoverableLines);
                        reportTextWriter.Write("|**{0}**", assembly.TotalLines.GetValueOrDefault());
                        reportTextWriter.Write("|**{0}**", assembly.CoverageQuota.HasValue ? assembly.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);

                        if (summaryResult.SupportsBranchCoverage)
                        {
                            reportTextWriter.Write("|**{0}**", assembly.CoveredBranches);
                            reportTextWriter.Write("|**{0}**", assembly.TotalBranches);
                            reportTextWriter.Write("|**{0}**", assembly.BranchCoverageQuota.HasValue ? assembly.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                        }

                        if (proVersion)
                        {
                            reportTextWriter.Write("|**{0}**", assembly.CoveredCodeElements);
                            reportTextWriter.Write("|**{0}**", assembly.TotalCodeElements);
                            reportTextWriter.Write("|**{0}**", assembly.CodeElementCoverageQuota.HasValue ? assembly.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                            reportTextWriter.Write("|**{0}**", assembly.FullCodeElementCoverageQuota.HasValue ? assembly.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                        }

                        reportTextWriter.WriteLine("|");

                        foreach (var @class in assembly.Classes)
                        {
                            reportTextWriter.Write("|{0}", @class.Name);
                            reportTextWriter.Write("|{0}", @class.CoveredLines);
                            reportTextWriter.Write("|{0}", @class.CoverableLines - @class.CoveredLines);
                            reportTextWriter.Write("|{0}", @class.CoverableLines);
                            reportTextWriter.Write("|{0}", @class.TotalLines.GetValueOrDefault());
                            reportTextWriter.Write("|{0}", @class.CoverageQuota.HasValue ? @class.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);

                            if (summaryResult.SupportsBranchCoverage)
                            {
                                reportTextWriter.Write("|{0}", @class.CoveredBranches);
                                reportTextWriter.Write("|{0}", @class.TotalBranches);
                                reportTextWriter.Write("|{0}", @class.BranchCoverageQuota.HasValue ? @class.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                            }

                            if (proVersion)
                            {
                                reportTextWriter.Write("|{0}", @class.CoveredCodeElements);
                                reportTextWriter.Write("|{0}", @class.TotalCodeElements);
                                reportTextWriter.Write("|{0}", @class.CodeElementCoverageQuota.HasValue ? @class.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                                reportTextWriter.Write("|{0}", @class.FullCodeElementCoverageQuota.HasValue ? @class.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                            }

                            reportTextWriter.WriteLine("|");
                        }
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
