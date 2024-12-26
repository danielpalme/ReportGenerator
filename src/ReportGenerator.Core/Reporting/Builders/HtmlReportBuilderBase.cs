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
using Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Implementation of <see cref="IReportBuilder"/> that uses <see cref="IHtmlRenderer"/> to create reports.
    /// </summary>
    public abstract class HtmlReportBuilderBase : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(HtmlReportBuilderBase));

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
        public virtual void CreateClassReport(IHtmlRenderer reportRenderer, Class @class, IEnumerable<FileAnalysis> fileAnalyses)
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

            bool proVersion = this.ReportContext.ReportConfiguration.License.DetermineLicenseType() == LicenseType.Pro;

            string additionalTitle = this.ReportContext.ReportConfiguration.Title != null ? $"{this.ReportContext.ReportConfiguration.Title} - " : null;

            reportRenderer.BeginClassReport(this.CreateTargetDirectory(), @class.Assembly, @class.Name, @class.DisplayName, additionalTitle);

            if (this.ReportContext.ReportConfiguration.Title != null)
            {
                reportRenderer.HeaderWithBackLink($"{ReportResources.Summary} - {this.ReportContext.ReportConfiguration.Title}");
            }
            else
            {
                reportRenderer.HeaderWithBackLink(ReportResources.Summary);
            }

            var infoCardItems = new List<CardLineItem>()
            {
                new CardLineItem(ReportResources.Class, @class.DisplayName, null, CardLineItemAlignment.Left),
                new CardLineItem(ReportResources.Assembly, @class.Assembly.ShortName, null, CardLineItemAlignment.Left),
                new CardLineItem(ReportResources.Files3, @class.Files.Select(f => f.Path).ToArray())
            };

            if (this.ReportContext.ReportConfiguration.Tag != null)
            {
                infoCardItems.Add(new CardLineItem(ReportResources.Tag, this.ReportContext.ReportConfiguration.Tag, null, CardLineItemAlignment.Left));
            }

            var infoCard = new Card(
                ReportResources.Information,
                string.Empty,
                null,
                infoCardItems.ToArray());

            reportRenderer.Cards(new[] { infoCard });

            var cards = new List<Card>()
            {
                new Card(
                    ReportResources.Coverage,
                    @class.CoverageQuota.HasValue ? $"{Math.Floor(@class.CoverageQuota.Value).ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable,
                    @class.CoverageQuota,
                    new CardLineItem(ReportResources.CoveredLines, @class.CoveredLines.ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(ReportResources.UncoveredLines, (@class.CoverableLines - @class.CoveredLines).ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(ReportResources.CoverableLines, @class.CoverableLines.ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(ReportResources.TotalLines, @class.TotalLines.GetValueOrDefault().ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(
                        ReportResources.Coverage2,
                        @class.CoverageQuota.HasValue ? $"{@class.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable,
                        @class.CoverageQuota.HasValue ? $"{@class.CoveredLines.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {@class.CoverableLines.ToString(CultureInfo.InvariantCulture)}" : ReportResources.NotAvailable))
            };

            if (@class.CoveredBranches.HasValue && @class.TotalBranches.HasValue)
            {
                cards.Add(new Card(
                    ReportResources.BranchCoverage,
                    @class.BranchCoverageQuota.HasValue ? $"{Math.Floor(@class.BranchCoverageQuota.Value).ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable,
                    @class.BranchCoverageQuota,
                    new CardLineItem(ReportResources.CoveredBranches2, @class.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(ReportResources.TotalBranches, @class.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(
                        ReportResources.BranchCoverage2,
                        @class.BranchCoverageQuota.HasValue ? $"{@class.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable,
                        @class.BranchCoverageQuota.HasValue ? $"{@class.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {@class.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)}" : ReportResources.NotAvailable)));
            }

            if (proVersion)
            {
                cards.Add(new Card(
                    ReportResources.CodeElementCoverageQuota,
                    @class.CodeElementCoverageQuota.HasValue ? $"{Math.Floor(@class.CodeElementCoverageQuota.Value).ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable,
                    @class.CodeElementCoverageQuota,
                    new CardLineItem(ReportResources.CoveredCodeElements, @class.CoveredCodeElements.ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(ReportResources.FullCoveredCodeElements, @class.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(ReportResources.TotalCodeElements, @class.TotalCodeElements.ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(
                        ReportResources.CodeElementCoverageQuota2,
                        @class.CodeElementCoverageQuota.HasValue ? $"{@class.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable,
                        @class.CodeElementCoverageQuota.HasValue ? $"{@class.CoveredCodeElements.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {@class.TotalCodeElements.ToString(CultureInfo.InvariantCulture)}" : ReportResources.NotAvailable),
                    new CardLineItem(
                        ReportResources.FullCodeElementCoverageQuota2,
                        @class.FullCodeElementCoverageQuota.HasValue ? $"{@class.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable,
                        @class.FullCodeElementCoverageQuota.HasValue ? $"{@class.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {@class.TotalCodeElements.ToString(CultureInfo.InvariantCulture)}" : ReportResources.NotAvailable)));
            }
            else
            {
                cards.Add(new Card(ReportResources.CodeElementCoverageQuota));
            }

            reportRenderer.Cards(cards);

            if (@class.HistoricCoverages.Any(h => h.CoverageQuota.HasValue || h.BranchCoverageQuota.HasValue || h.CodeElementCoverageQuota.HasValue))
            {
                reportRenderer.Header(ReportResources.History);
                reportRenderer.Chart(@class.HistoricCoverages, proVersion);
            }

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

            reportRenderer.AddFooter();

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

                reportRenderer.TestMethods(testMethods, fileAnalyses, codeElementsByFileIndex);
            }

            reportRenderer.SaveClassReport(this.CreateTargetDirectory(), @class.Name);
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="reportRenderer">The report renderer.</param>
        /// <param name="summaryResult">The summary result.</param>
        public virtual void CreateSummaryReport(IHtmlRenderer reportRenderer, SummaryResult summaryResult)
        {
            if (reportRenderer == null)
            {
                throw new ArgumentNullException(nameof(reportRenderer));
            }

            if (summaryResult == null)
            {
                throw new ArgumentNullException(nameof(summaryResult));
            }

            bool proVersion = this.ReportContext.ReportConfiguration.License.DetermineLicenseType() == LicenseType.Pro;

            string title = this.ReportContext.ReportConfiguration.Title != null ? $"{ReportResources.Summary} - {this.ReportContext.ReportConfiguration.Title}" : ReportResources.Summary;

            reportRenderer.BeginSummaryReport(this.CreateTargetDirectory(), null, title);
            reportRenderer.HeaderWithGithubLinks(title);

            var assembliesWithClasses = summaryResult.Assemblies
                .Where(a => a.Classes.Any())
                .ToArray();

            var infoCardItems = new List<CardLineItem>()
            {
                new CardLineItem(ReportResources.Parser, summaryResult.UsedParser, null, CardLineItemAlignment.Left),
                new CardLineItem(ReportResources.Assemblies2, assembliesWithClasses.Count().ToString(CultureInfo.InvariantCulture), null),
                new CardLineItem(ReportResources.Classes, assembliesWithClasses.SelectMany(a => a.Classes).Count().ToString(CultureInfo.InvariantCulture), null),
                new CardLineItem(ReportResources.Files2, assembliesWithClasses.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count().ToString(CultureInfo.InvariantCulture), null)
            };

            if (this.ReportContext.ReportConfiguration.Tag != null)
            {
                infoCardItems.Add(new CardLineItem(ReportResources.Tag, this.ReportContext.ReportConfiguration.Tag, null, CardLineItemAlignment.Left));
            }

            if (summaryResult.MinimumTimeStamp.HasValue || summaryResult.MaximumTimeStamp.HasValue)
            {
                infoCardItems.Add(new CardLineItem(ReportResources.CoverageDate, summaryResult.CoverageDate(), null, CardLineItemAlignment.Left));
            }

            var cards = new List<Card>()
            {
                new Card(
                    ReportResources.Information,
                    string.Empty,
                    null,
                    infoCardItems.ToArray()),
                new Card(
                    ReportResources.Coverage,
                    summaryResult.CoverageQuota.HasValue ? $"{Math.Floor(summaryResult.CoverageQuota.Value).ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable,
                    summaryResult.CoverageQuota,
                    new CardLineItem(ReportResources.CoveredLines, summaryResult.CoveredLines.ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(ReportResources.UncoveredLines, (summaryResult.CoverableLines - summaryResult.CoveredLines).ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(ReportResources.CoverableLines, summaryResult.CoverableLines.ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(ReportResources.TotalLines, summaryResult.TotalLines.GetValueOrDefault().ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(
                        ReportResources.Coverage2,
                        summaryResult.CoverageQuota.HasValue ? $"{summaryResult.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable,
                        summaryResult.CoverageQuota.HasValue ? $"{summaryResult.CoveredLines.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {summaryResult.CoverableLines.ToString(CultureInfo.InvariantCulture)}" : ReportResources.NotAvailable))
            };

            if (summaryResult.CoveredBranches.HasValue && summaryResult.TotalBranches.HasValue)
            {
                cards.Add(new Card(
                    ReportResources.BranchCoverage,
                    summaryResult.BranchCoverageQuota.HasValue ? $"{Math.Floor(summaryResult.BranchCoverageQuota.Value).ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable,
                    summaryResult.BranchCoverageQuota,
                    new CardLineItem(ReportResources.CoveredBranches2, summaryResult.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(ReportResources.TotalBranches, summaryResult.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(
                        ReportResources.BranchCoverage2,
                        summaryResult.BranchCoverageQuota.HasValue ? $"{summaryResult.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable,
                        summaryResult.BranchCoverageQuota.HasValue ? $"{summaryResult.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {summaryResult.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)}" : ReportResources.NotAvailable)));
            }

            if (proVersion)
            {
                cards.Add(new Card(
                    ReportResources.CodeElementCoverageQuota,
                    summaryResult.CodeElementCoverageQuota.HasValue ? $"{Math.Floor(summaryResult.CodeElementCoverageQuota.Value).ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable,
                    summaryResult.CodeElementCoverageQuota,
                    new CardLineItem(ReportResources.CoveredCodeElements, summaryResult.CoveredCodeElements.ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(ReportResources.FullCoveredCodeElements, summaryResult.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(ReportResources.TotalCodeElements, summaryResult.TotalCodeElements.ToString(CultureInfo.InvariantCulture), null),
                    new CardLineItem(
                        ReportResources.CodeElementCoverageQuota2,
                        summaryResult.CodeElementCoverageQuota.HasValue ? $"{summaryResult.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable,
                        summaryResult.CodeElementCoverageQuota.HasValue ? $"{summaryResult.CoveredCodeElements.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {summaryResult.TotalCodeElements.ToString(CultureInfo.InvariantCulture)}" : ReportResources.NotAvailable),
                    new CardLineItem(
                        ReportResources.FullCodeElementCoverageQuota2,
                        summaryResult.FullCodeElementCoverageQuota.HasValue ? $"{summaryResult.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}%" : ReportResources.NotAvailable,
                        summaryResult.FullCodeElementCoverageQuota.HasValue ? $"{summaryResult.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture)} {ReportResources.Of} {summaryResult.TotalCodeElements.ToString(CultureInfo.InvariantCulture)}" : ReportResources.NotAvailable)));
            }
            else
            {
                cards.Add(new Card(ReportResources.CodeElementCoverageQuota));
            }

            reportRenderer.Cards(cards);
            var historicCoverages = HistoricCoverages.GetOverallHistoricCoverages(this.ReportContext.OverallHistoricCoverages);
            if (historicCoverages.Any(h => h.CoverageQuota.HasValue || h.BranchCoverageQuota.HasValue || h.CodeElementCoverageQuota.HasValue))
            {
                reportRenderer.Header(ReportResources.History);
                reportRenderer.Chart(historicCoverages, proVersion);
            }

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
                    reportRenderer.BeginRiskHotspots();
                    reportRenderer.RiskHotspots(this.ReportContext.RiskHotspotAnalysisResult.RiskHotspots);
                    reportRenderer.FinishRiskHotspots();
                }
                else
                {
                    // Angular element has to be present
                    reportRenderer.BeginRiskHotspots();
                    reportRenderer.FinishRiskHotspots();

                    reportRenderer.Paragraph(ReportResources.NoRiskHotspots);
                }
            }
            else
            {
                // Angular element has to be present
                reportRenderer.BeginRiskHotspots();
                reportRenderer.FinishRiskHotspots();
            }

            reportRenderer.Header(ReportResources.Coverage3);

            if (assembliesWithClasses.Any())
            {
                reportRenderer.BeginSummaryTable();
                reportRenderer.BeginSummaryTable(summaryResult.SupportsBranchCoverage, proVersion);

                foreach (var assembly in assembliesWithClasses)
                {
                    reportRenderer.SummaryAssembly(assembly, summaryResult.SupportsBranchCoverage, proVersion);

                    foreach (var @class in assembly.Classes)
                    {
                        reportRenderer.SummaryClass(@class, summaryResult.SupportsBranchCoverage, proVersion);
                    }
                }

                reportRenderer.FinishTable();
                reportRenderer.FinishSummaryTable();
            }
            else
            {
                // Angular element has to be present
                reportRenderer.BeginSummaryTable();
                reportRenderer.FinishSummaryTable();

                reportRenderer.Paragraph(ReportResources.NoCoveredAssemblies);
            }

            reportRenderer.CustomSummary(
                assembliesWithClasses,
                this.ReportContext.RiskHotspotAnalysisResult.RiskHotspots,
                summaryResult.SupportsBranchCoverage,
                proVersion);

            reportRenderer.AddFooter();
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
