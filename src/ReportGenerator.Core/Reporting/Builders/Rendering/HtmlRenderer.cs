using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.ObjectPool;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering
{
    /// <summary>
    /// HTML report renderer.
    /// </summary>
    internal class HtmlRenderer : IHtmlRenderer, IDisposable
    {
        /// <summary>
        /// The link to the static CSS file.
        /// </summary>
        private const string CssLink = "<link rel=\"stylesheet\" type=\"text/css\" href=\"report.css\" />";

        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(HtmlRenderer));

        /// <summary>
        /// Indicates whether which CSS and Javascript files have already been written.
        /// </summary>
        private static readonly HashSet<string> WrittenCssAndJavascriptFiles = new HashSet<string>();

        /// <summary>
        /// Dictionary containing the filenames of the class reports by class.
        /// </summary>
        private readonly IDictionary<string, string> fileNameByClass;

        /// <summary>
        /// Indicates that only a summary report is created (no class reports).
        /// </summary>
        private readonly bool onlySummary;

        /// <summary>
        /// Defines how CSS and JavaScript are referenced.
        /// </summary>
        private readonly HtmlMode htmlMode;

        /// <summary>
        /// Contains report specific JavaScript content.
        /// </summary>
        private readonly StringBuilder javaScriptContent;

        /// <summary>
        /// The css file resource.
        /// </summary>
        private readonly string cssFileResource;

        /// <summary>
        /// Optional additional CSS file resources.
        /// </summary>
        private readonly string[] additionalCssFileResources;

        /// <summary>
        /// Indicates that JavaScript was generated.
        /// </summary>
        private bool javaScriptGenerated;

        /// <summary>
        /// The report builder.
        /// </summary>
        private TextWriter reportTextWriter;

        /// <summary>
        /// Indicates that that a class report is created (not the summary page).
        /// </summary>
        private bool classReport;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlRenderer" /> class.
        /// </summary>
        /// <param name="fileNameByClass">Dictionary containing the filenames of the class reports by class.</param>
        /// <param name="onlySummary">if set to <c>true</c> only a summary report is created (no class reports).</param>
        /// <param name="htmlMode">Defines how CSS and JavaScript are referenced.</param>
        /// <param name="cssFileResource">Optional CSS file resource.</param>
        /// <param name="additionalCssFileResource">Optional additional CSS file resource.</param>
        internal HtmlRenderer(
            IDictionary<string, string> fileNameByClass,
            bool onlySummary,
            HtmlMode htmlMode,
            string cssFileResource = "custom.css",
            string additionalCssFileResource = "custom_adaptive.css")
        {
            this.fileNameByClass = fileNameByClass;
            this.onlySummary = onlySummary;
            this.htmlMode = htmlMode;
            this.javaScriptContent = StringBuilderCache.Get();
            this.cssFileResource = cssFileResource;
            this.additionalCssFileResources = additionalCssFileResource == null ? new string[0] : new[] { additionalCssFileResource };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlRenderer" /> class.
        /// </summary>
        /// <param name="fileNameByClass">Dictionary containing the filenames of the class reports by class.</param>
        /// <param name="onlySummary">if set to <c>true</c> only a summary report is created (no class reports).</param>
        /// <param name="htmlMode">Defines how CSS and JavaScript are referenced.</param>
        /// <param name="additionalCssFileResources">Optional additional CSS file resources.</param>
        /// <param name="cssFileResource">Optional CSS file resource.</param>
        internal HtmlRenderer(
            IDictionary<string, string> fileNameByClass,
            bool onlySummary,
            HtmlMode htmlMode,
            string[] additionalCssFileResources,
            string cssFileResource = "custom.css")
        {
            this.fileNameByClass = fileNameByClass;
            this.onlySummary = onlySummary;
            this.htmlMode = htmlMode;
            this.javaScriptContent = StringBuilderCache.Get();
            this.additionalCssFileResources = additionalCssFileResources ?? new string[0];
            this.cssFileResource = cssFileResource;
        }

        /// <inheritdoc />
        public void BeginSummaryReport(string targetDirectory, string fileName, string title)
        {
            string targetPath = Path.Combine(targetDirectory, this.onlySummary ? "summary.html" : "index.html");

            if (fileName != null)
            {
                targetPath = Path.Combine(targetDirectory, fileName);
            }

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);
            this.CreateTextWriter(targetPath);

            this.WriteHtmlStart(this.reportTextWriter, title, ReportResources.CoverageReport);
        }

        /// <inheritdoc />
        public void BeginClassReport(string targetDirectory, Assembly assembly, string className, string classDisplayName, string additionalTitle)
        {
            this.classReport = true;

            string targetPath = this.GetClassReportFilename(assembly, className);

            Logger.DebugFormat(Resources.WritingReportFile, targetPath);
            this.CreateTextWriter(Path.Combine(targetDirectory, targetPath));

            this.WriteHtmlStart(this.reportTextWriter, classDisplayName, additionalTitle + ReportResources.CoverageReport);
        }

        /// <inheritdoc />
        public void Cards(IEnumerable<Card> cards)
        {
            this.reportTextWriter.WriteLine("<div class=\"card-group\">");

            foreach (var card in cards)
            {
                this.reportTextWriter.WriteLine("<div class=\"card\">");

                if (!string.IsNullOrWhiteSpace(card.Title))
                {
                    this.reportTextWriter.WriteLine("<div class=\"card-header\">{0}</div>", card.Title);
                }

                this.reportTextWriter.WriteLine("<div class=\"card-body\">");
                if (!string.IsNullOrWhiteSpace(card.SubTitle))
                {
                    string clazz = string.Empty;

                    if (card.SubTitlePercentage.HasValue)
                    {
                        int uncovered = 100 - (int)Math.Round(card.SubTitlePercentage.Value, 0);

                        clazz = $" cardpercentagebar cardpercentagebar{uncovered}";
                    }

                    this.reportTextWriter.WriteLine("<div class=\"large{0}\">{1}</div>", clazz, WebUtility.HtmlEncode(card.SubTitle));
                }

                if (card.ProRequired)
                {
                    this.reportTextWriter.WriteLine("<div class=\"center\">");
                    this.reportTextWriter.WriteLine("<p>{0}</p>", ReportResources.MethodCoverageProVersion);
                    this.reportTextWriter.WriteLine("<a class=\"pro-button\" href=\"https://reportgenerator.io/pro\" target=\"_blank\">{0}</a>", ReportResources.MethodCoverageProButton);
                    this.reportTextWriter.WriteLine("</div>");
                }
                else
                {
                    this.reportTextWriter.WriteLine("<div class=\"table\">");
                    this.reportTextWriter.WriteLine("<table>");

                    foreach (var row in card.Rows)
                    {
                        this.reportTextWriter.WriteLine("<tr>");
                        this.reportTextWriter.WriteLine("<th>{0}</th>", WebUtility.HtmlEncode(row.Header));
                        if (row.Links != null)
                        {
                            int fileNumber = 1;

                            bool usePrefix = row.Links.Count > 1;

                            string value = string.Join("<br />", row.Links.Select(v => string.Format(CultureInfo.InvariantCulture, "<a href=\"#{0}\" class=\"navigatetohash\">{1}</a>", WebUtility.HtmlEncode(StringHelper.ReplaceNonLetterChars(v)), WebUtility.HtmlEncode((usePrefix ? $"{ReportResources.File} {fileNumber++}: " : string.Empty) + v))));

                            this.reportTextWriter.WriteLine(
                                "<td class=\"overflow-wrap\">{0}</td>",
                                value);
                        }
                        else
                        {
                            this.reportTextWriter.WriteLine(
                                "<td class=\"limit-width {0}\" title=\"{1}\">{2}</td>",
                                row.Alignment == CardLineItemAlignment.Right ? "right" : string.Empty,
                                WebUtility.HtmlEncode(row.Tooltip ?? row.Text),
                                WebUtility.HtmlEncode(row.Text));
                        }

                        this.reportTextWriter.WriteLine("</tr>");
                    }

                    this.reportTextWriter.WriteLine("</table>");
                    this.reportTextWriter.WriteLine("</div>");
                }

                this.reportTextWriter.WriteLine("</div>");
                this.reportTextWriter.WriteLine("</div>");
            }

            this.reportTextWriter.WriteLine("</div>");
        }

        /// <inheritdoc />
        public void Header(string text)
        {
            this.reportTextWriter.WriteLine("<h1>{0}</h1>", WebUtility.HtmlEncode(text));
        }

        /// <inheritdoc />
        public void HeaderWithGithubLinks(string text)
        {
            this.reportTextWriter.WriteLine(
                "<h1>{0}<a class=\"button\" href=\"https://github.com/danielpalme/ReportGenerator\" title=\"{1}\"><i class=\"icon-star\"></i>{2}</a><a class=\"button\" href=\"https://github.com/sponsors/danielpalme\" title=\"{3}\"><i class=\"icon-sponsor\"></i>{4}</a></h1>",
                WebUtility.HtmlEncode(text),
                WebUtility.HtmlEncode(ReportResources.StarTooltip),
                WebUtility.HtmlEncode(ReportResources.Star),
                WebUtility.HtmlEncode(ReportResources.SponsorTooltip),
                WebUtility.HtmlEncode(ReportResources.Sponsor));
        }

        /// <inheritdoc />
        public void HeaderWithBackLink(string text)
        {
            this.reportTextWriter.WriteLine("<h1><a href=\"index.html\" class=\"back\">&lt;</a> {0}</h1>", WebUtility.HtmlEncode(text));
        }

        /// <inheritdoc />
        public void TestMethods(IEnumerable<TestMethod> testMethods, IEnumerable<FileAnalysis> fileAnalyses, IDictionary<int, IEnumerable<CodeElement>> codeElementsByFileIndex)
        {
            if (testMethods == null)
            {
                throw new ArgumentNullException(nameof(testMethods));
            }

            if (!testMethods.Any() && codeElementsByFileIndex.Count == 0)
            {
                return;
            }

            // Close 'containerleft' and begin 'containerright'
            this.reportTextWriter.WriteLine("</div>");
            this.reportTextWriter.WriteLine("<div class=\"containerright\">");
            this.reportTextWriter.WriteLine("<div class=\"containerrightfixed\">");

            if (testMethods.Any())
            {
                this.reportTextWriter.WriteLine("<h1>{0}</h1>", WebUtility.HtmlEncode(ReportResources.Testmethods));

                int coverableLines = fileAnalyses.SafeSum(f => f.Lines.Count(l => l.LineVisitStatus != LineVisitStatus.NotCoverable));
                int coveredLines = fileAnalyses.SafeSum(f => f.Lines.Count(l => l.LineVisitStatus > LineVisitStatus.NotCovered));
                decimal? coverage = (coverableLines == 0) ? (decimal?)null : MathExtensions.CalculatePercentage(coveredLines, coverableLines);

                int? coverageRounded = null;

                if (coverage.HasValue)
                {
                    coverageRounded = (int)coverage.Value;
                    coverageRounded -= coverageRounded % 10;
                }

                this.reportTextWriter.WriteLine(
                    "<label id=\"AllTestMethods\" class=\"testmethod percentagebar percentagebar{0}\" title=\"{1}{2}\"><input type=\"radio\" name=\"method\" value=\"AllTestMethods\" class=\"switchtestmethod\" checked=\"checked\" />{2}</label>",
                    coverage.HasValue ? coverageRounded.ToString() : "undefined",
                    coverage.HasValue ? ReportResources.Coverage2 + " " + coverage.Value.ToString(CultureInfo.InvariantCulture) + "% - " : string.Empty,
                    WebUtility.HtmlEncode(ReportResources.All));

                foreach (var testMethod in testMethods)
                {
                    coveredLines = fileAnalyses.SafeSum(f => f.Lines.Count(l => l.LineCoverageByTestMethod.ContainsKey(testMethod) && l.LineCoverageByTestMethod[testMethod].LineVisitStatus > LineVisitStatus.NotCovered));
                    coverage = (coverableLines == 0) ? (decimal?)null : MathExtensions.CalculatePercentage(coveredLines, coverableLines);

                    coverageRounded = null;

                    if (coverage.HasValue)
                    {
                        coverageRounded = (int)coverage.Value;
                        coverageRounded -= coverageRounded % 10;
                    }

                    this.reportTextWriter.WriteLine(
                        "<br /><label id=\"M{3}\" class=\"testmethod percentagebar percentagebar{0}\" title=\"{1}{2}\"><input type=\"radio\" name=\"method\" value=\"M{3}\" class=\"switchtestmethod\" />{4}</label>",
                        coverage.HasValue ? coverageRounded.ToString() : "undefined",
                        coverage.HasValue ? ReportResources.Coverage2 + " " + coverage.Value.ToString(CultureInfo.InvariantCulture) + "% - " : string.Empty,
                        WebUtility.HtmlEncode(testMethod.Name),
                        testMethod.Id,
                        WebUtility.HtmlEncode(testMethod.ShortName));
                }
            }

            if (codeElementsByFileIndex.Count > 0)
            {
                this.reportTextWriter.WriteLine("<h1>{0}</h1>", WebUtility.HtmlEncode(ReportResources.MethodsProperties));

                foreach (var item in codeElementsByFileIndex)
                {
                    foreach (var codeElement in item.Value)
                    {
                        int? coverageRounded = null;

                        if (codeElement.CoverageQuota.HasValue)
                        {
                            coverageRounded = (int)codeElement.CoverageQuota.Value;
                            coverageRounded -= coverageRounded % 10;
                        }

                        string prefix = fileAnalyses.Count() > 1 ? $"{ReportResources.File} {item.Key + 1}: " : string.Empty;
                        this.reportTextWriter.WriteLine(
                            "<a href=\"#file{0}_line{1}\" class=\"navigatetohash percentagebar percentagebar{2}\" title=\"{3}{4}\"><i class=\"icon-{5}\"></i>{4}</a><br />",
                            item.Key,
                            codeElement.FirstLine,
                            codeElement.CoverageQuota.HasValue ? coverageRounded.ToString() : "undefined",
                            prefix + (codeElement.CoverageQuota.HasValue ? ReportResources.Coverage2 + " " + codeElement.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "% - " : string.Empty),
                            WebUtility.HtmlEncode(codeElement.Name),
                            codeElement.CodeElementType == CodeElementType.Method ? "cube" : "wrench");
                    }
                }
            }

            this.reportTextWriter.WriteLine("<br/></div>");
        }

        /// <inheritdoc />
        public void File(string path)
        {
            this.reportTextWriter.WriteLine("<h2 id=\"{0}\">{1}</h2>", WebUtility.HtmlEncode(StringHelper.ReplaceNonLetterChars(path)), WebUtility.HtmlEncode(path));
        }

        /// <inheritdoc />
        public void Paragraph(string text)
        {
            this.reportTextWriter.WriteLine("<p>{0}</p>", WebUtility.HtmlEncode(text));
        }

        /// <inheritdoc />
        public void BeginSummaryTable()
        {
            this.reportTextWriter.WriteLine("<coverage-info>");
        }

        /// <inheritdoc />
        public void FinishSummaryTable()
        {
            this.reportTextWriter.WriteLine("</coverage-info>");
        }

        /// <inheritdoc />
        public void BeginSummaryTable(bool branchCoverageAvailable, bool methodCoverageAvailable)
        {
            this.reportTextWriter.WriteLine("<div class=\"table-responsive\">");
            this.reportTextWriter.WriteLine("<table class=\"overview table-fixed stripped\">");
            this.reportTextWriter.WriteLine("<colgroup>");
            this.reportTextWriter.WriteLine("<col class=\"column-min-200\" />");
            this.reportTextWriter.WriteLine("<col class=\"column90\" />");
            this.reportTextWriter.WriteLine("<col class=\"column105\" />");
            this.reportTextWriter.WriteLine("<col class=\"column100\" />");
            this.reportTextWriter.WriteLine("<col class=\"column70\" />");
            this.reportTextWriter.WriteLine("<col class=\"column60\" />");
            this.reportTextWriter.WriteLine("<col class=\"column112\" />");
            if (branchCoverageAvailable)
            {
                this.reportTextWriter.WriteLine("<col class=\"column90\" />");
                this.reportTextWriter.WriteLine("<col class=\"column70\" />");
                this.reportTextWriter.WriteLine("<col class=\"column60\" />");
                this.reportTextWriter.WriteLine("<col class=\"column112\" />");
            }

            if (methodCoverageAvailable)
            {
                this.reportTextWriter.WriteLine("<col class=\"column90\" />");
                this.reportTextWriter.WriteLine("<col class=\"column70\" />");
                this.reportTextWriter.WriteLine("<col class=\"column60\" />");
                this.reportTextWriter.WriteLine("<col class=\"column112\" />");

                this.reportTextWriter.WriteLine("<col class=\"column90\" />");
                this.reportTextWriter.WriteLine("<col class=\"column70\" />");
                this.reportTextWriter.WriteLine("<col class=\"column60\" />");
                this.reportTextWriter.WriteLine("<col class=\"column112\" />");
            }

            this.reportTextWriter.WriteLine("</colgroup>");

            this.reportTextWriter.WriteLine("<thead>");
            this.reportTextWriter.Write("<tr class=\"header\">");
            this.reportTextWriter.Write(
                "<th></th><th colspan=\"6\" class=\"center\">{0}</th>",
                WebUtility.HtmlEncode(ReportResources.Coverage));

            if (branchCoverageAvailable)
            {
                this.reportTextWriter.Write(
                    "<th colspan=\"4\" class=\"center\">{0}</th>",
                    WebUtility.HtmlEncode(ReportResources.BranchCoverage));
            }

            if (methodCoverageAvailable)
            {
                this.reportTextWriter.Write(
                "<th colspan=\"4\" class=\"center\">{0}</th>",
                WebUtility.HtmlEncode(ReportResources.CodeElementCoverageQuota));

                this.reportTextWriter.Write(
                "<th colspan=\"4\" class=\"center\">{0}</th>",
                WebUtility.HtmlEncode(ReportResources.FullCodeElementCoverageQuota));
            }

            this.reportTextWriter.WriteLine("</tr>");

            this.reportTextWriter.Write(
                "<tr><th>{0}</th><th class=\"right\">{1}</th><th class=\"right\">{2}</th><th class=\"right\">{3}</th><th class=\"right\">{4}</th><th class=\"center\" colspan=\"2\">{5}</th>",
                WebUtility.HtmlEncode(ReportResources.Name),
                WebUtility.HtmlEncode(ReportResources.Covered),
                WebUtility.HtmlEncode(ReportResources.Uncovered),
                WebUtility.HtmlEncode(ReportResources.Coverable),
                WebUtility.HtmlEncode(ReportResources.Total),
                WebUtility.HtmlEncode(ReportResources.Percentage));

            if (branchCoverageAvailable)
            {
                this.reportTextWriter.Write(
                "<th class=\"right\">{0}</th><th class=\"right\">{1}</th><th class=\"center\" colspan=\"2\">{2}</th>",
                WebUtility.HtmlEncode(ReportResources.Covered),
                WebUtility.HtmlEncode(ReportResources.Total),
                WebUtility.HtmlEncode(ReportResources.Percentage));
            }

            if (methodCoverageAvailable)
            {
                this.reportTextWriter.Write(
                "<th class=\"right\">{0}</th><th class=\"right\">{1}</th><th class=\"center\" colspan=\"2\">{2}</th>",
                WebUtility.HtmlEncode(ReportResources.Covered),
                WebUtility.HtmlEncode(ReportResources.Total),
                WebUtility.HtmlEncode(ReportResources.Percentage));

                this.reportTextWriter.Write(
                "<th class=\"right\">{0}</th><th class=\"right\">{1}</th><th class=\"center\" colspan=\"2\">{2}</th>",
                WebUtility.HtmlEncode(ReportResources.Covered),
                WebUtility.HtmlEncode(ReportResources.Total),
                WebUtility.HtmlEncode(ReportResources.Percentage));
            }

            this.reportTextWriter.WriteLine("</tr></thead>");
            this.reportTextWriter.WriteLine("<tbody>");
        }

        /// <inheritdoc />
        public void CustomSummary(IEnumerable<Assembly> assemblies, IEnumerable<RiskHotspot> riskHotspots, bool branchCoverageAvailable, bool methodCoverageAvailable)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            if (riskHotspots == null)
            {
                throw new ArgumentNullException(nameof(riskHotspots));
            }

            if (this.classReport)
            {
                return;
            }

            this.javaScriptContent.AppendLine("var assemblies = [");

            var historicCoverageExecutionTimes = new HashSet<DateTime>();
            var tagsByBistoricCoverageExecutionTime = new Dictionary<DateTime, string>();
            var metricsByName = new Dictionary<string, Metric>();

            foreach (var assembly in assemblies)
            {
                this.javaScriptContent.AppendLine("  {");
                this.javaScriptContent.AppendFormat("    \"name\": \"{0}\",", assembly.Name.Replace(@"\", @"\\"));
                this.javaScriptContent.AppendLine();
                this.javaScriptContent.AppendLine("    \"classes\": [");

                foreach (var @class in assembly.Classes)
                {
                    var historicCoverages = this.FilterHistoricCoverages(@class.HistoricCoverages, 10);

                    var lineCoverageHistory = "[" + string.Join(",", historicCoverages.Select(h => h.CoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture))) + "]";
                    var branchCoverageHistory = "[]";
                    if (historicCoverages.Any(h => h.BranchCoverageQuota.HasValue))
                    {
                        branchCoverageHistory = "[" + string.Join(",", historicCoverages.Select(h => h.BranchCoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture))) + "]";
                    }

                    var methodCoverageHistory = "[]";
                    if (methodCoverageAvailable && historicCoverages.Any(h => h.CodeElementCoverageQuota.HasValue))
                    {
                        methodCoverageHistory = "[" + string.Join(",", historicCoverages.Select(h => h.CodeElementCoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture))) + "]";
                    }

                    var methodFullCoverageHistory = "[]";
                    if (methodCoverageAvailable && historicCoverages.Any(h => h.FullCodeElementCoverageQuota.HasValue))
                    {
                        methodFullCoverageHistory = "[" + string.Join(",", historicCoverages.Select(h => h.FullCodeElementCoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture))) + "]";
                    }

                    void WriteHistoricCoverage()
                    {
                        int historicCoveragesCounter = 0;
                        this.javaScriptContent.Append("[");
                        foreach (var historicCoverage in @class.HistoricCoverages)
                        {
                            historicCoverageExecutionTimes.Add(historicCoverage.ExecutionTime);
                            tagsByBistoricCoverageExecutionTime[historicCoverage.ExecutionTime] = historicCoverage.Tag;

                            if (historicCoveragesCounter++ > 0)
                            {
                                this.javaScriptContent.Append(", ");
                            }

                            this.javaScriptContent.AppendFormat(
                                "{{ \"et\": \"{0} - {1}{2}{3}\", \"cl\": {4}, \"ucl\": {5}, \"cal\": {6}, \"tl\": {7}, \"lcq\": {8}, \"cb\": {9}, \"tb\": {10}, \"bcq\": {11}, \"cm\": {12}, \"fcm\": {13}, \"tm\": {14}, \"mcq\": {15}, \"mfcq\": {16} }}",
                                historicCoverage.ExecutionTime.ToShortDateString(),
                                historicCoverage.ExecutionTime.ToLongTimeString(),
                                string.IsNullOrEmpty(historicCoverage.Tag) ? string.Empty : " - ",
                                historicCoverage.Tag,
                                historicCoverage.CoveredLines.ToString(CultureInfo.InvariantCulture),
                                (historicCoverage.CoverableLines - historicCoverage.CoveredLines).ToString(CultureInfo.InvariantCulture),
                                historicCoverage.CoverableLines.ToString(CultureInfo.InvariantCulture),
                                historicCoverage.TotalLines.ToString(CultureInfo.InvariantCulture),
                                historicCoverage.CoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture),
                                historicCoverage.CoveredBranches.ToString(CultureInfo.InvariantCulture),
                                historicCoverage.TotalBranches.ToString(CultureInfo.InvariantCulture),
                                historicCoverage.BranchCoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture),
                                methodCoverageAvailable ? historicCoverage.CoveredCodeElements.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : "0",
                                methodCoverageAvailable ? historicCoverage.FullCoveredCodeElements.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : "0",
                                methodCoverageAvailable ? historicCoverage.TotalCodeElements.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : "0",
                                methodCoverageAvailable ? historicCoverage.CodeElementCoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : "0",
                                methodCoverageAvailable ? historicCoverage.FullCodeElementCoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture) : "0");
                        }

                        this.javaScriptContent.Append("]");
                    }

                    void WriteMetricsCoverage()
                    {
                        int metricsCounter = 0;
                        this.javaScriptContent.Append("{");

                        foreach (var metricGroup in @class.Files.SelectMany(f => f.MethodMetrics).SelectMany(m => m.Metrics).GroupBy(m => m.Name))
                        {
                            var firstMetric = metricGroup.First();
                            metricsByName[firstMetric.Name] = firstMetric;

                            if (!methodCoverageAvailable)
                            {
                                continue;
                            }

                            decimal? value = null;

                            if (firstMetric.MetricType == MetricType.CoverageAbsolute)
                            {
                                value = metricGroup.SafeSum(m => m.Value);
                            }
                            else
                            {
                                // Show worst result on summary page
                                if (firstMetric.MergeOrder == MetricMergeOrder.HigherIsBetter)
                                {
                                    value = metricGroup.Min(m => m.Value);
                                }
                                else
                                {
                                    value = metricGroup.Max(m => m.Value);
                                }
                            }

                            if (value.HasValue)
                            {
                                if (metricsCounter++ > 0)
                                {
                                    this.javaScriptContent.Append(", ");
                                }

                                this.javaScriptContent.AppendFormat(
                                    " \"{0}\": {1}",
                                    firstMetric.Abbreviation,
                                    value.Value.ToString(CultureInfo.InvariantCulture));
                            }
                        }

                        this.javaScriptContent.Append(" }");
                    }

                    this.javaScriptContent.Append("      { ");
                    this.javaScriptContent.AppendFormat("\"name\": \"{0}\",", @class.DisplayName.Replace(@"\", @"\\"));
                    this.javaScriptContent.AppendFormat(
                        " \"rp\": \"{0}\",",
                        this.onlySummary ? string.Empty : this.GetClassReportFilename(@class.Assembly, @class.Name));
                    this.javaScriptContent.AppendFormat(" \"cl\": {0},", @class.CoveredLines.ToString(CultureInfo.InvariantCulture));
                    this.javaScriptContent.AppendFormat(" \"ucl\": {0},", (@class.CoverableLines - @class.CoveredLines).ToString(CultureInfo.InvariantCulture));
                    this.javaScriptContent.AppendFormat(" \"cal\": {0},", @class.CoverableLines.ToString(CultureInfo.InvariantCulture));
                    this.javaScriptContent.AppendFormat(" \"tl\": {0},", @class.TotalLines.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));

                    this.javaScriptContent.AppendFormat(" \"cb\": {0},", @class.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
                    this.javaScriptContent.AppendFormat(" \"tb\": {0},", @class.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
                    this.javaScriptContent.AppendFormat(" \"cm\": {0},", methodCoverageAvailable ? @class.CoveredCodeElements.ToString(CultureInfo.InvariantCulture) : "0");
                    this.javaScriptContent.AppendFormat(" \"fcm\": {0},", methodCoverageAvailable ? @class.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture) : "0");
                    this.javaScriptContent.AppendFormat(" \"tm\": {0},", methodCoverageAvailable ? @class.TotalCodeElements.ToString(CultureInfo.InvariantCulture) : "0");
                    this.javaScriptContent.AppendFormat(" \"lch\": {0},", lineCoverageHistory);
                    this.javaScriptContent.AppendFormat(" \"bch\": {0},", branchCoverageHistory);
                    this.javaScriptContent.AppendFormat(" \"mch\": {0},", methodCoverageHistory);
                    this.javaScriptContent.AppendFormat(" \"mfch\": {0},", methodFullCoverageHistory);

                    this.javaScriptContent.Append(" \"hc\": ");
                    WriteHistoricCoverage();
                    this.javaScriptContent.Append(",");

                    this.javaScriptContent.Append(" \"metrics\": ");
                    WriteMetricsCoverage();

                    this.javaScriptContent.AppendLine(" },");
                }

                this.javaScriptContent.AppendLine("    ]},");
            }

            this.javaScriptContent.AppendLine("];");

            this.javaScriptContent.AppendLine();

            this.javaScriptContent.Append("var metrics = [");
            int metricAbbreviationCounter = 0;

            foreach (var item in metricsByName)
            {
                if (metricAbbreviationCounter++ > 0)
                {
                    this.javaScriptContent.Append(", ");
                }

                this.javaScriptContent.AppendFormat(
                    "{{ \"name\": \"{0}\", \"abbreviation\": \"{1}\", \"explanationUrl\": \"{2}\" }}",
                    item.Key,
                    item.Value.Abbreviation,
                    item.Value.ExplanationUrl);
            }

            this.javaScriptContent.AppendLine("];");

            this.javaScriptContent.AppendLine();

            this.javaScriptContent.Append("var historicCoverageExecutionTimes = [");
            int historicCoverageExecutionTimesCounter = 0;

            foreach (var item in historicCoverageExecutionTimes.OrderByDescending(i => i).Skip(1).Take(100).ToList())
            {
                if (historicCoverageExecutionTimesCounter++ > 0)
                {
                    this.javaScriptContent.Append(", ");
                }

                string tag = tagsByBistoricCoverageExecutionTime[item];
                this.javaScriptContent.AppendFormat(
                    "\"{0} - {1}{2}{3}\"",
                    item.ToShortDateString(),
                    item.ToLongTimeString(),
                    string.IsNullOrEmpty(tag) ? string.Empty : " - ",
                    tag);
            }

            this.javaScriptContent.AppendLine("];");

            this.javaScriptContent.AppendLine();

            this.javaScriptContent.AppendLine("var riskHotspotMetrics = [");

            if (riskHotspots.Any())
            {
                foreach (var metric in riskHotspots.First().StatusMetrics)
                {
                    this.javaScriptContent.Append("      { ");
                    this.javaScriptContent.AppendFormat("\"name\": \"{0}\",", metric.Metric.Name);
                    this.javaScriptContent.AppendFormat(" \"explanationUrl\": \"{0}\"", metric.Metric.ExplanationUrl);
                    this.javaScriptContent.AppendLine(" },");
                }
            }

            this.javaScriptContent.AppendLine("];");

            this.javaScriptContent.AppendLine();

            this.javaScriptContent.AppendLine("var riskHotspots = [");

            foreach (var riskHotspot in riskHotspots)
            {
                this.javaScriptContent.AppendLine("  {");
                this.javaScriptContent.AppendFormat("    \"assembly\": \"{0}\",", riskHotspot.Assembly.ShortName);
                this.javaScriptContent.AppendFormat(" \"class\": \"{0}\",", riskHotspot.Class.DisplayName);
                this.javaScriptContent.AppendFormat(" \"reportPath\": \"{0}\",", this.onlySummary ? string.Empty : this.GetClassReportFilename(riskHotspot.Assembly, riskHotspot.Class.Name));
                this.javaScriptContent.AppendFormat(" \"methodName\": \"{0}\",", riskHotspot.MethodMetric.FullName);
                this.javaScriptContent.AppendFormat(" \"methodShortName\": \"{0}\",", riskHotspot.MethodMetric.ShortName);
                this.javaScriptContent.AppendFormat(" \"fileIndex\": {0},", riskHotspot.FileIndex);
                this.javaScriptContent.AppendFormat(" \"line\": {0},", !this.onlySummary && riskHotspot.MethodMetric.Line.HasValue ? riskHotspot.MethodMetric.Line.Value.ToString(CultureInfo.InvariantCulture) : "null");
                this.javaScriptContent.AppendLine();
                this.javaScriptContent.AppendLine("    \"metrics\": [");

                foreach (var metric in riskHotspot.StatusMetrics)
                {
                    this.javaScriptContent.Append("      { ");
                    this.javaScriptContent.AppendFormat("\"value\": {0},", metric.Metric.Value.HasValue ? metric.Metric.Value.Value.ToString("0.##", CultureInfo.InvariantCulture) : "null");
                    this.javaScriptContent.AppendFormat(" \"exceeded\": {0}", metric.Exceeded.ToString().ToLowerInvariant());
                    this.javaScriptContent.AppendLine(" },");
                }

                this.javaScriptContent.AppendLine("    ]},");
            }

            this.javaScriptContent.AppendLine("];");

            this.javaScriptContent.AppendLine();

            this.javaScriptContent.AppendLine("var branchCoverageAvailable = " + branchCoverageAvailable.ToString().ToLowerInvariant() + ";");
            this.javaScriptContent.AppendLine("var methodCoverageAvailable = " + methodCoverageAvailable.ToString().ToLowerInvariant() + ";");
            this.javaScriptContent.AppendLine("var maximumDecimalPlacesForCoverageQuotas = " + MathExtensions.MaximumDecimalPlaces + ";");
            this.javaScriptContent.AppendLine();
        }

        /// <inheritdoc />
        public void BeginLineAnalysisTable(IEnumerable<string> headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            this.reportTextWriter.WriteLine("<div class=\"table-responsive\">");
            this.reportTextWriter.WriteLine("<table class=\"lineAnalysis\">");
            this.reportTextWriter.Write("<thead><tr>");

            foreach (var header in headers)
            {
                this.reportTextWriter.Write("<th>{0}</th>", WebUtility.HtmlEncode(header));
            }

            this.reportTextWriter.WriteLine("</tr></thead>");
            this.reportTextWriter.WriteLine("<tbody>");
        }

        /// <inheritdoc />
        public void MetricsTable(Class @class)
        {
            if (@class == null)
            {
                throw new ArgumentNullException(nameof(@class));
            }

            var metrics = @class.Files
                .SelectMany(f => f.MethodMetrics)
                .SelectMany(m => m.Metrics)
                .Distinct()
                .OrderBy(m => m.Name)
                .ToArray();

            this.reportTextWriter.WriteLine("<div class=\"table-responsive\">");
            this.reportTextWriter.WriteLine("<table class=\"overview table-fixed\">");

            this.reportTextWriter.WriteLine("<colgroup>");
            this.reportTextWriter.WriteLine("<col class=\"column-min-200\" />");

            foreach (var met in metrics)
            {
                this.reportTextWriter.WriteLine("<col class=\"column105\" />");
            }

            this.reportTextWriter.WriteLine("</colgroup>");

            this.reportTextWriter.Write("<thead><tr>");

            this.reportTextWriter.Write("<th>{0}</th>", WebUtility.HtmlEncode(ReportResources.Method));

            foreach (var met in metrics)
            {
                if (met.ExplanationUrl == null)
                {
                    this.reportTextWriter.Write("<th>{0}</th>", WebUtility.HtmlEncode(met.Name));
                }
                else
                {
                    this.reportTextWriter.Write("<th>{0} <a href=\"{1}\" target=\"_blank\"><i class=\"icon-info-circled\"></i></a></th>", WebUtility.HtmlEncode(met.Name), WebUtility.HtmlEncode(met.ExplanationUrl.OriginalString));
                }
            }

            this.reportTextWriter.WriteLine("</tr></thead>");
            this.reportTextWriter.WriteLine("<tbody>");

            int fileIndex = 0;
            bool usePrefix = @class.Files.Count() > 1;

            foreach (var file in @class.Files)
            {
                foreach (var methodMetric in file.MethodMetrics.OrderBy(c => c.Line))
                {
                    this.reportTextWriter.Write("<tr>");

                    if (methodMetric.Line.HasValue)
                    {
                        this.reportTextWriter.Write(
                            "<td title=\"{0}\"><a href=\"#file{1}_line{2}\" class=\"navigatetohash\">{3}</a></td>",
                            WebUtility.HtmlEncode(methodMetric.FullName),
                            fileIndex,
                            methodMetric.Line,
                            WebUtility.HtmlEncode(
                                (usePrefix ? $"{ReportResources.File} {fileIndex + 1}: " : string.Empty) + methodMetric.ShortName));
                    }
                    else
                    {
                        this.reportTextWriter.Write("<td title=\"{0}\">{1}</td>", WebUtility.HtmlEncode(methodMetric.FullName), WebUtility.HtmlEncode(file.Path + " => " + methodMetric.ShortName));
                    }

                    foreach (var metric in metrics)
                    {
                        var metricValue = methodMetric.Metrics.FirstOrDefault(m => m.Equals(metric));

                        if (metricValue != null)
                        {
                            this.reportTextWriter.Write(
                            "<td>{0}{1}</td>",
                            metricValue.Value.HasValue ? metricValue.Value.Value.ToString("0.##", CultureInfo.InvariantCulture) : "-",
                            metricValue.Value.HasValue && metricValue.MetricType == MetricType.CoveragePercentual ? "%" : string.Empty);
                        }
                        else
                        {
                            this.reportTextWriter.Write("<td>-</td>");
                        }
                    }

                    this.reportTextWriter.WriteLine("</tr>");
                }

                fileIndex++;
            }

            this.reportTextWriter.WriteLine("</tbody>");
            this.reportTextWriter.WriteLine("</table>");
            this.reportTextWriter.WriteLine("</div>");
        }

        /// <inheritdoc />
        public void MetricsTable(IEnumerable<MethodMetric> methodMetrics)
        {
            if (methodMetrics == null)
            {
                throw new ArgumentNullException(nameof(methodMetrics));
            }

            var metrics = methodMetrics
                .SelectMany(m => m.Metrics)
                .Distinct()
                .OrderBy(m => m.Name);

            this.reportTextWriter.WriteLine("<div class=\"table-responsive\">");
            this.reportTextWriter.WriteLine("<table class=\"overview table-fixed\">");

            this.reportTextWriter.WriteLine("<colgroup>");
            this.reportTextWriter.WriteLine("<col class=\"column-min-200\" />");

            foreach (var met in metrics)
            {
                this.reportTextWriter.WriteLine("<col class=\"column105\" />");
            }

            this.reportTextWriter.WriteLine("</colgroup>");

            this.reportTextWriter.Write("<thead><tr>");

            this.reportTextWriter.Write("<th>{0}</th>", WebUtility.HtmlEncode(ReportResources.Method));

            foreach (var met in metrics)
            {
                if (met.ExplanationUrl == null)
                {
                    this.reportTextWriter.Write("<th>{0}</th>", WebUtility.HtmlEncode(met.Name));
                }
                else
                {
                    this.reportTextWriter.Write("<th>{0} <a href=\"{1}\" target=\"_blank\"><i class=\"icon-info-circled\"></i></a></th>", WebUtility.HtmlEncode(met.Name), WebUtility.HtmlEncode(met.ExplanationUrl.OriginalString));
                }
            }

            this.reportTextWriter.WriteLine("</tr></thead>");
            this.reportTextWriter.WriteLine("<tbody>");

            foreach (var methodMetric in methodMetrics)
            {
                this.reportTextWriter.Write("<tr>");

                this.reportTextWriter.Write("<td title=\"{0}\">{1}</td>", WebUtility.HtmlEncode(methodMetric.FullName), WebUtility.HtmlEncode(methodMetric.ShortName));

                foreach (var metric in metrics)
                {
                    var metricValue = methodMetric.Metrics.FirstOrDefault(m => m.Equals(metric));

                    if (metricValue != null)
                    {
                        this.reportTextWriter.Write(
                        "<td>{0}</td>",
                        metricValue.Value.HasValue ? metricValue.Value.Value.ToString("0.##", CultureInfo.InvariantCulture) : "-");
                    }
                    else
                    {
                        this.reportTextWriter.Write("<td>-</td>");
                    }
                }

                this.reportTextWriter.WriteLine("</tr>");
            }

            this.reportTextWriter.WriteLine("</tbody>");
            this.reportTextWriter.WriteLine("</table>");
            this.reportTextWriter.WriteLine("</div>");
        }

        /// <inheritdoc />
        public void LineAnalysis(int fileIndex, LineAnalysis analysis)
        {
            if (analysis == null)
            {
                throw new ArgumentNullException(nameof(analysis));
            }

            string formattedLine = analysis.LineContent
                .Replace(((char)11).ToString(), "  ") // replace tab
                .Replace(((char)9).ToString(), "  "); // replace tab

            if (formattedLine.Length > 120)
            {
                formattedLine = formattedLine.Substring(0, 120);
            }

            formattedLine = WebUtility.HtmlEncode(formattedLine);
            formattedLine = formattedLine.Replace(" ", "&nbsp;");

            string lineVisitStatus = ConvertToCssClass(analysis.LineVisitStatus, false);

            this.reportTextWriter.Write(
                "<tr class=\"{0}\" title=\"{1}\" data-coverage=\"{{",
                analysis.LineVisitStatus > LineVisitStatus.NotCoverable ? "coverableline" : string.Empty,
                WebUtility.HtmlEncode(GetTooltip(analysis)));

            this.reportTextWriter.Write(
                "'AllTestMethods': {{'VC': '{0}', 'LVS': '{1}'}}",
                analysis.LineVisitStatus != LineVisitStatus.NotCoverable ? analysis.LineVisits.ToString(CultureInfo.InvariantCulture) : string.Empty,
                lineVisitStatus);

            foreach (var coverageByTestMethod in analysis.LineCoverageByTestMethod)
            {
                this.reportTextWriter.Write(
                    ", 'M{0}': {{'VC': '{1}', 'LVS': '{2}'}}",
                    coverageByTestMethod.Key.Id.ToString(CultureInfo.InvariantCulture),
                    coverageByTestMethod.Value.LineVisitStatus != LineVisitStatus.NotCoverable ? coverageByTestMethod.Value.LineVisits.ToString(CultureInfo.InvariantCulture) : string.Empty,
                    ConvertToCssClass(coverageByTestMethod.Value.LineVisitStatus, false));
            }

            this.reportTextWriter.Write("}\">");

            this.reportTextWriter.Write(
                "<td class=\"{0}\">&nbsp;</td>",
                lineVisitStatus);
            this.reportTextWriter.Write(
                "<td class=\"leftmargin rightmargin right\">{0}</td>",
                analysis.LineVisitStatus != LineVisitStatus.NotCoverable ? analysis.LineVisits.ToString(CultureInfo.InvariantCulture) : string.Empty);
            this.reportTextWriter.Write(
                "<td class=\"rightmargin right\"><a id=\"file{0}_line{1}\"></a><code>{1}</code></td>",
                fileIndex,
                analysis.LineNumber);

            if (analysis.CoveredBranches.HasValue && analysis.TotalBranches.HasValue && analysis.TotalBranches.Value > 0)
            {
                int branchCoverage = (int)(100 * (double)analysis.CoveredBranches.Value / analysis.TotalBranches.Value);
                branchCoverage -= branchCoverage % 10;
                this.reportTextWriter.Write("<td class=\"percentagebar percentagebar{0}\"><i class=\"icon-fork\"></i></td>", branchCoverage);
            }
            else
            {
                this.reportTextWriter.Write("<td></td>");
            }

            this.reportTextWriter.Write(
                "<td class=\"{0}\"><code>{1}</code></td>",
                ConvertToCssClass(analysis.LineVisitStatus, true),
                formattedLine);

            this.reportTextWriter.WriteLine("</tr>");
        }

        /// <inheritdoc />
        public void FinishTable()
        {
            this.reportTextWriter.WriteLine("</tbody>");
            this.reportTextWriter.WriteLine("</table>");
            this.reportTextWriter.WriteLine("</div>");
        }

        /// <inheritdoc />
        public void Chart(IEnumerable<HistoricCoverage> historicCoverages, bool methodCoverageAvailable)
        {
            if (historicCoverages == null)
            {
                throw new ArgumentNullException(nameof(historicCoverages));
            }

            var filteredHistoricCoverages = this.FilterHistoricCoverages(historicCoverages, 100);

            string id = Guid.NewGuid().ToString("N");

            string svgHistory = SvgHistoryChartRenderer.RenderHistoryChart(filteredHistoricCoverages, methodCoverageAvailable);

            this.reportTextWriter.WriteLine(
                "<div class=\"historychart ct-chart\" data-data=\"historyChartData{0}\">{1}</div>",
                id,
                svgHistory);

            void WriteSeries(TextWriter series)
            {
                series.Write("[");
                if (filteredHistoricCoverages.Any(h => h.CoverageQuota.HasValue))
                {
                    for (int i = 0; i < filteredHistoricCoverages.Count; i++)
                    {
                        if (i > 0)
                        {
                            series.Write(", ");
                        }

                        if (filteredHistoricCoverages[i].CoverageQuota.HasValue)
                        {
                            series.Write("{ 'meta': ");
                            series.Write(i);
                            series.Write(", 'value': ");
                            series.Write(filteredHistoricCoverages[i].CoverageQuota.Value.ToString(CultureInfo.InvariantCulture));
                            series.Write(" }");
                        }
                        else
                        {
                            series.Write("null");
                        }
                    }
                }

                series.WriteLine("],");
                series.Write("[");

                if (filteredHistoricCoverages.Any(h => h.BranchCoverageQuota.HasValue))
                {
                    for (int i = 0; i < filteredHistoricCoverages.Count; i++)
                    {
                        if (i > 0)
                        {
                            series.Write(", ");
                        }

                        if (filteredHistoricCoverages[i].BranchCoverageQuota.HasValue)
                        {
                            series.Write("{ 'meta': ");
                            series.Write(i);
                            series.Write(", 'value': ");
                            series.Write(filteredHistoricCoverages[i].BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture));
                            series.Write(" }");
                        }
                        else
                        {
                            series.Write("null");
                        }
                    }
                }

                series.WriteLine("],");
                series.Write("[");

                if (methodCoverageAvailable && filteredHistoricCoverages.Any(h => h.CodeElementCoverageQuota.HasValue))
                {
                    for (int i = 0; i < filteredHistoricCoverages.Count; i++)
                    {
                        if (i > 0)
                        {
                            series.Write(", ");
                        }

                        if (filteredHistoricCoverages[i].CodeElementCoverageQuota.HasValue)
                        {
                            series.Write("{ 'meta': ");
                            series.Write(i);
                            series.Write(", 'value': ");
                            series.Write(filteredHistoricCoverages[i].CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture));
                            series.Write(" }");
                        }
                        else
                        {
                            series.Write("null");
                        }
                    }
                }

                series.WriteLine("],");
                series.Write("[");

                if (methodCoverageAvailable && filteredHistoricCoverages.Any(h => h.FullCodeElementCoverageQuota.HasValue))
                {
                    for (int i = 0; i < filteredHistoricCoverages.Count; i++)
                    {
                        if (i > 0)
                        {
                            series.Write(", ");
                        }

                        if (filteredHistoricCoverages[i].FullCodeElementCoverageQuota.HasValue)
                        {
                            series.Write("{ 'meta': ");
                            series.Write(i);
                            series.Write(", 'value': ");
                            series.Write(filteredHistoricCoverages[i].FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture));
                            series.Write(" }");
                        }
                        else
                        {
                            series.Write("null");
                        }
                    }
                }

                series.WriteLine("]");
            }

            var toolTips = filteredHistoricCoverages.Select(h =>
                string.Format(
                    "'<h3>{0} - {1}</h3>{2}{3}{4}{5}{6}{7}'",
                    h.ExecutionTime.ToShortDateString(),
                    h.ExecutionTime.ToLongTimeString(),
                    h.CoverageQuota.HasValue ? string.Format(CultureInfo.InvariantCulture, "<br /><span class=\"linecoverage\"></span> {0} {1}% ({2}/{3})", WebUtility.HtmlEncode(ReportResources.Coverage2), h.CoverageQuota.Value, h.CoveredLines, h.CoverableLines) : null,
                    h.BranchCoverageQuota.HasValue ? string.Format(CultureInfo.InvariantCulture, "<br /><span class=\"branchcoverage\"></span> {0} {1}% ({2}/{3})", WebUtility.HtmlEncode(ReportResources.BranchCoverage2), h.BranchCoverageQuota.Value, h.CoveredBranches, h.TotalBranches) : null,
                    methodCoverageAvailable && h.CodeElementCoverageQuota.HasValue ? string.Format(CultureInfo.InvariantCulture, "<br /><span class=\"codeelementcoverage\"></span> {0} {1}% ({2}/{3})", WebUtility.HtmlEncode(ReportResources.CodeElementCoverageQuota2), h.CodeElementCoverageQuota.Value, h.CoveredCodeElements, h.TotalCodeElements) : null,
                    methodCoverageAvailable && h.FullCodeElementCoverageQuota.HasValue ? string.Format(CultureInfo.InvariantCulture, "<br /><span class=\"fullcodeelementcoverage\"></span> {0} {1}% ({2}/{3})", WebUtility.HtmlEncode(ReportResources.FullCodeElementCoverageQuota), h.FullCodeElementCoverageQuota.Value, h.FullCoveredCodeElements, h.TotalCodeElements) : null,
                    string.Format(CultureInfo.InvariantCulture, "<br />{0} {1}", WebUtility.HtmlEncode(ReportResources.TotalLines), h.TotalLines),
                    h.Tag != null ? string.Format(CultureInfo.InvariantCulture, "<br />{0} {1}", WebUtility.HtmlEncode(ReportResources.Tag), h.Tag) : string.Empty));

            this.reportTextWriter.WriteLine("<script type=\"text/javascript\">/* <![CDATA[ */ ");

            this.reportTextWriter.WriteLine("var historyChartData{0} = {{", id);
            this.reportTextWriter.Write("    \"series\" : [");
            WriteSeries(this.reportTextWriter);
            this.reportTextWriter.WriteLine("],");

            this.reportTextWriter.WriteLine(
                 "    \"tooltips\" : [{0}]",
                 string.Join(",", toolTips));
            this.reportTextWriter.WriteLine("};");
            this.reportTextWriter.WriteLine(" /* ]]> */ </script>");
        }

        /// <inheritdoc />
        public void BeginRiskHotspots()
        {
            this.reportTextWriter.WriteLine("<risk-hotspots>");
        }

        /// <inheritdoc />
        public void FinishRiskHotspots()
        {
            this.reportTextWriter.WriteLine("</risk-hotspots>");
        }

        /// <inheritdoc />
        public void RiskHotspots(IEnumerable<RiskHotspot> riskHotspots)
        {
            var codeQualityMetrics = riskHotspots.First().MethodMetric.Metrics
                .Where(m => m.MetricType == MetricType.CodeQuality)
                .ToArray();

            this.reportTextWriter.WriteLine("<div class=\"table-responsive\">");
            this.reportTextWriter.WriteLine("<table class=\"overview table-fixed stripped\">");

            this.reportTextWriter.WriteLine("<colgroup>");
            this.reportTextWriter.WriteLine("<col class=\"column-min-200\" />");
            this.reportTextWriter.WriteLine("<col class=\"column-min-200\" />");
            this.reportTextWriter.WriteLine("<col class=\"column-min-200\" />");

            foreach (var met in codeQualityMetrics)
            {
                this.reportTextWriter.WriteLine("<col class=\"column105\" />");
            }

            this.reportTextWriter.WriteLine("</colgroup>");

            this.reportTextWriter.Write("<thead><tr>");

            this.reportTextWriter.WriteLine("<th>{0}</th>", WebUtility.HtmlEncode(ReportResources.Assembly2));
            this.reportTextWriter.WriteLine("<th>{0}</th>", WebUtility.HtmlEncode(ReportResources.Class2));
            this.reportTextWriter.WriteLine("<th>{0}</th>", WebUtility.HtmlEncode(ReportResources.Method));

            foreach (var metric in codeQualityMetrics)
            {
                if (metric.ExplanationUrl == null)
                {
                    this.reportTextWriter.WriteLine("<th>{0}</th>", WebUtility.HtmlEncode(metric.Name));
                }
                else
                {
                    this.reportTextWriter.WriteLine("<th>{0} <a href=\"{1}\" target=\"_blank\"><i class=\"icon-info-circled\"></i></a></th>", WebUtility.HtmlEncode(metric.Name), WebUtility.HtmlEncode(metric.ExplanationUrl.OriginalString));
                }
            }

            this.reportTextWriter.WriteLine("</tr></thead>");

            this.reportTextWriter.WriteLine("<tbody>");

            foreach (var riskHotspot in riskHotspots.Take(20))
            {
                string filenameColumn = riskHotspot.Class.Name;

                if (!this.onlySummary)
                {
                    filenameColumn = string.Format(
                        CultureInfo.InvariantCulture,
                        "<a href=\"{0}\">{1}</a>",
                        WebUtility.HtmlEncode(this.GetClassReportFilename(riskHotspot.Assembly, riskHotspot.Class.Name)),
                        WebUtility.HtmlEncode(riskHotspot.Class.DisplayName));
                }

                this.reportTextWriter.WriteLine("<tr>");
                this.reportTextWriter.WriteLine("<td>{0}</td>", WebUtility.HtmlEncode(riskHotspot.Assembly.ShortName));
                this.reportTextWriter.WriteLine("<td>{0}</td>", filenameColumn);

                if (!this.onlySummary && riskHotspot.MethodMetric.Line.HasValue)
                {
                    this.reportTextWriter.Write(
                        "<td title=\"{0}\"><a href=\"{1}#file{2}_line{3}\">{4}</a></td>",
                        WebUtility.HtmlEncode(riskHotspot.MethodMetric.FullName),
                        WebUtility.HtmlEncode(this.GetClassReportFilename(riskHotspot.Assembly, riskHotspot.Class.Name)),
                        riskHotspot.FileIndex,
                        riskHotspot.MethodMetric.Line,
                        WebUtility.HtmlEncode(riskHotspot.MethodMetric.ShortName));
                }
                else
                {
                    this.reportTextWriter.Write("<td title=\"{0}\">{1}</td>", WebUtility.HtmlEncode(riskHotspot.MethodMetric.FullName), WebUtility.HtmlEncode(riskHotspot.MethodMetric.ShortName));
                }

                foreach (var statusMetric in riskHotspot.StatusMetrics)
                {
                    this.reportTextWriter.WriteLine(
                        "<td class=\"{0} right\">{1}</td>",
                        statusMetric.Exceeded ? "lightred" : "lightgreen",
                        statusMetric.Metric.Value.HasValue ? statusMetric.Metric.Value.Value.ToString("0.##", CultureInfo.InvariantCulture) : "-");
                }

                this.reportTextWriter.WriteLine("</tr>");
            }

            this.reportTextWriter.WriteLine("</tbody>");
            this.reportTextWriter.WriteLine("</table>");
            this.reportTextWriter.WriteLine("</div>");
        }

        /// <inheritdoc />
        public void SummaryAssembly(Assembly assembly, bool branchCoverageAvailable, bool methodCoverageAvailable)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            this.reportTextWriter.Write("<tr>");
            this.reportTextWriter.Write("<th>{0}</th>", WebUtility.HtmlEncode(assembly.Name));
            this.reportTextWriter.Write("<th class=\"right\">{0}</th>", assembly.CoveredLines);
            this.reportTextWriter.Write("<th class=\"right\">{0}</th>", assembly.CoverableLines - assembly.CoveredLines);
            this.reportTextWriter.Write("<th class=\"right\">{0}</th>", assembly.CoverableLines);
            this.reportTextWriter.Write("<th class=\"right\">{0}</th>", assembly.TotalLines.GetValueOrDefault());
            this.reportTextWriter.Write(
                "<th title=\"{0}\" class=\"right\">{1}</th>",
                assembly.CoverageQuota.HasValue ? $"{assembly.CoveredLines}/{assembly.CoverableLines}" : string.Empty,
                assembly.CoverageQuota.HasValue ? assembly.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
            WriteCoverageTable(this.reportTextWriter, "th", assembly.CoverageQuota);

            if (branchCoverageAvailable)
            {
                this.reportTextWriter.Write("<th class=\"right\">{0}</th>", assembly.CoveredBranches);
                this.reportTextWriter.Write("<th class=\"right\">{0}</th>", assembly.TotalBranches);
                this.reportTextWriter.Write(
                "<th class=\"right\" title=\"{0}\">{1}</th>",
                assembly.BranchCoverageQuota.HasValue ? $"{assembly.CoveredBranches}/{assembly.TotalBranches}" : "-",
                assembly.BranchCoverageQuota.HasValue ? assembly.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                WriteCoverageTable(this.reportTextWriter, "th", assembly.BranchCoverageQuota);
            }

            if (methodCoverageAvailable)
            {
                this.reportTextWriter.Write("<th class=\"right\">{0}</th>", assembly.CoveredCodeElements);
                this.reportTextWriter.Write("<th class=\"right\">{0}</th>", assembly.TotalCodeElements);
                this.reportTextWriter.Write(
                "<th class=\"right\" title=\"{0}\">{1}</th>",
                assembly.CodeElementCoverageQuota.HasValue ? $"{assembly.CoveredCodeElements}/{assembly.TotalCodeElements}" : "-",
                assembly.CodeElementCoverageQuota.HasValue ? assembly.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                WriteCoverageTable(this.reportTextWriter, "th", assembly.CodeElementCoverageQuota);

                this.reportTextWriter.Write("<th class=\"right\">{0}</th>", assembly.FullCoveredCodeElements);
                this.reportTextWriter.Write("<th class=\"right\">{0}</th>", assembly.TotalCodeElements);
                this.reportTextWriter.Write(
                "<th class=\"right\" title=\"{0}\">{1}</th>",
                assembly.FullCodeElementCoverageQuota.HasValue ? $"{assembly.FullCoveredCodeElements}/{assembly.TotalCodeElements}" : "-",
                assembly.FullCodeElementCoverageQuota.HasValue ? assembly.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                WriteCoverageTable(this.reportTextWriter, "th", assembly.FullCodeElementCoverageQuota);
            }

            this.reportTextWriter.WriteLine("</tr>");
        }

        /// <inheritdoc />
        public void SummaryClass(Class @class, bool branchCoverageAvailable, bool methodCoverageAvailable)
        {
            if (@class == null)
            {
                throw new ArgumentNullException(nameof(@class));
            }

            string filenameColumn = @class.Name;

            if (!this.onlySummary)
            {
                filenameColumn = string.Format(
                    CultureInfo.InvariantCulture,
                    "<a href=\"{0}\">{1}</a>",
                    WebUtility.HtmlEncode(this.GetClassReportFilename(@class.Assembly, @class.Name)),
                    WebUtility.HtmlEncode(@class.DisplayName));
            }

            this.reportTextWriter.Write("<tr>");
            this.reportTextWriter.Write("<td>{0}</td>", filenameColumn);
            this.reportTextWriter.Write("<td class=\"right\">{0}</td>", @class.CoveredLines);
            this.reportTextWriter.Write("<td class=\"right\">{0}</td>", @class.CoverableLines - @class.CoveredLines);
            this.reportTextWriter.Write("<td class=\"right\">{0}</td>", @class.CoverableLines);
            this.reportTextWriter.Write("<td class=\"right\">{0}</td>", @class.TotalLines.GetValueOrDefault());
            this.reportTextWriter.Write(
                "<td title=\"{0}\" class=\"right\">{1}</td>",
                @class.CoverageQuota.HasValue ? $"{@class.CoveredLines}/{@class.CoverableLines}" : string.Empty,
                @class.CoverageQuota.HasValue ? @class.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);

            WriteCoverageTable(this.reportTextWriter, "td", @class.CoverageQuota);

            if (branchCoverageAvailable)
            {
                this.reportTextWriter.Write("<td class=\"right\">{0}</td>", @class.CoveredBranches);
                this.reportTextWriter.Write("<td class=\"right\">{0}</td>", @class.TotalBranches);
                this.reportTextWriter.Write(
                    "<td class=\"right\" title=\"{0}\">{1}</td>",
                    @class.BranchCoverageQuota.HasValue ? $"{@class.CoveredBranches}/{@class.TotalBranches}" : "-",
                    @class.BranchCoverageQuota.HasValue ? @class.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                WriteCoverageTable(this.reportTextWriter, "td", @class.BranchCoverageQuota);
            }

            if (methodCoverageAvailable)
            {
                this.reportTextWriter.Write("<td class=\"right\">{0}</td>", @class.CoveredCodeElements);
                this.reportTextWriter.Write("<td class=\"right\">{0}</td>", @class.TotalCodeElements);
                this.reportTextWriter.Write(
                    "<td class=\"right\" title=\"{0}\">{1}</td>",
                    @class.CodeElementCoverageQuota.HasValue ? $"{@class.CoveredCodeElements}/{@class.TotalCodeElements}" : "-",
                    @class.CodeElementCoverageQuota.HasValue ? @class.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                WriteCoverageTable(this.reportTextWriter, "td", @class.CodeElementCoverageQuota);

                this.reportTextWriter.Write("<td class=\"right\">{0}</td>", @class.FullCoveredCodeElements);
                this.reportTextWriter.Write("<td class=\"right\">{0}</td>", @class.TotalCodeElements);
                this.reportTextWriter.Write(
                    "<td class=\"right\" title=\"{0}\">{1}</td>",
                    @class.FullCodeElementCoverageQuota.HasValue ? $"{@class.FullCoveredCodeElements}/{@class.TotalCodeElements}" : "-",
                    @class.FullCodeElementCoverageQuota.HasValue ? @class.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                WriteCoverageTable(this.reportTextWriter, "td", @class.FullCodeElementCoverageQuota);
            }

            this.reportTextWriter.WriteLine("</tr>");
        }

        /// <inheritdoc />
        public void AddFooter()
        {
            this.reportTextWriter.Write(string.Format(
                CultureInfo.InvariantCulture,
                "<div class=\"footer\">{0} ReportGenerator {1}<br />{2} - {3}<br /><a href=\"https://github.com/danielpalme/ReportGenerator\">GitHub</a> | <a href=\"https://reportgenerator.io\">reportgenerator.io</a></div>",
                WebUtility.HtmlEncode(ReportResources.GeneratedBy),
                typeof(IReportBuilder).Assembly.GetName().Version,
                DateTime.Now.ToShortDateString(),
                DateTime.Now.ToLongTimeString()));
        }

        /// <inheritdoc />
        public void SaveSummaryReport(string targetDirectory)
        {
            this.SaveReport();

            if (this.htmlMode != HtmlMode.InlineCssAndJavaScript)
            {
                this.SaveCss(targetDirectory);
                this.SaveJavaScript(targetDirectory);
            }
        }

        /// <inheritdoc />
        public void SaveClassReport(string targetDirectory, string className)
        {
            this.SaveReport();

            if (this.htmlMode != HtmlMode.InlineCssAndJavaScript && !this.javaScriptGenerated)
            {
                this.SaveJavaScript(targetDirectory);
                this.javaScriptGenerated = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.reportTextWriter?.Dispose();
                StringBuilderCache.Return(this.javaScriptContent);
            }
        }

        /// <summary>
        /// Writes a table showing the coverage quota with red and green bars to the TextWriter.
        /// </summary>
        /// <param name="writer"><see cref="TextWriter"/> to write to.</param>
        /// <param name="surroundingElement">Html element to write around the table (e.g. 'td', or 'th').</param>
        /// <param name="coverage">The coverage quota.</param>
        private static void WriteCoverageTable(TextWriter writer, string surroundingElement, decimal? coverage)
        {
            writer.Write("<{0}><table class=\"coverage\"><tr>", surroundingElement);

            if (coverage.HasValue)
            {
                int covered = (int)Math.Round(coverage.Value, 0);
                int uncovered = 100 - covered;

                if (covered > 0)
                {
                    writer.Write("<td class=\"green covered{0}\">&nbsp;</td>", covered);
                }

                if (uncovered > 0)
                {
                    writer.Write("<td class=\"red covered{0}\">&nbsp;</td>", uncovered);
                }
            }
            else
            {
                writer.Write("<td class=\"gray covered100\">&nbsp;</td>");
            }

            writer.Write("</tr></table></{0}>", surroundingElement);
        }

        /// <summary>
        /// Converts the <see cref="LineVisitStatus" /> to the corresponding CSS class.
        /// </summary>
        /// <param name="lineVisitStatus">The line visit status.</param>
        /// <param name="lightcolor">if set to <c>true</c> a CSS class representing a light color is returned.</param>
        /// <returns>The corresponding CSS class.</returns>
        private static string ConvertToCssClass(LineVisitStatus lineVisitStatus, bool lightcolor)
        {
            switch (lineVisitStatus)
            {
                case LineVisitStatus.Covered:
                    return lightcolor ? "lightgreen" : "green";
                case LineVisitStatus.NotCovered:
                    return lightcolor ? "lightred" : "red";
                case LineVisitStatus.PartiallyCovered:
                    return lightcolor ? "lightorange" : "orange";
                default:
                    return lightcolor ? "lightgray" : "gray";
            }
        }

        private static string GetTooltip(LineAnalysis analysis)
        {
            string branchRate = string.Empty;

            if (analysis.CoveredBranches.HasValue && analysis.TotalBranches.HasValue && analysis.TotalBranches.Value > 0)
            {
                branchRate = ", " + string.Format(ReportResources.CoveredBranches, analysis.CoveredBranches, analysis.TotalBranches);
            }

            if (analysis.LineVisitStatus == LineVisitStatus.Covered)
            {
                return string.Format(ReportResources.CoverageTooltip_Covered, analysis.LineVisits, branchRate);
            }
            else if (analysis.LineVisitStatus == LineVisitStatus.PartiallyCovered)
            {
                return string.Format(ReportResources.CoverageTooltip_PartiallyCovered, analysis.LineVisits, branchRate);
            }
            else if (analysis.LineVisitStatus == LineVisitStatus.NotCovered)
            {
                return string.Format(ReportResources.CoverageTooltip_NotCovered, analysis.LineVisits, branchRate);
            }
            else
            {
                return ReportResources.CoverageTooltip_NotCoverable;
            }
        }

        private void WriteHtmlStart(TextWriter writer, string title, string subtitle)
        {
            writer.WriteLine($@"<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
<meta http-equiv=""X-UA-Compatible"" content=""IE=EDGE,chrome=1"" />
<link href=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAAn1BMVEUAAADCAAAAAAA3yDfUAAA3yDfUAAA8PDzr6+sAAAD4+Pg3yDeQkJDTAADt7e3V1dU3yDdCQkIAAADbMTHUAABBykHUAAA2yDY3yDfr6+vTAAB3diDR0dGYcHDUAAAjhiPSAAA3yDeuAADUAAA3yDf////OCALg9+BLzktBuzRelimzKgv87+/dNTVflSn1/PWz6rO126g5yDlYniy0KgwjJ0TyAAAAI3RSTlMABAj0WD6rJcsN7X1HzMqUJyYW+/X08+bltqSeaVRBOy0cE+citBEAAADBSURBVDjLlczXEoIwFIThJPYGiL0XiL3r+z+bBOJs9JDMuLffP8v+Gxfc6aIyDQVjQcnqnvRDEQwLJYtXpZT+YhDHKIjLbS+OUeT4TjkKi6OwOArq+yeKXD9uDqQQbcOjyCy0e6bTojZSftX+U6zUQ7OuittDu1k0WHqRFfdXQijgjKfF6ZwAikvmKD6OQjmKWUcDigkztm5FZN05nMON9ZcoinlBmTNnAUdBnRbUUbgdBZwWbkcBpwXcVsBtxfjb31j1QB5qeebOAAAAAElFTkSuQmCC"" rel=""icon"" type=""image/x-icon"" />
<title>{WebUtility.HtmlEncode(title)} - {WebUtility.HtmlEncode(subtitle)}</title>");

            if (this.htmlMode == HtmlMode.InlineCssAndJavaScript)
            {
                writer.Write("<style type=\"text/css\">");
                this.WriteCss(writer);

                writer.WriteLine("</style>");
            }
            else
            {
                writer.WriteLine(CssLink);
            }

            writer.WriteLine("</head><body><div class=\"container\"><div class=\"containerleft\">");
        }

        /// <summary>
        /// Gets the file name of the report file for the given class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns>The file name.</returns>
        private string GetClassReportFilename(Assembly assembly, string className)
        {
            string assemblyName = assembly.ShortName;

            string key = assembly.Name + "_" + className;

            if (!this.fileNameByClass.TryGetValue(key, out string fileName))
            {
                string shortClassName = null;

                if (assemblyName == "Default" && className.Contains(Path.DirectorySeparatorChar))
                {
                    assemblyName = className.Substring(0, className.LastIndexOf(Path.DirectorySeparatorChar));
                    shortClassName = className.Substring(className.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                }
                else
                {
                    if (className.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                    {
                        shortClassName = className.Substring(0, className.LastIndexOf('.'));
                    }
                    else
                    {
                        shortClassName = className.Substring(className.LastIndexOf('.') + 1);
                    }
                }

                fileName = StringHelper.ReplaceInvalidPathChars(assemblyName + "_" + shortClassName) + ".html";

                if (fileName.Length > 100)
                {
                    string firstPart = fileName.Substring(0, 50);
                    string lastPart = fileName.Substring(fileName.Length - 45, 45);

                    fileName = firstPart + lastPart;
                }

                if (this.fileNameByClass.Values.Any(v => v.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    int counter = 2;
                    string fileNameWithoutExtension = fileName.Substring(0, fileName.Length - 4);

                    do
                    {
                        fileName = fileNameWithoutExtension + counter + ".html";
                        counter++;
                    }
                    while (this.fileNameByClass.Values.Any(v => v.Equals(fileName, StringComparison.OrdinalIgnoreCase)));
                }

                this.fileNameByClass.Add(key, fileName);
            }

            return fileName;
        }

        /// <summary>
        /// Saves the CSS.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        private void SaveCss(string targetDirectory)
        {
            string targetPath = Path.Combine(targetDirectory, "report.css");

            if (WrittenCssAndJavascriptFiles.Contains(targetPath))
            {
                return;
            }

            if (this.htmlMode == HtmlMode.InlineCssAndJavaScript)
            {
                using (var fs = new FileStream(targetPath, FileMode.Create))
                using (var writer = new StreamWriter(fs))
                {
                    this.WriteCss(writer);
                }
            }
            else
            {
                var builder = StringBuilderCache.Get();
                using (var writer = new StringWriter(builder))
                {
                    this.WriteCss(writer);
                }

                string css = StringBuilderCache.ToStringAndReturnToPool(builder);

                System.IO.File.WriteAllText(targetPath, css);

                var matches = Regex.Matches(css, @"url\(icon_(?<filename>.+).svg\),\surl\(data:image/svg\+xml;base64,(?<base64image>.+)\)");

                foreach (Match match in matches)
                {
                    System.IO.File.WriteAllBytes(
                        Path.Combine(targetDirectory, "icon_" + match.Groups["filename"].Value + ".svg"),
                        Convert.FromBase64String(match.Groups["base64image"].Value));
                }
            }

            WrittenCssAndJavascriptFiles.Add(targetPath);
        }

        /// <summary>
        /// Saves the java script.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        private void SaveJavaScript(string targetDirectory)
        {
            string targetPath = Path.Combine(targetDirectory, this.classReport ? "class.js" : "main.js");

            if (WrittenCssAndJavascriptFiles.Contains(targetPath))
            {
                return;
            }

            using (var fs = new FileStream(targetPath, FileMode.Create))
            using (var writer = new StreamWriter(fs))
            {
                this.WriteCombinedJavascript(writer);
            }

            WrittenCssAndJavascriptFiles.Add(targetPath);
        }

        /// <summary>
        /// Writes CSS to the provided Text Writer.
        /// </summary>
        private void WriteCss(TextWriter writer)
        {
            void WriteWithFilteredUrls(string resourceName)
            {
                var resource = ResourceStreamCache.Get(resourceName);
                if (this.htmlMode != HtmlMode.InlineCssAndJavaScript)
                {
                    writer.Write(resource);
                }
                else
                {
                    writer.WriteLine(Regex.Replace(resource, @"url\(icon_.+.svg\),\s", string.Empty));
                }
            }

            WriteWithFilteredUrls(this.cssFileResource);
            writer.WriteLine();
            writer.WriteLine();

            if (this.additionalCssFileResources != null && this.additionalCssFileResources.Length > 0)
            {
                foreach (var additionalCssFileResource in this.additionalCssFileResources)
                {
                    writer.WriteLine();
                    writer.WriteLine();
                    WriteWithFilteredUrls(additionalCssFileResource);
                }

                writer.WriteLine();
                writer.WriteLine();
            }

            WriteWithFilteredUrls("chartist.min.css");
        }

        /// <summary>
        /// Writes combined javascript to the provided TextWriter.
        /// </summary>
        private void WriteCombinedJavascript(TextWriter writer)
        {
            writer.WriteLine(ResourceStreamCache.Get("chartist.min.js"));
            writer.WriteLine();

            writer.Write(ResourceStreamCache.Get("custom.js"));

            if (this.classReport)
            {
                return;
            }

            writer.WriteLine();
            writer.WriteLine();

#if NETSTANDARD
            writer.Write(this.javaScriptContent.ToString());

#else
            writer.Write(this.javaScriptContent);
#endif

            writer.WriteLine();

            writer.WriteLine("var translations = {");
            writer.Write("'top': '{0}'", WebUtility.HtmlEncode(ReportResources.Top));
            writer.WriteLine(",");
            writer.Write("'all': '{0}'", WebUtility.HtmlEncode(ReportResources.All));
            writer.WriteLine(",");
            writer.Write("'assembly': '{0}'", WebUtility.HtmlEncode(ReportResources.Assembly2));
            writer.WriteLine(",");
            writer.Write("'class': '{0}'", WebUtility.HtmlEncode(ReportResources.Class2));
            writer.WriteLine(",");
            writer.Write("'method': '{0}'", WebUtility.HtmlEncode(ReportResources.Method));
            writer.WriteLine(",");
            writer.Write("'lineCoverage': '{0}'", WebUtility.HtmlEncode(ReportResources.Coverage));
            writer.WriteLine(",");
            writer.Write("'noGrouping': '{0}'", WebUtility.HtmlEncode(ReportResources.NoGrouping));
            writer.WriteLine(",");
            writer.Write("'byAssembly': '{0}'", WebUtility.HtmlEncode(ReportResources.ByAssembly));
            writer.WriteLine(",");
            writer.Write("'byNamespace': '{0}'", WebUtility.HtmlEncode(ReportResources.ByNamespace));
            writer.WriteLine(",");
            writer.Write("'all': '{0}'", WebUtility.HtmlEncode(ReportResources.All));
            writer.WriteLine(",");
            writer.Write("'collapseAll': '{0}'", WebUtility.HtmlEncode(ReportResources.CollapseAll));
            writer.WriteLine(",");
            writer.Write("'expandAll': '{0}'", WebUtility.HtmlEncode(ReportResources.ExpandAll));
            writer.WriteLine(",");
            writer.Write("'grouping': '{0}'", WebUtility.HtmlEncode(ReportResources.Grouping));
            writer.WriteLine(",");
            writer.Write("'filter': '{0}'", WebUtility.HtmlEncode(ReportResources.Filter));
            writer.WriteLine(",");
            writer.Write("'name': '{0}'", WebUtility.HtmlEncode(ReportResources.Name));
            writer.WriteLine(",");
            writer.Write("'covered': '{0}'", WebUtility.HtmlEncode(ReportResources.Covered));
            writer.WriteLine(",");
            writer.Write("'uncovered': '{0}'", WebUtility.HtmlEncode(ReportResources.Uncovered));
            writer.WriteLine(",");
            writer.Write("'coverable': '{0}'", WebUtility.HtmlEncode(ReportResources.Coverable));
            writer.WriteLine(",");
            writer.Write("'total': '{0}'", WebUtility.HtmlEncode(ReportResources.Total));
            writer.WriteLine(",");
            writer.Write("'coverage': '{0}'", WebUtility.HtmlEncode(ReportResources.Coverage));
            writer.WriteLine(",");
            writer.Write("'branchCoverage': '{0}'", WebUtility.HtmlEncode(ReportResources.BranchCoverage));
            writer.WriteLine(",");
            writer.Write("'methodCoverage': '{0}'", WebUtility.HtmlEncode(ReportResources.CodeElementCoverageQuota));
            writer.WriteLine(",");
            writer.Write("'fullMethodCoverage': '{0}'", WebUtility.HtmlEncode(ReportResources.FullCodeElementCoverageQuota));
            writer.WriteLine(",");
            writer.Write("'percentage': '{0}'", WebUtility.HtmlEncode(ReportResources.Percentage));
            writer.WriteLine(",");
            writer.Write("'history': '{0}'", WebUtility.HtmlEncode(ReportResources.History));
            writer.WriteLine(",");
            writer.Write("'compareHistory': '{0}'", WebUtility.HtmlEncode(ReportResources.CompareHistory));
            writer.WriteLine(",");
            writer.Write("'date': '{0}'", WebUtility.HtmlEncode(ReportResources.Date));
            writer.WriteLine(",");
            writer.Write("'allChanges': '{0}'", WebUtility.HtmlEncode(ReportResources.AllChanges));
            writer.WriteLine(",");
            writer.Write("'selectCoverageTypes': '{0}'", WebUtility.HtmlEncode(ReportResources.SelectCoverageTypes));
            writer.WriteLine(",");
            writer.Write("'selectCoverageTypesAndMetrics': '{0}'", ReportResources.SelectCoverageTypesAndMetrics);
            writer.WriteLine(",");
            writer.Write("'coverageTypes': '{0}'", WebUtility.HtmlEncode(ReportResources.CoverageTypes));
            writer.WriteLine(",");
            writer.Write("'metrics': '{0}'", WebUtility.HtmlEncode(ReportResources.Metrics));
            writer.WriteLine(",");
            writer.Write("'methodCoverageProVersion': '{0}'", WebUtility.HtmlEncode(ReportResources.MethodCoverageProVersion));
            writer.WriteLine(",");
            writer.Write("'lineCoverageIncreaseOnly': '{0}'", WebUtility.HtmlEncode(ReportResources.LineCoverageIncreaseOnly));
            writer.WriteLine(",");
            writer.Write("'lineCoverageDecreaseOnly': '{0}'", WebUtility.HtmlEncode(ReportResources.LineCoverageDecreaseOnly));
            writer.WriteLine(",");
            writer.Write("'branchCoverageIncreaseOnly': '{0}'", WebUtility.HtmlEncode(ReportResources.BranchCoverageIncreaseOnly));
            writer.WriteLine(",");
            writer.Write("'branchCoverageDecreaseOnly': '{0}'", WebUtility.HtmlEncode(ReportResources.BranchCoverageDecreaseOnly));
            writer.WriteLine(",");
            writer.Write("'methodCoverageIncreaseOnly': '{0}'", WebUtility.HtmlEncode(ReportResources.CodeElementCoverageIncreaseOnly));
            writer.WriteLine(",");
            writer.Write("'methodCoverageDecreaseOnly': '{0}'", WebUtility.HtmlEncode(ReportResources.CodeElementCoverageDecreaseOnly));
            writer.WriteLine(",");
            writer.Write("'fullMethodCoverageIncreaseOnly': '{0}'", WebUtility.HtmlEncode(ReportResources.FullCodeElementCoverageIncreaseOnly));
            writer.WriteLine(",");
            writer.Write("'fullMethodCoverageDecreaseOnly': '{0}'", WebUtility.HtmlEncode(ReportResources.FullCodeElementCoverageDecreaseOnly));
            writer.WriteLine();
            writer.WriteLine("};");

            writer.WriteLine();
            writer.WriteLine();

            writer.WriteLine(ResourceStreamCache.Get("runtime.js"));
            writer.WriteLine();

            writer.WriteLine(ResourceStreamCache.Get("polyfills.js"));
            writer.WriteLine();

            writer.Write(ResourceStreamCache.Get("main.js"));
        }

        /// <summary>
        /// Initializes the text writer.
        /// </summary>
        /// <param name="targetPath">The target path.</param>
        private void CreateTextWriter(string targetPath)
        {
            this.reportTextWriter = new StreamWriter(new FileStream(targetPath, FileMode.Create));
        }

        /// <summary>
        /// Saves the report.
        /// </summary>
        private void SaveReport()
        {
            this.FinishReport();

            this.reportTextWriter.Flush();
            this.reportTextWriter.Dispose();

            this.reportTextWriter = null;
        }

        /// <summary>
        /// Finishes the report.
        /// </summary>
        private void FinishReport()
        {
            string fileName = this.classReport ? "class.js" : "main.js";

            this.reportTextWriter.WriteLine("</div></div>");

            if (this.htmlMode == HtmlMode.ExternalCssAndJavaScriptWithQueryStringHandling)
            {
                // #349: Apply query string to referenced CSS and JavaScript files and links
                this.reportTextWriter.Write($@"<script type=""text/javascript"">
/* <![CDATA[ */
(function() {{
    var url = window.location.href;
    var startOfQueryString = url.indexOf('?');
    var queryString = startOfQueryString > -1 ? url.substr(startOfQueryString) : '';

    if (startOfQueryString > -1) {{
        var i = 0, href= null;
        var css = document.getElementsByTagName('link');

        for (i = 0; i < css.length; i++) {{
            if (css[i].getAttribute('rel') !== 'stylesheet') {{
            continue;
            }}

            href = css[i].getAttribute('href');

            if (href) {{
            css[i].setAttribute('href', href + queryString);
            }}
        }}

        var links = document.getElementsByTagName('a');

        for (i = 0; i < links.length; i++) {{
            href = links[i].getAttribute('href');

            if (href
                && !href.startsWith('http://')
                && !href.startsWith('https://')
                && !href.startsWith('#')
                && href.indexOf('?') === -1) {{
            links[i].setAttribute('href', href + queryString);
            }}
        }}
    }}

    var newScript = document.createElement('script');
    newScript.src = '{fileName}' + queryString;
    document.getElementsByTagName('body')[0].appendChild(newScript);
}})();
/* ]]> */ 
</script>");
            }
            else if (this.htmlMode == HtmlMode.InlineCssAndJavaScript)
            {
                this.reportTextWriter.Write("<script type=\"text/javascript\">/* <![CDATA[ */ ");
                this.WriteCombinedJavascript(this.reportTextWriter);
                this.reportTextWriter.WriteLine(" /* ]]> */ </script>");
            }
            else
            {
                this.reportTextWriter.WriteLine($"<script type=\"text/javascript\" src=\"{fileName}\"></script>");
            }

            this.reportTextWriter.Write("</body></html>");
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

        private static class ResourceStreamCache
        {
            private static readonly ConcurrentDictionary<string, string> Cache = new ConcurrentDictionary<string, string>();

            public static string Get(string resourceName) => Cache.GetOrAdd(resourceName, v =>
            {
                using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream("Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering.resources." + resourceName))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            });
        }

        private static class StringBuilderCache
        {
            private static readonly DefaultObjectPool<StringBuilder> Pool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy
            {
                InitialCapacity = 4096,
                MaximumRetainedCapacity = 1 * 1024 * 1024
            });

            internal static StringBuilder Get() => Pool.Get();

            internal static void Return(StringBuilder builder) => Pool.Return(builder);

            internal static string ToStringAndReturnToPool(StringBuilder builder)
            {
                var result = builder.ToString();
                Pool.Return(builder);
                return result;
            }
        }
    }
}
