using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;
using Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Implementation of <see cref="IReportBuilder"/> that uses <see cref="ILatexRenderer"/> to create reports.
    /// </summary>
    public abstract class LatexReportBuilderBase : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(LatexReportBuilderBase));

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        public abstract string ReportType { get; }

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
        /// <param name="reportRenderer">The report renderer.</param>
        /// <param name="class">The class.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        public virtual void CreateClassReport(ILatexRenderer reportRenderer, Class @class, IEnumerable<FileAnalysis> fileAnalyses)
        {
            if (reportRenderer == null)
            {
                throw new ArgumentNullException(nameof(reportRenderer));
            }

            if (@class == null)
            {
                throw new ArgumentNullException(nameof(@class));
            }

            if (fileAnalyses == null)
            {
                throw new ArgumentNullException(nameof(fileAnalyses));
            }

            reportRenderer.BeginClassReport(this.CreateTargetDirectory(), @class.DisplayName);

            if (this.ReportContext.ReportConfiguration.Title != null)
            {
                reportRenderer.Header($"{ReportResources.Summary} - {this.ReportContext.ReportConfiguration.Title}");
            }
            else
            {
                reportRenderer.Header(ReportResources.Summary);
            }

            reportRenderer.BeginKeyValueTable();
            reportRenderer.KeyValueRow(ReportResources.Class, @class.DisplayName);
            reportRenderer.KeyValueRow(ReportResources.Assembly, @class.Assembly.ShortName);
            reportRenderer.KeyValueRow(ReportResources.Files3, @class.Files.Select(f => f.Path));
            reportRenderer.KeyValueRow(ReportResources.CoveredLines, @class.CoveredLines.ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.UncoveredLines, (@class.CoverableLines - @class.CoveredLines).ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.CoverableLines, @class.CoverableLines.ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.TotalLines, @class.TotalLines.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.Coverage2, @class.CoverageQuota.HasValue ? $"{@class.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}% ({@class.CoveredLines.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {@class.CoverableLines.ToString(CultureInfo.InvariantCulture)})" : string.Empty);

            if (@class.CoveredBranches.HasValue && @class.TotalBranches.HasValue)
            {
                reportRenderer.KeyValueRow(ReportResources.CoveredBranches2, @class.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
                reportRenderer.KeyValueRow(ReportResources.TotalBranches, @class.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));

                decimal? branchCoverage = @class.BranchCoverageQuota;

                if (branchCoverage.HasValue)
                {
                    reportRenderer.KeyValueRow(ReportResources.BranchCoverage2, $"{branchCoverage.Value.ToString(CultureInfo.InvariantCulture)}% ({@class.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {@class.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)})");
                }
            }

            reportRenderer.KeyValueRow(ReportResources.CoveredCodeElements, @class.CoveredCodeElements.ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.FullCoveredCodeElements, @class.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.TotalCodeElements, @class.TotalCodeElements.ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.CodeElementCoverageQuota2, @class.CodeElementCoverageQuota.HasValue ? $"{@class.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}% ({@class.CoveredCodeElements.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {@class.TotalCodeElements.ToString(CultureInfo.InvariantCulture)})" : string.Empty);
            reportRenderer.KeyValueRow(ReportResources.FullCodeElementCoverageQuota2, @class.FullCodeElementCoverageQuota.HasValue ? $"{@class.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}% ({@class.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {@class.TotalCodeElements.ToString(CultureInfo.InvariantCulture)})" : string.Empty);

            if (this.ReportContext.ReportConfiguration.Tag != null)
            {
                reportRenderer.KeyValueRow(ReportResources.Tag, this.ReportContext.ReportConfiguration.Tag);
            }

            reportRenderer.FinishTable();

            if (@class.Files.Any(f => f.MethodMetrics.Any()))
            {
                reportRenderer.Header(ReportResources.Metrics);
                reportRenderer.MetricsTable(@class);
            }

            reportRenderer.Header(ReportResources.Files);

            if (fileAnalyses.Any())
            {
                int fileIndex = 0;
                foreach (var fileAnalysis in fileAnalyses)
                {
                    reportRenderer.File(fileAnalysis.Path);

                    if (!string.IsNullOrEmpty(fileAnalysis.Error))
                    {
                        reportRenderer.Paragraph(fileAnalysis.Error);
                    }
                    else
                    {
                        reportRenderer.BeginLineAnalysisTable(new[] { string.Empty, "#", ReportResources.Line, string.Empty, ReportResources.Coverage });

                        foreach (var line in fileAnalysis.Lines)
                        {
                            reportRenderer.LineAnalysis(fileIndex, line);
                        }

                        reportRenderer.FinishTable();
                    }

                    fileIndex++;
                }
            }
            else
            {
                reportRenderer.Paragraph(ReportResources.NoFilesFound);
            }

            if (fileAnalyses.Any())
            {
                var testMethods = @class.Files
                    .SelectMany(f => f.TestMethods)
                    .Distinct()
                    .OrderBy(l => l.ShortName);

                var codeElementsByFileIndex = new Dictionary<int, IEnumerable<CodeElement>>();

                int fileIndex = 0;
                foreach (var file in @class.Files)
                {
                    codeElementsByFileIndex.Add(fileIndex++, file.CodeElements.OrderBy(c => c.FirstLine));
                }
            }
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="reportRenderer">The report renderer.</param>
        /// <param name="summaryResult">The summary result.</param>
        public virtual void CreateSummaryReport(ILatexRenderer reportRenderer, SummaryResult summaryResult)
        {
            if (reportRenderer == null)
            {
                throw new ArgumentNullException(nameof(reportRenderer));
            }

            if (summaryResult == null)
            {
                throw new ArgumentNullException(nameof(summaryResult));
            }

            string title = this.ReportContext.ReportConfiguration.Title != null ? $"{ReportResources.Summary} - {this.ReportContext.ReportConfiguration.Title}" : ReportResources.Summary;

            reportRenderer.BeginSummaryReport();
            reportRenderer.Header(title);

            reportRenderer.BeginKeyValueTable();
            reportRenderer.KeyValueRow(ReportResources.GeneratedOn, DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString());

            if (summaryResult.MinimumTimeStamp.HasValue || summaryResult.MaximumTimeStamp.HasValue)
            {
                reportRenderer.KeyValueRow(ReportResources.CoverageDate, summaryResult.CoverageDate());
            }

            var assembliesWithClasses = summaryResult.Assemblies
                .Where(a => a.Classes.Any())
                .ToArray();

            reportRenderer.KeyValueRow(ReportResources.Parser, summaryResult.UsedParser);
            reportRenderer.KeyValueRow(ReportResources.Assemblies2, assembliesWithClasses.Count().ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.Classes, assembliesWithClasses.SelectMany(a => a.Classes).Count().ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.Files2, assembliesWithClasses.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count().ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.CoveredLines, summaryResult.CoveredLines.ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.UncoveredLines, (summaryResult.CoverableLines - summaryResult.CoveredLines).ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.CoverableLines, summaryResult.CoverableLines.ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.TotalLines, summaryResult.TotalLines.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.Coverage2, summaryResult.CoverageQuota.HasValue ? $"{summaryResult.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}% ({summaryResult.CoveredLines.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {summaryResult.CoverableLines.ToString(CultureInfo.InvariantCulture)})" : string.Empty);

            if (summaryResult.CoveredBranches.HasValue && summaryResult.TotalBranches.HasValue)
            {
                reportRenderer.KeyValueRow(ReportResources.CoveredBranches2, summaryResult.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
                reportRenderer.KeyValueRow(ReportResources.TotalBranches, summaryResult.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));

                decimal? branchCoverage = summaryResult.BranchCoverageQuota;

                if (branchCoverage.HasValue)
                {
                    reportRenderer.KeyValueRow(ReportResources.BranchCoverage2, $"{branchCoverage.Value.ToString(CultureInfo.InvariantCulture)}% ({summaryResult.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {summaryResult.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)})");
                }
            }

            reportRenderer.KeyValueRow(ReportResources.CoveredCodeElements, summaryResult.CoveredCodeElements.ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.FullCoveredCodeElements, summaryResult.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.TotalCodeElements, summaryResult.TotalCodeElements.ToString(CultureInfo.InvariantCulture));
            reportRenderer.KeyValueRow(ReportResources.CodeElementCoverageQuota2, summaryResult.CodeElementCoverageQuota.HasValue ? $"{summaryResult.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}% ({summaryResult.CoveredCodeElements.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {summaryResult.TotalCodeElements.ToString(CultureInfo.InvariantCulture)})" : string.Empty);
            reportRenderer.KeyValueRow(ReportResources.FullCodeElementCoverageQuota2, summaryResult.FullCodeElementCoverageQuota.HasValue ? $"{summaryResult.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}% ({summaryResult.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {summaryResult.TotalCodeElements.ToString(CultureInfo.InvariantCulture)})" : string.Empty);

            if (this.ReportContext.ReportConfiguration.Tag != null)
            {
                reportRenderer.KeyValueRow(ReportResources.Tag, this.ReportContext.ReportConfiguration.Tag);
            }

            reportRenderer.FinishTable();

            var sumableMetrics = summaryResult.SumableMetrics;

            if (sumableMetrics.Count > 0)
            {
                reportRenderer.Header(ReportResources.Metrics);

                var methodMetric = new MethodMetric(ReportResources.Total, ReportResources.Total, sumableMetrics);
                reportRenderer.MetricsTable(new[] { methodMetric });
            }

            if (this.ReportContext.RiskHotspotAnalysisResult != null
                && this.ReportContext.RiskHotspotAnalysisResult.CodeCodeQualityMetricsAvailable)
            {
                reportRenderer.Header(ReportResources.RiskHotspots);

                if (this.ReportContext.RiskHotspotAnalysisResult.RiskHotspots.Count > 0)
                {
                    reportRenderer.RiskHotspots(this.ReportContext.RiskHotspotAnalysisResult.RiskHotspots);
                }
                else
                {
                    reportRenderer.Paragraph(ReportResources.NoRiskHotspots);
                }
            }

            reportRenderer.Header(ReportResources.Coverage3);

            if (assembliesWithClasses.Any())
            {
                reportRenderer.BeginSummaryTable(summaryResult.SupportsBranchCoverage, true);

                foreach (var assembly in assembliesWithClasses)
                {
                    reportRenderer.SummaryAssembly(assembly, summaryResult.SupportsBranchCoverage, true);

                    foreach (var @class in assembly.Classes)
                    {
                        reportRenderer.SummaryClass(@class, summaryResult.SupportsBranchCoverage, true);
                    }
                }

                reportRenderer.FinishTable();
            }
            else
            {
                reportRenderer.Paragraph(ReportResources.NoCoveredAssemblies);
            }

            reportRenderer.SaveSummaryReport(this.CreateTargetDirectory());
        }

        /// <summary>
        /// Creates a class report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        public abstract void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses);

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        public abstract void CreateSummaryReport(SummaryResult summaryResult);

        /// <summary>
        /// Creates the target directory.
        /// </summary>
        /// <returns>The target directory.</returns>
        protected string CreateTargetDirectory()
        {
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
                        throw;
                    }
                }
            }

            return targetDirectory;
        }
    }
}
