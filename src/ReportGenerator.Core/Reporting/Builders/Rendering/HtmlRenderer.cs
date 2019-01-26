using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering
{
    /// <summary>
    /// HTML report renderer.
    /// </summary>
    internal class HtmlRenderer : RendererBase, IReportRenderer, IDisposable
    {
        #region HTML Snippets

        /// <summary>
        /// The head of each generated HTML file.
        /// </summary>
        private const string HtmlStart = @"<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"" />
<meta http-equiv=""X-UA-Compatible"" content=""IE=EDGE,chrome=1"" />
<link href=""data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAFwklEQVR4AcWXA3Qk2RrH/7e6qtPbaQ6TdKyXsbm2bdv22N7R2raRs9JYyhszk0FvOkYnzfJLnTc3p3rv9GY9X+m7hfv/fagGwRG2PwywdetWUl5e7hNF0QeAv/LKK1f+4wDffPONIXiZqqpntG3DdV13tW3gOC5wt3XM22ElfAZHLPWEkD084cu62br+XH7WBv9fBvjhhx9KAExRFOXCNkHeEDWMHtsEA9fhvkWtaujaQxdgXCEgusAJ63gL/8Jgb//3F4/8SvxDAP6TT7ZEN268d2mfPjf7Hn20t9PphCF6OIAL5BsXRfXYtYa0Yf8/tI/AE/6gXbA/0nxSxSe/C8BfUJAmNzV9pEnSsQqAj08+GaeMGgVBEBgAAIEz41ctCpHItdBNsqZs0GOKJeWDDFvarRVDN4STAlTl5haoweAiXVGyqUjYYsFXV16Jc++914j41xCB4yMXLgpxiQDUp0N6RSDCJq/Vc2rNwO0NDEBDfn53tbl5bZt4DqU2RAy/ymbD2ocfxllXXcUADGk9c1HIEr42QYxymIDomCf8xiyb7/iKknWhdoDI8cdz8vbtP2qSdBIVNZ4xg2x3OlHzxBM49qSTzP0Q+E/o2G9UXrn1kAYT/eF6w8pZ34+X+K9qB4j5fDdp4fCremLkCdEa3pLu3eF86SX06t0bhmmaFsiLDiuFgDv0X9c+UZQph52znxfOqygl2qmnCurGjXtp3ak4BaHiFOjjkhIMfP11+Hw+YxzIjA0qhRV3UA26MyUhAYL6PCzbBtn69SV6Xt4FCAY/Z1JPN4CBevmYY3Deyy/D5XIF0uX+pbpA7oBxlRjX2VfRDGT23ZzzJKKnp78GUbzReLJ9A6jPQBnHOMfhpYsuwnULFwayLUNLZat6R3Ihtjmpn0KszxA9LW0bJKmXWZSFYM81CgLeuvHGwPgZX5cqNpXpAVaU9QXwZUYGQpBlBytqHrPXNKMU2Wl1j+4Q/ApRBptFk/sMQJDo2dkqJInrQJAZr4tGMeGVNKy5zmFMx4owIxaABy8TPSenCarq7TBy07ajTXzh/S58OTPNmMo8eQcwTAZaiF5UtB6yPDi5YCKYPxbDq6dY8MZHBdA4nU7MHH8T5tBcAoSNRO/ZcwEk6QFDoCOIBlHE+7lxvLCkN0Q7Z4qewaB7BolObfhWCM8SedCgY/lIZBkrmAgTliR8ITTj2VUD0JpupbLmErBHamZE084ZTz2L7L7lFi571aqDNiAzGYSkqvi+pQbP/zwQNf0drDiT7OQZaAdo1RqK1mRmEGO8s6BgZondPupw0WuahmWBSrz6Xm/sPbebWZwFoHu2+kwDSj+Hntl16rp7DQB8k5ubUczz5QUOhx2altAD6ysr8e6EHGx6sBB68sUsxpaDzkdgGCJlLQr5WOy5Y/Z/ywnl+iQzc9xwp3Oqz2ZrB9hVU4OPz3di+QuDaKx/MAPsWdEfQ8PzlQv8s3Y/lPCDpHTECEvzgQOLjvV6j8u12/FLYyO+KBTx4zcnQ+Vh2G9Ez+4B1osfjKD6hQNb236QjFg5aVGE+Un2Tu/enVrq6xf3stn6HrA047uyCyB6BCbNGjTzGbNHfaYIwbUNqPnA/4uDSz125/xNB5P+KH2lTx9vpLb2U9Uled6b2X+A7+RcpHhTOqp/UgSxPo7AJ/sR2tKyw53qOnvbvLIDHf4sf/+ii3hJCx41e3DwFSkqXdapZxd0G5YBZ4EHIDq0DtKv6zpC5S1oWFGNpg31Gk8sr3V1d3tw7cxl4T/8z2jgpBEnhVvDk6WIdIzVZYW7uBMcOU7YutphcfDgBA6arEEKiYjXxxD2h9C6qwlySNZ5C/+DwPOTdj29Zc1f/m846JERfSOxyGWqpp2qaWp/TdetSDAdhHAxjuM2coR87zzK+UHZvNV7/tY/p9Tue+UhoWzvxtyIGO0ajUcET6pbsQop1T2zSvwv3fWcgj9gBEfY/gcDB4tklLmGUgAAAABJRU5ErkJggg=="" rel=""icon"" type=""image/x-icon"" />
<title>{0} - {1}</title>
{2}
</head><body><div class=""container""><div class=""containerleft"">";

        /// <summary>
        /// The end of each generated HTML file.
        /// </summary>
        private const string HtmlEnd = @"</div></div>
{0}
</body></html>";

        /// <summary>
        /// The link to the static CSS file.
        /// </summary>
        private const string CssLink = "<link rel=\"stylesheet\" type=\"text/css\" href=\"report.css\" />";

        #endregion

        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(HtmlRenderer));

        /// <summary>
        /// Dictionary containing the filenames of the class reports by class.
        /// </summary>
        private static readonly Dictionary<string, string> FileNameByClass = new Dictionary<string, string>();

        /// <summary>
        /// Indicates that only a summary report is created (no class reports).
        /// </summary>
        private readonly bool onlySummary;

        /// <summary>
        /// Indicates that CSS and JavaScript is included into the HTML instead of seperate files.
        /// </summary>
        private readonly bool inlineCssAndJavaScript;

        /// <summary>
        /// Contains report specific JavaScript content.
        /// </summary>
        private readonly StringBuilder javaScriptContent;

        /// <summary>
        /// The css file resource.
        /// </summary>
        private readonly string cssFileResource;

        /// <summary>
        /// Optional additional CSS file resource.
        /// </summary>
        private readonly string additionalCssFileResource;

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
        /// <param name="onlySummary">if set to <c>true</c> only a summary report is created (no class reports).</param>
        /// <param name="inlineCssAndJavaScript">if set to <c>true</c> CSS and JavaScript is included into the HTML instead of seperate files.</param>
        /// <param name="cssFileResource">Optional CSS file resource.</param>
        /// <param name="additionalCssFileResource">Optional additional CSS file resource.</param>
        internal HtmlRenderer(bool onlySummary, bool inlineCssAndJavaScript, string cssFileResource = "custom.css", string additionalCssFileResource = null)
        {
            this.onlySummary = onlySummary;
            this.inlineCssAndJavaScript = inlineCssAndJavaScript;
            this.javaScriptContent = new StringBuilder();
            this.cssFileResource = cssFileResource;
            this.additionalCssFileResource = additionalCssFileResource;
        }

        /// <summary>
        /// Gets a value indicating whether renderer support rendering of charts.
        /// </summary>
        public bool SupportsCharts => true;

        /// <summary>
        /// Gets a value indicating whether renderer support rendering of charts.
        /// </summary>
        public bool SupportsRiskHotsSpots => true;

        /// <summary>
        /// Begins the summary report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="title">The title.</param>
        public void BeginSummaryReport(string targetDirectory, string fileName, string title)
        {
            string targetPath = Path.Combine(targetDirectory, this.onlySummary ? "summary.htm" : "index.htm");

            if (fileName != null)
            {
                targetPath = Path.Combine(targetDirectory, fileName);
            }

            Logger.InfoFormat("  " + Resources.WritingReportFile, targetPath);
            this.CreateTextWriter(targetPath);

            using (var cssStream = this.GetCombinedCss())
            {
                string style = this.inlineCssAndJavaScript ?
                    "<style TYPE=\"text/css\">" + new StreamReader(cssStream).ReadToEnd() + "</style>"
                    : CssLink;

                this.reportTextWriter.WriteLine(HtmlStart, WebUtility.HtmlEncode(title), WebUtility.HtmlEncode(ReportResources.CoverageReport), style);
            }
        }

        /// <summary>
        /// Begins the class report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        public void BeginClassReport(string targetDirectory, string assemblyName, string className)
        {
            this.classReport = true;

            string targetPath = GetClassReportFilename(assemblyName, className);

            Logger.DebugFormat("  " + Resources.WritingReportFile, targetPath);
            this.CreateTextWriter(Path.Combine(targetDirectory, targetPath));

            using (var cssStream = this.GetCombinedCss())
            {
                string style = this.inlineCssAndJavaScript ?
                    "<style TYPE=\"text/css\">" + new StreamReader(cssStream).ReadToEnd() + "</style>"
                    : CssLink;

                this.reportTextWriter.WriteLine(HtmlStart, WebUtility.HtmlEncode(className), WebUtility.HtmlEncode(ReportResources.CoverageReport), style);
            }
        }

        /// <summary>
        /// Adds a header to the report.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Header(string text)
        {
            this.reportTextWriter.WriteLine("<h1>{0}</h1>", WebUtility.HtmlEncode(text));
        }

        /// <summary>
        /// Adds the test methods to the report.
        /// </summary>
        /// <param name="testMethods">The test methods.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        /// <param name="codeElementsByFileIndex">Code elements by file index.</param>
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

                int coverableLines = fileAnalyses.Sum(f => f.Lines.Count(l => l.LineVisitStatus != LineVisitStatus.NotCoverable));
                int coveredLines = fileAnalyses.Sum(f => f.Lines.Count(l => l.LineVisitStatus > LineVisitStatus.NotCovered));
                decimal? coverage = (coverableLines == 0) ? (decimal?)null : (decimal)Math.Truncate(1000 * (double)coveredLines / (double)coverableLines) / 10;

                int? coverageRounded = null;

                if (coverage.HasValue)
                {
                    coverageRounded = (int)coverage.Value;
                    coverageRounded -= coverageRounded % 10;
                }

                this.reportTextWriter.WriteLine(
                    "<label class=\"percentagebar{0}\" title=\"{1}{2}\"><input type=\"radio\" name=\"method\" value=\"AllTestMethods\" class=\"switchtestmethod\" checked=\"checked\" />{2}</label>",
                    coverage.HasValue ? coverageRounded.ToString() : "undefined",
                    coverage.HasValue ? ReportResources.Coverage2 + " " + coverage.Value.ToString(CultureInfo.InvariantCulture) + "% - " : string.Empty,
                    WebUtility.HtmlEncode(ReportResources.All));

                foreach (var testMethod in testMethods)
                {
                    coveredLines = fileAnalyses.Sum(f => f.Lines.Count(l => l.LineCoverageByTestMethod.ContainsKey(testMethod) && l.LineCoverageByTestMethod[testMethod].LineVisitStatus > LineVisitStatus.NotCovered));
                    coverage = (coverableLines == 0) ? (decimal?)null : (decimal)Math.Truncate(1000 * (double)coveredLines / (double)coverableLines) / 10;

                    coverageRounded = null;

                    if (coverage.HasValue)
                    {
                        coverageRounded = (int)coverage.Value;
                        coverageRounded -= coverageRounded % 10;
                    }

                    this.reportTextWriter.WriteLine(
                        "<br /><label class=\"percentagebar{0}\" title=\"{1}{2}\"><input type=\"radio\" name=\"method\" value=\"M{3}\" class=\"switchtestmethod\" />{4}</label>",
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
                        this.reportTextWriter.WriteLine(
                            "<a href=\"#file{0}_line{1}\" class=\"navigatetohash\" title=\"{2}\"><i class=\"icon-{3}\"></i>{2}</a><br />",
                            item.Key,
                            codeElement.FirstLine,
                            WebUtility.HtmlEncode(codeElement.Name),
                            codeElement.CodeElementType == CodeElementType.Method ? "cube" : "wrench");
                    }
                }
            }

            this.reportTextWriter.WriteLine("<br/></div>");
        }

        /// <summary>
        /// Adds a file of a class to a report.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        public void File(string path)
        {
            this.reportTextWriter.WriteLine("<h2 id=\"{0}\">{1}</h2>", WebUtility.HtmlEncode(HtmlRenderer.ReplaceNonLetterChars(path)), WebUtility.HtmlEncode(path));
        }

        /// <summary>
        /// Adds a paragraph to the report.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Paragraph(string text)
        {
            this.reportTextWriter.WriteLine("<p>{0}</p>", WebUtility.HtmlEncode(text));
        }

        /// <summary>
        /// Adds a table with two columns to the report.
        /// </summary>
        public void BeginKeyValueTable()
        {
            this.reportTextWriter.WriteLine("<table class=\"overview table-fixed\">");
            this.reportTextWriter.WriteLine("<colgroup>");
            this.reportTextWriter.WriteLine("<col class=\"column150\" />");
            this.reportTextWriter.WriteLine("<col />");
            this.reportTextWriter.WriteLine("</colgroup>");
            this.reportTextWriter.WriteLine("<tbody>");
        }

        /// <summary>
        /// Start of risk summary table section.
        /// </summary>
        public void BeginSummaryTable()
        {
            this.reportTextWriter.WriteLine("<coverage-info>");
        }

        /// <summary>
        /// End of risk summary table section.
        /// </summary>
        public void FinishSummaryTable()
        {
            this.reportTextWriter.WriteLine("</coverage-info>");
        }

        /// <summary>
        /// Adds a summary table to the report.
        /// </summary>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        public void BeginSummaryTable(bool branchCoverageAvailable)
        {
            this.reportTextWriter.WriteLine("<table class=\"overview table-fixed stripped\">");
            this.reportTextWriter.WriteLine("<colgroup>");
            this.reportTextWriter.WriteLine("<col />");
            this.reportTextWriter.WriteLine("<col class=\"column90\" />");
            this.reportTextWriter.WriteLine("<col class=\"column105\" />");
            this.reportTextWriter.WriteLine("<col class=\"column100\" />");
            this.reportTextWriter.WriteLine("<col class=\"column70\" />");
            this.reportTextWriter.WriteLine("<col class=\"column60\" />");
            this.reportTextWriter.WriteLine("<col class=\"column112\" />");

            if (branchCoverageAvailable)
            {
                this.reportTextWriter.WriteLine("<col class=\"column60\" />");
                this.reportTextWriter.WriteLine("<col class=\"column112\" />");
            }

            this.reportTextWriter.WriteLine("</colgroup>");

            this.reportTextWriter.Write(
                "<thead><tr><th>{0}</th><th class=\"right\">{1}</th><th class=\"right\">{2}</th><th class=\"right\">{3}</th><th class=\"right\">{4}</th><th class=\"center\" colspan=\"2\">{5}</th>",
                WebUtility.HtmlEncode(ReportResources.Name),
                WebUtility.HtmlEncode(ReportResources.Covered),
                WebUtility.HtmlEncode(ReportResources.Uncovered),
                WebUtility.HtmlEncode(ReportResources.Coverable),
                WebUtility.HtmlEncode(ReportResources.Total),
                WebUtility.HtmlEncode(ReportResources.Coverage));

            if (branchCoverageAvailable)
            {
                this.reportTextWriter.Write(
                "<th class=\"center\" colspan=\"2\">{0}</th>",
                WebUtility.HtmlEncode(ReportResources.BranchCoverage));
            }

            this.reportTextWriter.WriteLine("</tr></thead>");
            this.reportTextWriter.WriteLine("<tbody>");
        }

        /// <summary>
        /// Adds custom summary elements to the report.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="riskHotspots">The risk hotspots.</param>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        public void CustomSummary(IEnumerable<Assembly> assemblies, IEnumerable<RiskHotspot> riskHotspots, bool branchCoverageAvailable)
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

                    var historicCoveragesSb = new StringBuilder();
                    int historicCoveragesCounter = 0;
                    historicCoveragesSb.Append("[");
                    foreach (var historicCoverage in @class.HistoricCoverages)
                    {
                        historicCoverageExecutionTimes.Add(historicCoverage.ExecutionTime);

                        if (historicCoveragesCounter++ > 0)
                        {
                            historicCoveragesSb.Append(", ");
                        }

                        historicCoveragesSb.AppendFormat(
                            "{{ \"et\": \"{0} - {1}\", \"cl\": {2}, \"ucl\": {3}, \"cal\": {4}, \"tl\": {5}, \"lcq\": {6}, \"cb\": {7}, \"tb\": {8}, \"bcq\": {9} }}",
                            historicCoverage.ExecutionTime.ToShortDateString(),
                            historicCoverage.ExecutionTime.ToLongTimeString(),
                            historicCoverage.CoveredLines.ToString(CultureInfo.InvariantCulture),
                            (historicCoverage.CoverableLines - historicCoverage.CoveredLines).ToString(CultureInfo.InvariantCulture),
                            historicCoverage.CoverableLines.ToString(CultureInfo.InvariantCulture),
                            historicCoverage.TotalLines.ToString(CultureInfo.InvariantCulture),
                            historicCoverage.CoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture),
                            historicCoverage.CoveredBranches.ToString(CultureInfo.InvariantCulture),
                            historicCoverage.TotalBranches.ToString(CultureInfo.InvariantCulture),
                            historicCoverage.BranchCoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
                    }

                    historicCoveragesSb.Append("]");

                    this.javaScriptContent.Append("      { ");
                    this.javaScriptContent.AppendFormat("\"name\": \"{0}\",", @class.Name.Replace(@"\", @"\\"));
                    this.javaScriptContent.AppendFormat(
                        " \"rp\": \"{0}\",",
                        this.onlySummary ? string.Empty : GetClassReportFilename(@class.Assembly.ShortName, @class.Name));
                    this.javaScriptContent.AppendFormat(" \"cl\": {0},", @class.CoveredLines);
                    this.javaScriptContent.AppendFormat(" \"ucl\": {0},", @class.CoverableLines - @class.CoveredLines);
                    this.javaScriptContent.AppendFormat(" \"cal\": {0},", @class.CoverableLines);
                    this.javaScriptContent.AppendFormat(" \"tl\": {0},", @class.TotalLines.GetValueOrDefault());
                    this.javaScriptContent.AppendFormat(" \"ct\": \"{0}\",", @class.CoverageType);
                    this.javaScriptContent.AppendFormat(
                        " \"mc\": {0},",
                        @class.CoverageType == CoverageType.MethodCoverage && @class.CoverageQuota.HasValue ? @class.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : "\"-\"");
                    this.javaScriptContent.AppendFormat(" \"cb\": {0},", @class.CoveredBranches.GetValueOrDefault());
                    this.javaScriptContent.AppendFormat(" \"tb\": {0},", @class.TotalBranches.GetValueOrDefault());
                    this.javaScriptContent.AppendFormat(" \"lch\": {0},", lineCoverageHistory);
                    this.javaScriptContent.AppendFormat(" \"bch\": {0},", branchCoverageHistory);
                    this.javaScriptContent.AppendFormat(" \"hc\": {0}", historicCoveragesSb.ToString());

                    this.javaScriptContent.AppendLine(" },");
                }

                this.javaScriptContent.AppendLine("    ]},");
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

                this.javaScriptContent.AppendFormat("\"{0} - {1}\"", item.ToShortDateString(), item.ToLongTimeString());
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
                this.javaScriptContent.AppendFormat(" \"class\": \"{0}\",", riskHotspot.Class.Name);
                this.javaScriptContent.AppendFormat(" \"reportPath\": \"{0}\",", this.onlySummary ? string.Empty : GetClassReportFilename(riskHotspot.Assembly.ShortName, riskHotspot.Class.Name));
                this.javaScriptContent.AppendFormat(" \"methodName\": \"{0}\",", riskHotspot.MethodMetric.FullName);
                this.javaScriptContent.AppendFormat(" \"methodShortName\": \"{0}\",", riskHotspot.MethodMetric.ShortName);
                this.javaScriptContent.AppendFormat(" \"fileIndex\": {0},", riskHotspot.FileIndex);
                this.javaScriptContent.AppendFormat(" \"line\": {0},", !this.onlySummary && riskHotspot.MethodMetric.Line.HasValue ? riskHotspot.MethodMetric.Line.Value.ToString(CultureInfo.InvariantCulture) : "null");
                this.javaScriptContent.AppendLine();
                this.javaScriptContent.AppendLine("    \"metrics\": [");

                foreach (var metric in riskHotspot.StatusMetrics)
                {
                    this.javaScriptContent.Append("      { ");
                    this.javaScriptContent.AppendFormat("\"value\": {0},", metric.Metric.Value.HasValue ? metric.Metric.Value.Value.ToString(CultureInfo.InvariantCulture) : "null");
                    this.javaScriptContent.AppendFormat(" \"exceeded\": {0}", metric.Exceeded.ToString().ToLowerInvariant());
                    this.javaScriptContent.AppendLine(" },");
                }

                this.javaScriptContent.AppendLine("    ]},");
            }

            this.javaScriptContent.AppendLine("];");

            this.javaScriptContent.AppendLine();

            this.javaScriptContent.AppendLine("var branchCoverageAvailable = " + branchCoverageAvailable.ToString().ToLowerInvariant() + ";");
            this.javaScriptContent.AppendLine();
        }

        /// <summary>
        /// Adds a file analysis table to the report.
        /// </summary>
        /// <param name="headers">The headers.</param>
        public void BeginLineAnalysisTable(IEnumerable<string> headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            this.reportTextWriter.WriteLine("<table class=\"lineAnalysis\">");
            this.reportTextWriter.Write("<thead><tr>");

            foreach (var header in headers)
            {
                this.reportTextWriter.Write("<th>{0}</th>", WebUtility.HtmlEncode(header));
            }

            this.reportTextWriter.WriteLine("</tr></thead>");
            this.reportTextWriter.WriteLine("<tbody>");
        }

        /// <summary>
        /// Adds a table row with two cells to the report.
        /// </summary>
        /// <param name="key">The text of the first column.</param>
        /// <param name="value">The text of the second column.</param>
        public void KeyValueRow(string key, string value)
        {
            this.reportTextWriter.WriteLine(
                "<tr><th>{0}</th><td>{1}</td></tr>",
                WebUtility.HtmlEncode(key),
                WebUtility.HtmlEncode(value));
        }

        /// <summary>
        /// Adds a table row with two cells to the report.
        /// </summary>
        /// <param name="key">The text of the first column.</param>
        /// <param name="files">The files.</param>
        public void KeyValueRow(string key, IEnumerable<string> files)
        {
            string value = string.Join("<br />", files.Select(v => string.Format(CultureInfo.InvariantCulture, "<a href=\"#{0}\" class=\"navigatetohash\">{1}</a>", WebUtility.HtmlEncode(ReplaceNonLetterChars(v)), WebUtility.HtmlEncode(v))));

            this.reportTextWriter.WriteLine(
                "<tr><th>{0}</th><td>{1}</td></tr>",
                WebUtility.HtmlEncode(key),
                value);
        }

        /// <summary>
        /// Adds metrics to the report.
        /// </summary>
        /// <param name="class">The class.</param>
        public void MetricsTable(Class @class)
        {
            if (@class == null)
            {
                throw new ArgumentNullException(nameof(@class));
            }

            var firstMethodMetric = @class.Files.SelectMany(f => f.MethodMetrics).First();

            this.reportTextWriter.WriteLine("<table class=\"overview table-fixed\">");
            this.reportTextWriter.Write("<thead><tr>");

            this.reportTextWriter.Write("<th>{0}</th>", WebUtility.HtmlEncode(ReportResources.Method));

            foreach (var met in firstMethodMetric.Metrics)
            {
                if (met.ExplanationUrl == null)
                {
                    this.reportTextWriter.Write("<th>{0}</th>", WebUtility.HtmlEncode(met.Name));
                }
                else
                {
                    this.reportTextWriter.Write("<th>{0} <a href=\"{1}\"><i class=\"icon-info-circled\"></i></a></th>", WebUtility.HtmlEncode(met.Name), WebUtility.HtmlEncode(met.ExplanationUrl.OriginalString));
                }
            }

            this.reportTextWriter.WriteLine("</tr></thead>");
            this.reportTextWriter.WriteLine("<tbody>");

            int fileIndex = 0;

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
                            WebUtility.HtmlEncode(methodMetric.ShortName));
                    }
                    else
                    {
                        this.reportTextWriter.Write("<td title=\"{0}\">{1}</td>", WebUtility.HtmlEncode(methodMetric.FullName), WebUtility.HtmlEncode(methodMetric.ShortName));
                    }

                    foreach (var metricValue in methodMetric.Metrics)
                    {
                        this.reportTextWriter.Write(
                            "<td>{0}{1}</td>",
                            metricValue.Value.HasValue ? metricValue.Value.Value.ToString(CultureInfo.InvariantCulture) : "-",
                            metricValue.Value.HasValue && metricValue.MetricType == MetricType.CoveragePercentual ? "%" : string.Empty);
                    }

                    this.reportTextWriter.WriteLine("</tr>");
                }

                fileIndex++;
            }

            this.reportTextWriter.WriteLine("</tbody>");
            this.reportTextWriter.WriteLine("</table>");
        }

        /// <summary>
        /// Adds metrics to the report.
        /// </summary>
        /// <param name="methodMetrics">The method metrics.</param>
        public void MetricsTable(IEnumerable<MethodMetric> methodMetrics)
        {
            if (methodMetrics == null)
            {
                throw new ArgumentNullException(nameof(methodMetrics));
            }

            var firstMethodMetric = methodMetrics.First();

            this.reportTextWriter.WriteLine("<table class=\"overview table-fixed\">");
            this.reportTextWriter.Write("<thead><tr>");

            this.reportTextWriter.Write("<th>{0}</th>", WebUtility.HtmlEncode(ReportResources.Method));

            foreach (var met in firstMethodMetric.Metrics)
            {
                if (met.ExplanationUrl == null)
                {
                    this.reportTextWriter.Write("<th>{0}</th>", WebUtility.HtmlEncode(met.Name));
                }
                else
                {
                    this.reportTextWriter.Write("<th>{0} <a href=\"{1}\"><i class=\"icon-info-circled\"></i></a></th>", WebUtility.HtmlEncode(met.Name), WebUtility.HtmlEncode(met.ExplanationUrl.OriginalString));
                }
            }

            this.reportTextWriter.WriteLine("</tr></thead>");
            this.reportTextWriter.WriteLine("<tbody>");

            foreach (var methodMetric in methodMetrics)
            {
                this.reportTextWriter.Write("<tr>");

                this.reportTextWriter.Write("<td title=\"{0}\">{1}</td>", WebUtility.HtmlEncode(methodMetric.FullName), WebUtility.HtmlEncode(methodMetric.ShortName));

                foreach (var metricValue in methodMetric.Metrics.Select(m => m.Value))
                {
                    this.reportTextWriter.Write("<td>{0}</td>", metricValue.HasValue ? metricValue.Value.ToString(CultureInfo.InvariantCulture) : "-");
                }

                this.reportTextWriter.WriteLine("</tr>");
            }

            this.reportTextWriter.WriteLine("</tbody>");
            this.reportTextWriter.WriteLine("</table>");
        }

        /// <summary>
        /// Adds the coverage information of a single line of a file to the report.
        /// </summary>
        /// <param name="fileIndex">The index of the file.</param>
        /// <param name="analysis">The line analysis.</param>
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

            this.reportTextWriter.Write("<tr title=\"{0}\" data-coverage=\"{{", WebUtility.HtmlEncode(GetTooltip(analysis)));

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
                this.reportTextWriter.Write("<td class=\"percentagebar{0}\"><i class=\"icon-fork\"></i></td>", branchCoverage);
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

        /// <summary>
        /// Finishes the current table.
        /// </summary>
        public void FinishTable()
        {
            this.reportTextWriter.WriteLine("</tbody>");
            this.reportTextWriter.WriteLine("</table>");
        }

        /// <summary>
        /// Renderes a chart with the given historic coverages.
        /// </summary>
        /// <param name="historicCoverages">The historic coverages.</param>
        /// <param name="renderPngFallBackImage">Indicates whether PNG images are rendered as a fallback.</param>
        public void Chart(IEnumerable<HistoricCoverage> historicCoverages, bool renderPngFallBackImage)
        {
            if (historicCoverages == null)
            {
                throw new ArgumentNullException(nameof(historicCoverages));
            }

            var filteredHistoricCoverages = this.FilterHistoricCoverages(historicCoverages, 100);

            string id = Guid.NewGuid().ToString("N");

            if (renderPngFallBackImage || this.inlineCssAndJavaScript)
            {
                byte[] pngHistory = PngHistoryChartRenderer.RenderHistoryChart(filteredHistoricCoverages);

                this.reportTextWriter.WriteLine(
                    "<div class=\"historychart ct-chart\" data-data=\"historyChartData{0}\"><img src=\"data:image/png;base64,{1}\" /></div>",
                    id,
                    Convert.ToBase64String(pngHistory));
            }
            else
            {
                this.reportTextWriter.WriteLine(
                    "<div class=\"historychart ct-chart\" data-data=\"historyChartData{0}\"></div>",
                    id);
            }

            var series = new List<string>();
            series.Add("[" + string.Join(",", filteredHistoricCoverages.Select(h => h.CoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture))) + "]");

            if (filteredHistoricCoverages.Any(h => h.BranchCoverageQuota.HasValue))
            {
                series.Add("[" + string.Join(",", filteredHistoricCoverages.Select(h => h.BranchCoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture))) + "]");
            }

            var toolTips = filteredHistoricCoverages.Select(h =>
                string.Format(
                    "'<h3>{0} - {1}</h3>{2}{3}{4}{5}'",
                    h.ExecutionTime.ToShortDateString(),
                    h.ExecutionTime.ToLongTimeString(),
                    h.CoverageQuota.HasValue ? string.Format(CultureInfo.InvariantCulture, "<br /><span class=\"linecoverage\"></span> {0} {1}% ({2}/{3})", WebUtility.HtmlEncode(ReportResources.Coverage2), h.CoverageQuota.Value, h.CoveredLines, h.CoverableLines) : null,
                    h.BranchCoverageQuota.HasValue ? string.Format(CultureInfo.InvariantCulture, "<br /><span class=\"branchcoverage\"></span> {0} {1}% ({2}/{3})", WebUtility.HtmlEncode(ReportResources.BranchCoverage2), h.BranchCoverageQuota.Value, h.CoveredBranches, h.TotalBranches) : null,
                    string.Format(CultureInfo.InvariantCulture, "<br />{0} {1}", WebUtility.HtmlEncode(ReportResources.TotalLines), h.TotalLines),
                    h.Tag != null ? string.Format(CultureInfo.InvariantCulture, "<br />{0} {1}", WebUtility.HtmlEncode(ReportResources.Tag), h.Tag) : string.Empty));

            this.reportTextWriter.WriteLine("<script type=\"text/javascript\">/* <![CDATA[ */ ");

            this.reportTextWriter.WriteLine("var historyChartData{0} = {{", id);
            this.reportTextWriter.WriteLine(
                "    \"series\" : [{0}],",
                string.Join(",", series));
            this.reportTextWriter.WriteLine(
                 "    \"tooltips\" : [{0}]",
                 string.Join(",", toolTips));
            this.reportTextWriter.WriteLine("};");
            this.reportTextWriter.WriteLine(" /* ]]> */ </script>");
        }

        /// <summary>
        /// Start of risk hotspots section.
        /// </summary>
        public void BeginRiskHotspots()
        {
            this.reportTextWriter.WriteLine("<risk-hotspots>");
        }

        /// <summary>
        /// End of risk hotspots section.
        /// </summary>
        public void FinishRiskHotspots()
        {
            this.reportTextWriter.WriteLine("</risk-hotspots>");
        }

        /// <summary>
        /// Summary of risk hotspots.
        /// </summary>
        /// <param name="riskHotspots">The risk hotspots.</param>
        public void RiskHotspots(IEnumerable<RiskHotspot> riskHotspots)
        {
            var codeQualityMetrics = riskHotspots.First().MethodMetric.Metrics
                .Where(m => m.MetricType == MetricType.CodeQuality)
                .ToArray();

            this.reportTextWriter.WriteLine("<table class=\"overview table-fixed stripped\">");

            this.reportTextWriter.WriteLine("<colgroup>");
            this.reportTextWriter.WriteLine("<col />");
            this.reportTextWriter.WriteLine("<col />");
            this.reportTextWriter.WriteLine("<col />");

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
                    this.reportTextWriter.WriteLine("<th>{0} <a href=\"{1}\"><i class=\"icon-info-circled\"></i></a></th>", WebUtility.HtmlEncode(metric.Name), WebUtility.HtmlEncode(metric.ExplanationUrl.OriginalString));
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
                        WebUtility.HtmlEncode(GetClassReportFilename(riskHotspot.Assembly.ShortName, riskHotspot.Class.Name)),
                        WebUtility.HtmlEncode(riskHotspot.Class.Name));
                }

                this.reportTextWriter.WriteLine("<tr>");
                this.reportTextWriter.WriteLine("<td>{0}</td>", WebUtility.HtmlEncode(riskHotspot.Assembly.ShortName));
                this.reportTextWriter.WriteLine("<td>{0}</td>", filenameColumn);

                if (!this.onlySummary && riskHotspot.MethodMetric.Line.HasValue)
                {
                    this.reportTextWriter.Write(
                        "<td title=\"{0}\"><a href=\"{1}#file{2}_line{3}\">{4}</a></td>",
                        WebUtility.HtmlEncode(riskHotspot.MethodMetric.FullName),
                        WebUtility.HtmlEncode(GetClassReportFilename(riskHotspot.Assembly.ShortName, riskHotspot.Class.Name)),
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
                        statusMetric.Metric.Value.HasValue ? statusMetric.Metric.Value.Value.ToString(CultureInfo.InvariantCulture) : "-");
                }

                this.reportTextWriter.WriteLine("</tr>");
            }

            this.reportTextWriter.WriteLine("</tbody>");
            this.reportTextWriter.WriteLine("</table>");
        }

        /// <summary>
        /// Adds the coverage information of an assembly to the report.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        public void SummaryAssembly(Assembly assembly, bool branchCoverageAvailable)
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
                assembly.CoverageQuota.HasValue ? CoverageType.LineCoverage.ToString() : string.Empty,
                assembly.CoverageQuota.HasValue ? assembly.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
            this.reportTextWriter.Write("<th>{0}</th>", CreateCoverageTable(assembly.CoverageQuota));

            if (branchCoverageAvailable)
            {
                this.reportTextWriter.Write(
                "<th class=\"right\">{0}</th>",
                assembly.BranchCoverageQuota.HasValue ? assembly.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                this.reportTextWriter.Write("<th>{0}</th>", CreateCoverageTable(assembly.BranchCoverageQuota));
            }

            this.reportTextWriter.WriteLine("</tr>");
        }

        /// <summary>
        /// Adds the coverage information of a class to the report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        public void SummaryClass(Class @class, bool branchCoverageAvailable)
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
                    WebUtility.HtmlEncode(GetClassReportFilename(@class.Assembly.ShortName, @class.Name)),
                    WebUtility.HtmlEncode(@class.Name));
            }

            this.reportTextWriter.Write("<tr>");
            this.reportTextWriter.Write("<td>{0}</td>", filenameColumn);
            this.reportTextWriter.Write("<td class=\"right\">{0}</td>", @class.CoveredLines);
            this.reportTextWriter.Write("<td class=\"right\">{0}</td>", @class.CoverableLines - @class.CoveredLines);
            this.reportTextWriter.Write("<td class=\"right\">{0}</td>", @class.CoverableLines);
            this.reportTextWriter.Write("<td class=\"right\">{0}</td>", @class.TotalLines.GetValueOrDefault());
            this.reportTextWriter.Write(
                "<td title=\"{0}\" class=\"right\">{1}</td>",
                @class.CoverageType,
                @class.CoverageQuota.HasValue ? @class.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
            this.reportTextWriter.Write("<td>{0}</td>", CreateCoverageTable(@class.CoverageQuota));

            if (branchCoverageAvailable)
            {
                this.reportTextWriter.Write(
                    "<td class=\"right\">{0}</td>",
                    @class.BranchCoverageQuota.HasValue ? @class.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
                this.reportTextWriter.Write("<td>{0}</td>", CreateCoverageTable(@class.BranchCoverageQuota));
            }

            this.reportTextWriter.WriteLine("</tr>");
        }

        /// <summary>
        /// Adds the footer to the report.
        /// </summary>
        public void AddFooter()
        {
            this.reportTextWriter.Write(string.Format(
                CultureInfo.InvariantCulture,
                "<div class=\"footer\">{0} ReportGenerator {1}<br />{2} - {3}<br /><a href=\"https://github.com/danielpalme/ReportGenerator\">GitHub</a> | <a href=\"http://www.palmmedia.de\">www.palmmedia.de</a></div>",
                WebUtility.HtmlEncode(ReportResources.GeneratedBy),
                typeof(IReportBuilder).Assembly.GetName().Version,
                DateTime.Now.ToShortDateString(),
                DateTime.Now.ToLongTimeString()));
        }

        /// <summary>
        /// Saves a summary report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        public void SaveSummaryReport(string targetDirectory)
        {
            this.SaveReport();

            if (!this.inlineCssAndJavaScript)
            {
                this.SaveCss(targetDirectory);
                this.SaveJavaScript(targetDirectory);
            }
        }

        /// <summary>
        /// Saves a class report.
        /// </summary><param name="targetDirectory">The target directory.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        public void SaveClassReport(string targetDirectory, string assemblyName, string className)
        {
            this.SaveReport();

            if (!this.inlineCssAndJavaScript)
            {
                this.SaveJavaScript(targetDirectory);
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
                if (this.reportTextWriter != null)
                {
                    this.reportTextWriter.Dispose();
                }
            }
        }

        /// <summary>
        /// Builds a table showing the coverage quota with red and green bars.
        /// </summary>
        /// <param name="coverage">The coverage quota.</param>
        /// <returns>Table showing the coverage quota with red and green bars.</returns>
        private static string CreateCoverageTable(decimal? coverage)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append("<table class=\"coverage\"><tr>");

            if (coverage.HasValue)
            {
                int covered = (int)Math.Round(coverage.Value, 0);
                int uncovered = 100 - covered;

                if (covered > 0)
                {
                    stringBuilder.Append("<td class=\"green covered" + covered + "\">&nbsp;</td>");
                }

                if (uncovered > 0)
                {
                    stringBuilder.Append("<td class=\"red covered" + uncovered + "\">&nbsp;</td>");
                }
            }
            else
            {
                stringBuilder.Append("<td class=\"gray covered100\">&nbsp;</td>");
            }

            stringBuilder.Append("</tr></table>");
            return stringBuilder.ToString();
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

        /// <summary>
        /// Gets the file name of the report file for the given class.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns>The file name.</returns>
        private static string GetClassReportFilename(string assemblyName, string className)
        {
            string key = assemblyName + "_" + className;

            string fileName = null;

            if (!FileNameByClass.TryGetValue(key, out fileName))
            {
                string shortClassName = className.Substring(className.LastIndexOf('.') + 1);
                fileName = RendererBase.ReplaceInvalidPathChars(assemblyName + "_" + shortClassName) + ".htm";

                if (fileName.Length > 100)
                {
                    string firstPart = fileName.Substring(0, 50);
                    string lastPart = fileName.Substring(fileName.Length - 45, 45);

                    fileName = firstPart + lastPart;
                }

                if (FileNameByClass.Values.Any(v => v.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    int counter = 2;
                    string fileNameWithoutExtension = fileName.Substring(0, fileName.Length - 4);

                    do
                    {
                        fileName = fileNameWithoutExtension + counter + ".htm";
                        counter++;
                    }
                    while (FileNameByClass.Values.Any(v => v.Equals(fileName, StringComparison.OrdinalIgnoreCase)));
                }

                FileNameByClass.Add(key, fileName);
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

            using (var fs = new FileStream(targetPath, FileMode.Create))
            {
                using (var cssStream = this.GetCombinedCss())
                {
                    cssStream.CopyTo(fs);

                    if (!this.inlineCssAndJavaScript)
                    {
                        cssStream.Position = 0;
                        string css = new StreamReader(cssStream).ReadToEnd();

                        var matches = Regex.Matches(css, @"url\(icon_(?<filename>.+).svg\),\surl\(data:image/svg\+xml;base64,(?<base64image>.+)\)");

                        foreach (Match match in matches)
                        {
                            System.IO.File.WriteAllBytes(
                                Path.Combine(targetDirectory, "icon_" + match.Groups["filename"].Value + ".svg"),
                                Convert.FromBase64String(match.Groups["base64image"].Value));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves the java script.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        private void SaveJavaScript(string targetDirectory)
        {
            string targetPath = Path.Combine(targetDirectory, this.classReport ? "class.js" : "main.js");

            using (var fs = new FileStream(targetPath, FileMode.Create))
            {
                using (var javaScriptStream = this.GetCombinedJavascript())
                {
                    javaScriptStream.CopyTo(fs);
                }
            }
        }

        /// <summary>
        /// Gets the combined CSS.
        /// </summary>
        /// <returns>The combined CSS.</returns>
        private Stream GetCombinedCss()
        {
            var ms = new MemoryStream();

            using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
                $"Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering.resources.{this.cssFileResource}"))
            {
                stream.CopyTo(ms);
            }

            byte[] lineBreak = Encoding.UTF8.GetBytes(Environment.NewLine);
            ms.Write(lineBreak, 0, lineBreak.Length);
            ms.Write(lineBreak, 0, lineBreak.Length);

            if (this.additionalCssFileResource != null)
            {
                ms.Write(lineBreak, 0, lineBreak.Length);
                ms.Write(lineBreak, 0, lineBreak.Length);

                using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
                    $"Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering.resources.{this.additionalCssFileResource}"))
                {
                    stream.CopyTo(ms);
                }

                ms.Write(lineBreak, 0, lineBreak.Length);
                ms.Write(lineBreak, 0, lineBreak.Length);
            }

            using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
                "Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering.resources.chartist.min.css"))
            {
                stream.CopyTo(ms);
            }

            ms.Position = 0;

            return ms;
        }

        /// <summary>
        /// Gets the combined javascript.
        /// </summary>
        /// <returns>The combined javascript.</returns>
        private Stream GetCombinedJavascript()
        {
            byte[] lineBreak = Encoding.UTF8.GetBytes(Environment.NewLine);

            var ms = new MemoryStream();

            using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
                "Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering.resources.chartist.min.js"))
            {
                stream.CopyTo(ms);
            }

            ms.Write(lineBreak, 0, lineBreak.Length);
            ms.Write(lineBreak, 0, lineBreak.Length);

            using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
                "Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering.resources.custom.js"))
            {
                stream.CopyTo(ms);
            }

            if (this.classReport)
            {
                ms.Position = 0;
                return ms;
            }

            ms.Write(lineBreak, 0, lineBreak.Length);
            ms.Write(lineBreak, 0, lineBreak.Length);

            byte[] assembliesText = Encoding.UTF8.GetBytes(this.javaScriptContent.ToString());
            ms.Write(assembliesText, 0, assembliesText.Length);

            ms.Write(lineBreak, 0, lineBreak.Length);

            var sb = new StringBuilder();
            sb.AppendLine("var translations = {");
            sb.AppendFormat("'top': '{0}'", WebUtility.HtmlEncode(ReportResources.Top));
            sb.AppendLine(",");
            sb.AppendFormat("'all': '{0}'", WebUtility.HtmlEncode(ReportResources.All));
            sb.AppendLine(",");
            sb.AppendFormat("'assembly': '{0}'", WebUtility.HtmlEncode(ReportResources.Assembly2));
            sb.AppendLine(",");
            sb.AppendFormat("'class': '{0}'", WebUtility.HtmlEncode(ReportResources.Class2));
            sb.AppendLine(",");
            sb.AppendFormat("'method': '{0}'", WebUtility.HtmlEncode(ReportResources.Method));
            sb.AppendLine(",");
            sb.AppendFormat("'lineCoverage': '{0}'", CoverageType.LineCoverage.ToString());
            sb.AppendLine(",");
            sb.AppendFormat("'noGrouping': '{0}'", WebUtility.HtmlEncode(ReportResources.NoGrouping));
            sb.AppendLine(",");
            sb.AppendFormat("'byAssembly': '{0}'", WebUtility.HtmlEncode(ReportResources.ByAssembly));
            sb.AppendLine(",");
            sb.AppendFormat("'byNamespace': '{0}'", WebUtility.HtmlEncode(ReportResources.ByNamespace));
            sb.AppendLine(",");
            sb.AppendFormat("'all': '{0}'", WebUtility.HtmlEncode(ReportResources.All));
            sb.AppendLine(",");
            sb.AppendFormat("'collapseAll': '{0}'", WebUtility.HtmlEncode(ReportResources.CollapseAll));
            sb.AppendLine(",");
            sb.AppendFormat("'expandAll': '{0}'", WebUtility.HtmlEncode(ReportResources.ExpandAll));
            sb.AppendLine(",");
            sb.AppendFormat("'grouping': '{0}'", WebUtility.HtmlEncode(ReportResources.Grouping));
            sb.AppendLine(",");
            sb.AppendFormat("'filter': '{0}'", WebUtility.HtmlEncode(ReportResources.Filter));
            sb.AppendLine(",");
            sb.AppendFormat("'name': '{0}'", WebUtility.HtmlEncode(ReportResources.Name));
            sb.AppendLine(",");
            sb.AppendFormat("'covered': '{0}'", WebUtility.HtmlEncode(ReportResources.Covered));
            sb.AppendLine(",");
            sb.AppendFormat("'uncovered': '{0}'", WebUtility.HtmlEncode(ReportResources.Uncovered));
            sb.AppendLine(",");
            sb.AppendFormat("'coverable': '{0}'", WebUtility.HtmlEncode(ReportResources.Coverable));
            sb.AppendLine(",");
            sb.AppendFormat("'total': '{0}'", WebUtility.HtmlEncode(ReportResources.Total));
            sb.AppendLine(",");
            sb.AppendFormat("'coverage': '{0}'", WebUtility.HtmlEncode(ReportResources.Coverage));
            sb.AppendLine(",");
            sb.AppendFormat("'branchCoverage': '{0}'", WebUtility.HtmlEncode(ReportResources.BranchCoverage));
            sb.AppendLine(",");
            sb.AppendFormat("'history': '{0}'", WebUtility.HtmlEncode(ReportResources.History));
            sb.AppendLine(",");
            sb.AppendFormat("'compareHistory': '{0}'", WebUtility.HtmlEncode(ReportResources.CompareHistory));
            sb.AppendLine(",");
            sb.AppendFormat("'date': '{0}'", WebUtility.HtmlEncode(ReportResources.Date));
            sb.AppendLine(",");
            sb.AppendFormat("'allChanges': '{0}'", WebUtility.HtmlEncode(ReportResources.AllChanges));
            sb.AppendLine(",");
            sb.AppendFormat("'lineCoverageIncreaseOnly': '{0}'", WebUtility.HtmlEncode(ReportResources.LineCoverageIncreaseOnly));
            sb.AppendLine(",");
            sb.AppendFormat("'lineCoverageDecreaseOnly': '{0}'", WebUtility.HtmlEncode(ReportResources.LineCoverageDecreaseOnly));
            sb.AppendLine(",");
            sb.AppendFormat("'branchCoverageIncreaseOnly': '{0}'", WebUtility.HtmlEncode(ReportResources.BranchCoverageIncreaseOnly));
            sb.AppendLine(",");
            sb.AppendFormat("'branchCoverageDecreaseOnly': '{0}'", WebUtility.HtmlEncode(ReportResources.BranchCoverageDecreaseOnly));
            sb.AppendLine();
            sb.AppendLine("};");

            byte[] translations = Encoding.UTF8.GetBytes(sb.ToString());
            ms.Write(translations, 0, translations.Length);

            ms.Write(lineBreak, 0, lineBreak.Length);
            ms.Write(lineBreak, 0, lineBreak.Length);

            using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
                "Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering.resources.runtime.js"))
            {
                stream.CopyTo(ms);
            }

            ms.Write(lineBreak, 0, lineBreak.Length);

            using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
                "Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering.resources.polyfills.js"))
            {
                stream.CopyTo(ms);
            }

            ms.Write(lineBreak, 0, lineBreak.Length);
            ms.Write(lineBreak, 0, lineBreak.Length);

            using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
                "Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering.resources.main.js"))
            {
                stream.CopyTo(ms);
            }

            ms.Position = 0;
            return ms;
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
            string javascript = $"<script type=\"text/javascript\" src=\"{(this.classReport ? "class" : "main")}.js\"></script>";

            if (this.inlineCssAndJavaScript)
            {
                using (var javaScriptStream = this.GetCombinedJavascript())
                {
                    javascript = "<script type=\"text/javascript\">/* <![CDATA[ */ " + new StreamReader(javaScriptStream).ReadToEnd() + " /* ]]> */ </script>";
                }
            }

            this.reportTextWriter.Write(HtmlEnd, javascript);
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
