using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Licensing;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates summary report in Markdown format optimized for Github (no reports for classes are generated).
    /// </summary>
    public class MarkdownSummaryGithubReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(MarkdownSummaryGithubReportBuilder));

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        public string ReportType => "MarkdownSummaryGithub";

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

            string targetPath = Path.Combine(targetDirectory, "SummaryGithub.md");

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

                reportTextWriter.WriteLine(
                    "<details open><summary>{0}</summary>",
                    ReportResources.Summary);

                reportTextWriter.WriteLine();

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
                reportTextWriter.WriteLine("</details>");

                reportTextWriter.WriteLine();
                reportTextWriter.WriteLine("## {0}", ReportResources.Coverage3);

                if (assembliesWithClasses.Any())
                {
                    foreach (var assembly in assembliesWithClasses)
                    {
                        reportTextWriter.WriteLine(
                            "<details><summary>{0} - {1}</summary>",
                            assembly.Name,
                            assembly.CoverageQuota.HasValue ? assembly.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);

                        reportTextWriter.WriteLine();

                        reportTextWriter.Write(
                            "|**{0}**|**{1}**|",
                            ReportResources.Name,
                            ReportResources.Line);

                        if (summaryResult.SupportsBranchCoverage)
                        {
                            reportTextWriter.Write(
                                "**{0}**|",
                                ReportResources.Branch);
                        }

                        if (proVersion)
                        {
                            reportTextWriter.Write(
                                "**{0}**|",
                                ReportResources.Method);

                            reportTextWriter.Write(
                                "**{0}**|",
                                ReportResources.FullMethod);
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

                        reportTextWriter.Write("|**{0}**", InsertLineBreaks(assembly.Name));
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

                        foreach (var @class in assembly.Classes)
                        {
                            reportTextWriter.Write("|{0}", InsertLineBreaks(@class.Name));
                            reportTextWriter.Write("|{0}", @class.CoverageQuota.HasValue ? @class.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);

                            if (summaryResult.SupportsBranchCoverage)
                            {
                                reportTextWriter.Write("|{0}", @class.BranchCoverageQuota.HasValue ? @class.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                            }

                            if (proVersion)
                            {
                                reportTextWriter.Write("|{0}", @class.CodeElementCoverageQuota.HasValue ? @class.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                                reportTextWriter.Write("|{0}", @class.FullCodeElementCoverageQuota.HasValue ? @class.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                            }

                            reportTextWriter.WriteLine("|");
                        }

                        reportTextWriter.WriteLine();
                        reportTextWriter.WriteLine("</details>");
                    }
                }
                else
                {
                    reportTextWriter.WriteLine(ReportResources.NoCoveredAssemblies);
                }

                reportTextWriter.Flush();
            }
        }

        /// <summary>
        /// Inserts a separator after the given chunk length.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="chunkLength">The chunk length.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>The text with the separator.</returns>
        private static string InsertLineBreaks(string text, int chunkLength = 75, string separator = "<br/>")
        {
            if (text == null || text.Length <= chunkLength)
            {
                return text;
            }

            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < text.Length; index += chunkLength)
            {
                if (builder.Length > 0)
                {
                    builder.Append(separator);
                }

                builder.Append(text, index, Math.Min(chunkLength, text.Length - index));
            }

            return builder.ToString();
        }
    }
}
