using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates summary report in Text format (no reports for classes are generated).
    /// </summary>
    public class TextSummaryReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(TextSummaryReportBuilder));

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        public string ReportType => "TextSummary";

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

            string targetPath = Path.Combine(targetDirectory, "Summary.txt");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            using (var reportTextWriter = File.CreateText(targetPath))
            {
                var assembliesWithClasses = summaryResult.Assemblies
                    .Where(a => a.Classes.Any())
                    .ToArray();

                if (this.ReportContext.ReportConfiguration.Title != null)
                {
                    reportTextWriter.WriteLine($"{ReportResources.Summary} - {this.ReportContext.ReportConfiguration.Title}");
                }
                else
                {
                    reportTextWriter.WriteLine(ReportResources.Summary);
                }

                reportTextWriter.WriteLine("  {0} {1}", ReportResources.GeneratedOn, DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString());

                if (summaryResult.MinimumTimeStamp.HasValue || summaryResult.MaximumTimeStamp.HasValue)
                {
                    reportTextWriter.WriteLine("  {0} {1}", ReportResources.CoverageDate, summaryResult.CoverageDate());
                }

                reportTextWriter.WriteLine("  {0} {1}", ReportResources.Parser, summaryResult.UsedParser);
                reportTextWriter.WriteLine("  {0} {1}", ReportResources.Assemblies2, assembliesWithClasses.Count().ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine("  {0} {1}", ReportResources.Classes, assembliesWithClasses.SelectMany(a => a.Classes).Count().ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine("  {0} {1}", ReportResources.Files2, assembliesWithClasses.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count().ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine("  {0} {1}", ReportResources.Coverage2, summaryResult.CoverageQuota.HasValue ? summaryResult.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                reportTextWriter.WriteLine("  {0} {1}", ReportResources.CoveredLines, summaryResult.CoveredLines.ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine("  {0} {1}", ReportResources.UncoveredLines, (summaryResult.CoverableLines - summaryResult.CoveredLines).ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine("  {0} {1}", ReportResources.CoverableLines, summaryResult.CoverableLines.ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine("  {0} {1}", ReportResources.TotalLines, summaryResult.TotalLines.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));

                if (summaryResult.SupportsBranchCoverage)
                {
                    if (summaryResult.CoveredBranches.HasValue && summaryResult.TotalBranches.HasValue)
                    {
                        decimal? branchCoverage = summaryResult.BranchCoverageQuota;

                        if (branchCoverage.HasValue)
                        {
                            reportTextWriter.WriteLine("  {0} {1}", ReportResources.BranchCoverage2, $"{branchCoverage.Value.ToString(CultureInfo.InvariantCulture)}% ({summaryResult.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {summaryResult.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)})");
                        }

                        reportTextWriter.WriteLine("  {0} {1}", ReportResources.CoveredBranches2, summaryResult.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
                        reportTextWriter.WriteLine("  {0} {1}", ReportResources.TotalBranches, summaryResult.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
                    }
                }

                reportTextWriter.WriteLine("  {0} {1}", ReportResources.CodeElementCoverageQuota2, summaryResult.CodeElementCoverageQuota.HasValue ? $"{summaryResult.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}% ({summaryResult.CoveredCodeElements.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {summaryResult.TotalCodeElements.ToString(CultureInfo.InvariantCulture)})" : string.Empty);
                reportTextWriter.WriteLine("  {0} {1}", ReportResources.FullCodeElementCoverageQuota2, summaryResult.FullCodeElementCoverageQuota.HasValue ? $"{summaryResult.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}% ({summaryResult.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {summaryResult.TotalCodeElements.ToString(CultureInfo.InvariantCulture)})" : string.Empty);
                reportTextWriter.WriteLine("  {0} {1}", ReportResources.CoveredCodeElements, summaryResult.CoveredCodeElements.ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine("  {0} {1}", ReportResources.FullCoveredCodeElements, summaryResult.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture));
                reportTextWriter.WriteLine("  {0} {1}", ReportResources.TotalCodeElements, summaryResult.TotalCodeElements.ToString(CultureInfo.InvariantCulture));

                if (this.ReportContext.ReportConfiguration.Tag != null)
                {
                    reportTextWriter.WriteLine("  {0} {1}", ReportResources.Tag, this.ReportContext.ReportConfiguration.Tag);
                }

                if (assembliesWithClasses.Any())
                {
                    var maximumNameLength = assembliesWithClasses
                        .SelectMany(a => a.Classes).Select(c => c.DisplayName)
                        .Union(assembliesWithClasses.Select(a => a.Name))
                        .Max(n => n.Length);

                    foreach (var assembly in assembliesWithClasses)
                    {
                        string assemblyQuota = assembly.CoverageQuota.HasValue ? assembly.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty;
                        reportTextWriter.WriteLine();
                        reportTextWriter.WriteLine(
                            "{0}{1}  {2}",
                            assembly.Name,
                            new string(' ', maximumNameLength - assembly.Name.Length + 8 - assemblyQuota.Length),
                            assemblyQuota);

                        foreach (var @class in assembly.Classes)
                        {
                            string classQuota = @class.CoverageQuota.HasValue ? @class.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty;
                            reportTextWriter.WriteLine(
                                "  {0}{1}  {2}",
                                @class.DisplayName,
                                new string(' ', maximumNameLength - @class.DisplayName.Length + 6 - classQuota.Length),
                                classQuota);
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
