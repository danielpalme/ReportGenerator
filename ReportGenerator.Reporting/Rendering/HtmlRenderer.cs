using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Palmmedia.ReportGenerator.Parser.Analysis;
using Palmmedia.ReportGenerator.Properties;

namespace Palmmedia.ReportGenerator.Reporting.Rendering
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
<html data-ng-app=""coverageApp"">
<head>
<meta charset=""utf-8"" />
<title>{0} - {1}</title>
{2}
</head><body data-ng-controller=""{3}""><div class=""container"">";

        /// <summary>
        /// The end of each generated HTML file.
        /// </summary>
        private const string HtmlEnd = @"</div>
{0}
</body></html>";

        /// <summary>
        /// The link to the static CSS file.
        /// </summary>
        private const string CssLink = "<link rel=\"stylesheet\" type=\"text/css\" href=\"report.css\" />";

        #endregion

        /// <summary>
        /// Dictionary containing the filenames of the class reports by class.
        /// </summary>
        private static readonly Dictionary<string, string> FileNameByClass = new Dictionary<string, string>();

        /// <summary>
        /// Indicates that only a summary report is created (no class reports).
        /// </summary>
        private readonly bool onlySummary;

        /// <summary>
        /// The report builder.
        /// </summary>
        private TextWriter reportTextWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlRenderer" /> class.
        /// </summary>
        /// <param name="onlySummary">if set to <c>true</c> only a summary report is created (no class reports).</param>
        internal HtmlRenderer(bool onlySummary)
        {
            this.onlySummary = onlySummary;
        }

        /// <summary>
        /// Begins the summary report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="title">The title.</param>
        public void BeginSummaryReport(string targetDirectory, string title)
        {
            string targetPath = Path.Combine(targetDirectory, "index.htm");
            this.CreateTextWriter(targetPath);

            using (var cssStream = GetCombinedCss())
            {
                string style = this.onlySummary ?
                    "<style TYPE=\"text/css\">" + new StreamReader(cssStream).ReadToEnd() + "</style>"
                    : CssLink;

                this.reportTextWriter.WriteLine(HtmlStart, WebUtility.HtmlEncode(title), ReportResources.CoverageReport, style, "SummaryViewCtrl");
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
            string fileName = GetClassReportFilename(assemblyName, className);

            this.CreateTextWriter(Path.Combine(targetDirectory, fileName));

            this.reportTextWriter.WriteLine(HtmlStart, WebUtility.HtmlEncode(className), ReportResources.CoverageReport, CssLink, "DetailViewCtrl");
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
        public void TestMethods(IEnumerable<TestMethod> testMethods)
        {
            if (testMethods == null)
            {
                throw new ArgumentNullException("testMethods");
            }

            if (!testMethods.Any())
            {
                return;
            }

            StringBuilder html = new StringBuilder();

            html.Append("<div id=\"testmethods\" data-ng-class=\"pinned ? \\'pinned\\' : \\'\\'\">");
            html.AppendFormat("<h2 id=\"pinheader\">{0}</h2>", ReportResources.Testmethods);
            html.Append("<a id=\"pin\" data-ng-click=\"togglePin()\">&nbsp;</a><div>");

            int counter = 0;

            html.AppendFormat("<span><input type=\"radio\" name=\"method\" value=\"AllTestMethods\" id=\"method{1}\" data-ng-change=\"switchTestMethod(\\'AllTestMethods\\')\" data-ng-model=\"selectedTestMethod\" /><label for=\"method{1}\" title=\"{0}\">{0}</label></span>", ReportResources.All, counter);

            foreach (var testMethod in testMethods)
            {
                counter++;
                html.AppendFormat(
                    "<span><input type=\"radio\" name=\"method\" value=\"M{0}\" id=\"method{0}\" data-ng-change=\"switchTestMethod(\\'M{0}\\')\" data-ng-model=\"selectedTestMethod\" /><label for=\"method{0}\" title=\"{2}\">{1}</label></span>",
                    testMethod.Id,
                    WebUtility.HtmlEncode(testMethod.ShortName),
                    WebUtility.HtmlEncode(testMethod.Name));
            }

            html.Append("</div></div>");

            this.reportTextWriter.WriteLine("<script type=\"text/javascript\">");
            this.reportTextWriter.WriteLine("/* <![CDATA[ */");
            this.reportTextWriter.WriteLine("document.write('{0}');", html.ToString());
            this.reportTextWriter.WriteLine("/* ]]> */");
            this.reportTextWriter.WriteLine("</script>");
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
            this.reportTextWriter.WriteLine("<table class=\"overview\">");
            this.reportTextWriter.WriteLine("<colgroup>");
            this.reportTextWriter.WriteLine("<col style=\"width: 135px;\" />");
            this.reportTextWriter.WriteLine("<col />");
            this.reportTextWriter.WriteLine("</colgroup>");
            this.reportTextWriter.WriteLine("<tbody>");
        }

        /// <summary>
        /// Adds a summary table to the report.
        /// </summary>
        public void BeginSummaryTable()
        {
            this.reportTextWriter.WriteLine("<div data-ng-if=\"filteringEnabled\" data-reactive-table data-data=\"assemblies\"></div>");

            this.reportTextWriter.WriteLine("<div data-ng-if=\"!filteringEnabled\">");
            this.reportTextWriter.WriteLine(
                "<div id=\"filterButtonContainer\" class=\"customizebox\" style=\"display: none;\"><input data-ng-click=\"enableFiltering()\" value=\"{0}\" title=\"{1}\" type=\"submit\" /></div>",
                ReportResources.ShowCustomizeBox,
                ReportResources.ShowCustomizeBoxHelp);
            this.reportTextWriter.WriteLine("<script type=\"text/javascript\">");
            this.reportTextWriter.WriteLine("/* <![CDATA[ */");
            this.reportTextWriter.WriteLine("document.getElementById('filterButtonContainer').style.display = 'block';");
            this.reportTextWriter.WriteLine("/* ]]> */");
            this.reportTextWriter.WriteLine("</script>");
            this.reportTextWriter.WriteLine("</div>");

            this.reportTextWriter.WriteLine("<table data-ng-if=\"!filteringEnabled\" class=\"overview\">");
            this.reportTextWriter.WriteLine("<colgroup>");
            this.reportTextWriter.WriteLine("<col />");
            this.reportTextWriter.WriteLine("<col style=\"width: 90px;\" />");
            this.reportTextWriter.WriteLine("<col style=\"width: 105px;\" />");
            this.reportTextWriter.WriteLine("<col style=\"width: 100px;\" />");
            this.reportTextWriter.WriteLine("<col style=\"width: 70px;\" />");
            this.reportTextWriter.WriteLine("<col style=\"width: 60px;\" />");
            this.reportTextWriter.WriteLine("<col style=\"width: 112px;\" />");
            this.reportTextWriter.WriteLine("<col style=\"width: 60px;\" />");
            this.reportTextWriter.WriteLine("<col style=\"width: 112px;\" />");
            this.reportTextWriter.WriteLine("</colgroup>");

            this.reportTextWriter.WriteLine(
                "<thead><tr><th>{0}</th><th class=\"right\">{1}</th><th class=\"right\">{2}</th><th class=\"right\">{3}</th><th class=\"right\">{4}</th><th class=\"center\" colspan=\"2\">{5}</th><th class=\"center\" colspan=\"2\">{6}</th></tr></thead>",
                ReportResources.Name,
                ReportResources.Covered,
                ReportResources.Uncovered,
                ReportResources.Coverable,
                ReportResources.Total,
                ReportResources.Coverage,
                ReportResources.BranchCoverage);
            this.reportTextWriter.WriteLine("<tbody>");
        }

        /// <summary>
        /// Adds custom summary elements to the report.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        public void CustomSummary(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException("assemblies");
            }

            this.reportTextWriter.WriteLine("<script type=\"text/javascript\">");
            this.reportTextWriter.WriteLine("/* <![CDATA[ */");

            this.reportTextWriter.WriteLine("var assemblies = [");

            foreach (var assembly in assemblies)
            {
                this.reportTextWriter.WriteLine("  {");
                this.reportTextWriter.WriteLine("    \"name\" : \"{0}\",", assembly.Name);
                this.reportTextWriter.WriteLine("    \"classes\" : [");

                foreach (var @class in assembly.Classes)
                {
                    var historicCoverages = this.FilterHistoricCoverages(@class.HistoricCoverages, 10);

                    var lineCoverageHistory = "[" + string.Join(",", historicCoverages.Select(h => h.CoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture))) + "]";
                    var branchCoverageHistory = "[]";
                    if (historicCoverages.Any(h => h.BranchCoverageQuota.HasValue))
                    {
                        branchCoverageHistory = "[" + string.Join(",", historicCoverages.Select(h => h.BranchCoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture))) + "]";
                    }

                    this.reportTextWriter.Write("    { ");
                    this.reportTextWriter.Write(" \"name\" : \"{0}\",", @class.Name);
                    this.reportTextWriter.Write(
                        " \"reportPath\" : \"{0}\",",
                        this.onlySummary ? string.Empty : GetClassReportFilename(@class.Assembly.ShortName, @class.Name));
                    this.reportTextWriter.Write(" \"coveredLines\" : {0},", @class.CoveredLines);
                    this.reportTextWriter.Write(" \"uncoveredLines\" : {0},", @class.CoverableLines - @class.CoveredLines);
                    this.reportTextWriter.Write(" \"coverableLines\" : {0},", @class.CoverableLines);
                    this.reportTextWriter.Write(" \"totalLines\" : {0},", @class.TotalLines.GetValueOrDefault());
                    this.reportTextWriter.Write(" \"coverageType\" : \"{0}\",", @class.CoverageType);
                    this.reportTextWriter.Write(
                        " \"methodCoverage\" : {0},",
                        @class.CoverageType == CoverageType.MethodCoverage && @class.CoverageQuota.HasValue ? @class.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : "\"-\"");
                    this.reportTextWriter.Write(" \"coveredBranches\" : {0},", @class.CoveredBranches.GetValueOrDefault());
                    this.reportTextWriter.Write(" \"totalBranches\" : {0},", @class.TotalBranches.GetValueOrDefault());
                    this.reportTextWriter.Write(" \"lineCoverageHistory\" : {0},", lineCoverageHistory);
                    this.reportTextWriter.Write(" \"branchCoverageHistory\" : {0}", branchCoverageHistory);

                    this.reportTextWriter.WriteLine(" },");
                }

                this.reportTextWriter.WriteLine("  ]},");
            }

            this.reportTextWriter.WriteLine("];");

            this.reportTextWriter.WriteLine("/* ]]> */");
            this.reportTextWriter.WriteLine("</script>");
        }

        /// <summary>
        /// Adds a metrics table to the report.
        /// </summary>
        /// <param name="headers">The headers.</param>
        public void BeginMetricsTable(IEnumerable<string> headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            this.reportTextWriter.WriteLine("<table class=\"overview\">");
            this.reportTextWriter.Write("<thead><tr>");

            foreach (var header in headers)
            {
                this.reportTextWriter.Write("<th>{0}</th>", WebUtility.HtmlEncode(header));
            }

            this.reportTextWriter.WriteLine("</tr></thead>");
            this.reportTextWriter.WriteLine("<tbody>");
        }

        /// <summary>
        /// Adds a file analysis table to the report.
        /// </summary>
        /// <param name="headers">The headers.</param>
        public void BeginLineAnalysisTable(IEnumerable<string> headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
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
            string value = string.Join("<br />", files.Select(v => string.Format(CultureInfo.InvariantCulture, "<a href=\"#{0}\">{1}</a>", WebUtility.HtmlEncode(ReplaceNonLetterChars(v)), WebUtility.HtmlEncode(v))));

            this.reportTextWriter.WriteLine(
                "<tr><th>{0}</th><td>{1}</td></tr>",
                WebUtility.HtmlEncode(key),
                value);
        }

        /// <summary>
        /// Adds the given metric values to the report.
        /// </summary>
        /// <param name="metric">The metric.</param>
        public void MetricsRow(MethodMetric metric)
        {
            if (metric == null)
            {
                throw new ArgumentNullException("metric");
            }

            this.reportTextWriter.Write("<tr>");

            this.reportTextWriter.Write("<td title=\"{0}\">{1}</td>", WebUtility.HtmlEncode(metric.Name), WebUtility.HtmlEncode(metric.ShortName));

            foreach (var metricValue in metric.Metrics.Select(m => m.Value))
            {
                this.reportTextWriter.Write("<td>{0}</td>", metricValue.ToString(CultureInfo.InvariantCulture));
            }

            this.reportTextWriter.WriteLine("</tr>");
        }

        /// <summary>
        /// Adds the coverage information of a single line of a file to the report.
        /// </summary>
        /// <param name="analysis">The line analysis.</param>
        public void LineAnalysis(LineAnalysis analysis)
        {
            if (analysis == null)
            {
                throw new ArgumentNullException("analysis");
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

            string title = null;
            if (analysis.CoveredBranches.HasValue && analysis.TotalBranches.HasValue && analysis.TotalBranches.Value > 0)
            {
                title = string.Format(ReportResources.CoveredBranches, analysis.CoveredBranches, analysis.TotalBranches);
            }

            if (title != null)
            {
                this.reportTextWriter.Write("<tr title=\"{0}\" data-coverage=\"{{", title);
            }
            else
            {
                this.reportTextWriter.Write("<tr data-coverage=\"{");
            }

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
                "<td class=\"rightmargin right\"><code>{0}</code></td>",
                analysis.LineNumber);

            if (title != null)
            {
                int branchCoverage = (int)(100 * (double)analysis.CoveredBranches.Value / analysis.TotalBranches.Value);
                branchCoverage -= branchCoverage % 10;
                this.reportTextWriter.Write("<td class=\"branch coverage{0}\">&nbsp;</td>", branchCoverage);
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
        /// Charts the specified historic coverages.
        /// </summary>
        /// <param name="historicCoverages">The historic coverages.</param>
        public void Chart(IEnumerable<HistoricCoverage> historicCoverages)
        {
            if (historicCoverages == null)
            {
                throw new ArgumentNullException("historicCoverages");
            }

            historicCoverages = this.FilterHistoricCoverages(historicCoverages, 100);

            var series = new List<string>();
            series.Add("[" + string.Join(",", historicCoverages.Select(h => h.CoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture))) + "]");

            if (historicCoverages.Any(h => h.BranchCoverageQuota.HasValue))
            {
                series.Add("[" + string.Join(",", historicCoverages.Select(h => h.BranchCoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture))) + "]");
            }

            var toolTips = historicCoverages.Select(h =>
                string.Format(
                    "'<h3>{0} - {1}</h3>{2}{3}{4}'",
                    h.ExecutionTime.ToShortDateString(),
                    h.ExecutionTime.ToLongTimeString(),
                    h.CoverageQuota.HasValue ? string.Format(CultureInfo.InvariantCulture, "<br /><span class=\"linecoverage\"></span> {0} {1}% ({2}/{3})", ReportResources.Coverage2, h.CoverageQuota.Value, h.CoveredLines, h.CoverableLines) : null,
                    h.BranchCoverageQuota.HasValue ? string.Format(CultureInfo.InvariantCulture, "<br /><span class=\"branchcoverage\"></span> {0} {1}% ({2}/{3})", ReportResources.BranchCoverage2, h.BranchCoverageQuota.Value, h.CoveredBranches, h.TotalBranches) : null,
                    string.Format(CultureInfo.InvariantCulture, "<br />{0} {1}", ReportResources.TotalLines, h.TotalLines)));

            string id = Guid.NewGuid().ToString("N");
            this.reportTextWriter.WriteLine("<div id=\"chart{0}\" class=\"ct-chart\" data-history-chart data-data=\"chartData{0}\"></div>", id);

            this.reportTextWriter.WriteLine("<script type=\"text/javascript\">");
            this.reportTextWriter.WriteLine("/* <![CDATA[ */");

            this.reportTextWriter.WriteLine("var chartData{0} = {{", id);
            this.reportTextWriter.WriteLine(
                "    \"series\" : [{0}],",
                string.Join(",", series));

            this.reportTextWriter.WriteLine(
                "    \"tooltips\" : [{0}]",
                string.Join(",", toolTips));
            this.reportTextWriter.WriteLine("};");

            this.reportTextWriter.WriteLine("/* ]]> */");
            this.reportTextWriter.WriteLine("</script>");
        }

        /// <summary>
        /// Adds the coverage information of an assembly to the report.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        public void SummaryAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
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
            this.reportTextWriter.Write(
                "<th class=\"right\">{0}</th>",
                assembly.BranchCoverageQuota.HasValue ? assembly.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
            this.reportTextWriter.Write("<th>{0}</th>", CreateCoverageTable(assembly.BranchCoverageQuota));
            this.reportTextWriter.WriteLine("</tr>");
        }

        /// <summary>
        /// Adds the coverage information of a class to the report.
        /// </summary>
        /// <param name="class">The class.</param>
        public void SummaryClass(Class @class)
        {
            if (@class == null)
            {
                throw new ArgumentNullException("class");
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
            this.reportTextWriter.Write(
                "<td class=\"right\">{0}</td>",
                @class.BranchCoverageQuota.HasValue ? @class.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) + "%" : string.Empty);
            this.reportTextWriter.Write("<td>{0}</td>", CreateCoverageTable(@class.BranchCoverageQuota));
            this.reportTextWriter.WriteLine("</tr>");
        }

        /// <summary>
        /// Saves a summary report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        public void SaveSummaryReport(string targetDirectory)
        {
            this.SaveReport();

            if (!this.onlySummary)
            {
                SaveCss(targetDirectory);
                SaveJavaScript(targetDirectory);
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
        /// Releases unmanaged and - optionally - managed resources
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
                    stringBuilder.Append("<td class=\"green\" style=\"width: " + covered + "px;\">&nbsp;</td>");
                }

                if (uncovered > 0)
                {
                    stringBuilder.Append("<td class=\"red\" style=\"width: " + uncovered + "px;\">&nbsp;</td>");
                }
            }
            else
            {
                stringBuilder.Append("<td class=\"gray\" style=\"width: 100px;\">&nbsp;</td>");
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
            if (lineVisitStatus == LineVisitStatus.Covered)
            {
                return lightcolor ? "lightgreen" : "green";
            }
            else if (lineVisitStatus == LineVisitStatus.NotCovered)
            {
                return lightcolor ? "lightred" : "red";
            }
            else
            {
                return lightcolor ? "lightgray" : "gray";
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
                lock (FileNameByClass)
                {
                    if (!FileNameByClass.TryGetValue(key, out fileName))
                    {
                        string shortClassName = className.Substring(className.LastIndexOf('.') + 1);
                        fileName = RendererBase.ReplaceInvalidPathChars(assemblyName + "_" + shortClassName) + ".htm";

                        if (FileNameByClass.Values.Any(v => v.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                        {
                            int counter = 2;

                            do
                            {
                                fileName = RendererBase.ReplaceInvalidPathChars(assemblyName + "_" + shortClassName + counter) + ".htm";
                                counter++;
                            }
                            while (FileNameByClass.Values.Any(v => v.Equals(fileName, StringComparison.OrdinalIgnoreCase)));
                        }

                        FileNameByClass.Add(key, fileName);
                    }
                }
            }

            return fileName;
        }

        /// <summary>
        /// Saves the CSS.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        private static void SaveCss(string targetDirectory)
        {
            string targetPath = Path.Combine(targetDirectory, "report.css");

            using (var fs = new FileStream(targetPath, FileMode.Create))
            {
                using (var cssStream = GetCombinedCss())
                {
                    cssStream.CopyTo(fs);
                }
            }
        }

        /// <summary>
        /// Saves the java script.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        private static void SaveJavaScript(string targetDirectory)
        {
            string targetPath = Path.Combine(targetDirectory, "combined.js");

            using (var fs = new FileStream(targetPath, FileMode.Create))
            {
                using (var javaScriptStream = GetCombinedJavascript())
                {
                    javaScriptStream.CopyTo(fs);
                }
            }
        }

        /// <summary>
        /// Gets the combined CSS.
        /// </summary>
        /// <returns>The combined CSS.</returns>
        private static Stream GetCombinedCss()
        {
            var ms = new MemoryStream();

            using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
                "Palmmedia.ReportGenerator.Reporting.Rendering.resources.custom.css"))
            {
                stream.CopyTo(ms);
            }

            byte[] lineBreak = Encoding.UTF8.GetBytes(Environment.NewLine);
            ms.Write(lineBreak, 0, lineBreak.Length);
            ms.Write(lineBreak, 0, lineBreak.Length);

            using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
                "Palmmedia.ReportGenerator.Reporting.Rendering.resources.chartist.min.css"))
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
        private static Stream GetCombinedJavascript()
        {
            var ms = new MemoryStream();

            using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
                "Palmmedia.ReportGenerator.Reporting.Rendering.resources.jquery-1.11.2.min.js"))
            {
                stream.CopyTo(ms);
            }

            byte[] lineBreak = Encoding.UTF8.GetBytes(Environment.NewLine);
            ms.Write(lineBreak, 0, lineBreak.Length);

            using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
                "Palmmedia.ReportGenerator.Reporting.Rendering.resources.angular.min.js"))
            {
                stream.CopyTo(ms);
            }

            ms.Write(lineBreak, 0, lineBreak.Length);

            using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
                "Palmmedia.ReportGenerator.Reporting.Rendering.resources.react.modified.min.js"))
            {
                stream.CopyTo(ms);
            }

            ms.Write(lineBreak, 0, lineBreak.Length);

            using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
    "Palmmedia.ReportGenerator.Reporting.Rendering.resources.chartist.min.js"))
            {
                stream.CopyTo(ms);
            }

            ms.Write(lineBreak, 0, lineBreak.Length);

            // Required for rendering charts in IE 9
            using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
    "Palmmedia.ReportGenerator.Reporting.Rendering.resources.matchMedia.js"))
            {
                stream.CopyTo(ms);
            }

            ms.Write(lineBreak, 0, lineBreak.Length);
            ms.Write(lineBreak, 0, lineBreak.Length);

            var sb = new StringBuilder();
            sb.AppendLine("var translations = {");
            sb.AppendFormat("'lineCoverage': '{0}'", CoverageType.LineCoverage.ToString());
            sb.AppendLine(",");
            sb.AppendFormat("'noGrouping': '{0}'", ReportResources.NoGrouping);
            sb.AppendLine(",");
            sb.AppendFormat("'byAssembly': '{0}'", ReportResources.ByAssembly);
            sb.AppendLine(",");
            sb.AppendFormat("'byNamespace': '{0}'", ReportResources.ByNamespace);
            sb.AppendLine(",");
            sb.AppendFormat("'all': '{0}'", ReportResources.All);
            sb.AppendLine(",");
            sb.AppendFormat("'collapseAll': '{0}'", ReportResources.CollapseAll);
            sb.AppendLine(",");
            sb.AppendFormat("'expandAll': '{0}'", ReportResources.ExpandAll);
            sb.AppendLine(",");
            sb.AppendFormat("'grouping': '{0}'", ReportResources.Grouping);
            sb.AppendLine(",");
            sb.AppendFormat("'filter': '{0}'", ReportResources.Filter);
            sb.AppendLine(",");
            sb.AppendFormat("'name': '{0}'", ReportResources.Name);
            sb.AppendLine(",");
            sb.AppendFormat("'covered': '{0}'", ReportResources.Covered);
            sb.AppendLine(",");
            sb.AppendFormat("'uncovered': '{0}'", ReportResources.Uncovered);
            sb.AppendLine(",");
            sb.AppendFormat("'coverable': '{0}'", ReportResources.Coverable);
            sb.AppendLine(",");
            sb.AppendFormat("'total': '{0}'", ReportResources.Total);
            sb.AppendLine(",");
            sb.AppendFormat("'coverage': '{0}'", ReportResources.Coverage);
            sb.AppendLine(",");
            sb.AppendFormat("'branchCoverage': '{0}'", ReportResources.BranchCoverage);
            sb.AppendLine(",");
            sb.AppendFormat("'history': '{0}'", ReportResources.History);
            sb.AppendLine();
            sb.AppendLine("};");

            byte[] translations = Encoding.UTF8.GetBytes(sb.ToString());
            ms.Write(translations, 0, translations.Length);

            ms.Write(lineBreak, 0, lineBreak.Length);
            ms.Write(lineBreak, 0, lineBreak.Length);

            using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
                "Palmmedia.ReportGenerator.Reporting.Rendering.resources.customReactComponents.js"))
            {
                stream.CopyTo(ms);
            }

            ms.Write(lineBreak, 0, lineBreak.Length);
            ms.Write(lineBreak, 0, lineBreak.Length);

            using (Stream stream = typeof(HtmlRenderer).Assembly.GetManifestResourceStream(
                "Palmmedia.ReportGenerator.Reporting.Rendering.resources.customAngularApp.js"))
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
            this.reportTextWriter.Write(string.Format(
                CultureInfo.InvariantCulture,
                "<div class=\"footer\">{0} {1} {2}<br />{3} - {4}<br /><a href=\"http://www.palmmedia.de\">www.palmmedia.de</a></div>",
                ReportResources.GeneratedBy,
                typeof(IReportBuilder).Assembly.GetName().Name,
                typeof(IReportBuilder).Assembly.GetName().Version,
                DateTime.Now.ToShortDateString(),
                DateTime.Now.ToLongTimeString()));

            using (var javaScriptStream = GetCombinedJavascript())
            {
                string javascript = this.onlySummary ?
                    "<script type=\"text/javascript\">/* <![CDATA[ */ " + new StreamReader(javaScriptStream).ReadToEnd() + " /* ]]> */ </script>"
                    : "<script type=\"text/javascript\" src=\"combined.js\"></script>";

                this.reportTextWriter.Write(HtmlEnd, javascript);
            }
        }

        /// <summary>
        /// Filters the historic coverages (equal elements are removed).
        /// </summary>
        /// <param name="historicCoverages">The historic coverages.</param>
        /// <param name="maximum">The maximum.</param>
        /// <returns>The filtered historic coverages.</returns>
        private IEnumerable<HistoricCoverage> FilterHistoricCoverages(IEnumerable<HistoricCoverage> historicCoverages, int maximum)
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
