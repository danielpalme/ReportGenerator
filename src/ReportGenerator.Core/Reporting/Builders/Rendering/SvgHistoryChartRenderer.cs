using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering
{
    /// <summary>
    /// Renderes history chart in SVG format.
    /// </summary>
    internal class SvgHistoryChartRenderer
    {
        /// <summary>
        /// Renderes the given historic coverages as SVG image.
        /// </summary>
        /// <param name="historicCoverages">The historic coverages.</param>
        /// <param name="methodCoverageAvailable">if set to <c>true</c> method coverage is available.</param>
        /// <returns>The image in SVG format.</returns>
        public static string RenderHistoryChart(IReadOnlyList<HistoricCoverage> historicCoverages, bool methodCoverageAvailable)
        {
            var sb = new StringBuilder(@"<svg xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"" class=""ct-chart"" width=""1200"" height=""150"" viewBox=""0 0 1200 150"">
<style type=""text/css"">
        <![CDATA[
        .ct-chart .ct-grid {
            stroke: rgba(0,0,0,.2);
            stroke-width: 1px;
            stroke-dasharray: 2px;
        }

        .ct-label {
            fill: rgba(0, 0, 0, 0.4);
            color: rgba(0, 0, 0, 0.4);
            font-size: 12px;
            font-family: sans-serif;
            text-anchor: end;
        }

        .ct-chart .ct-series.ct-series-a .ct-line, .ct-chart .ct-series.ct-series-a .ct-point {
            stroke: #c00;
        }

        .ct-chart .ct-series.ct-series-b .ct-line, .ct-chart .ct-series.ct-series-b .ct-point {
            stroke: #1c2298;
        }

        .ct-chart .ct-series.ct-series-c .ct-line, .ct-chart .ct-series.ct-series-c .ct-point {
            stroke: #0aad0a;
        }

        .ct-chart .ct-series.ct-series-d .ct-line, .ct-chart .ct-series.ct-series-d .ct-point {
            stroke: #FF6A00;
        }

        .ct-chart .ct-line {
            fill: none;
            stroke-width: 2px;
        }

        .ct-chart .ct-point {
            stroke-width: 6px;
            stroke-linecap: round;
            transition: stroke-width .2s;
        }
        ]]>
</style>
<title>Coverage history</title>
<defs>
</defs>
<g>
    <line x1=""50"" x2=""50"" y1=""15"" y2=""115"" class=""ct-grid""></line>
    <line x1=""1190"" x2=""1190"" y1=""15"" y2=""115"" class=""ct-grid""></line>
    <line y1=""115"" y2=""115"" x1=""50"" x2=""1190"" class=""ct-grid""></line>
    <line y1=""90"" y2=""90"" x1=""50"" x2=""1190"" class=""ct-grid""></line>
    <line y1=""65"" y2=""65"" x1=""50"" x2=""1190"" class=""ct-grid""></line>
    <line y1=""40"" y2=""40"" x1=""50"" x2=""1190"" class=""ct-grid""></line>
    <line y1=""15"" y2=""15"" x1=""50"" x2=""1190"" class=""ct-grid""></line>
</g>
    
<g>
    <text x=""45"" y=""113"" class=""ct-label"">0</text>
    <text x=""45"" y=""88"" class=""ct-label"">25</text>
    <text x=""45"" y=""63"" class=""ct-label"">50</text>
    <text x=""45"" y=""38"" class=""ct-label"">75</text>
    <text x=""45"" y=""13"" class=""ct-label"">100</text>
</g>");

            var toolTips = historicCoverages.Select(h =>
                string.Format(
                    "{0} - {1}{2}{3}{4}{5}{6}{7}",
                    h.ExecutionTime.ToShortDateString(),
                    h.ExecutionTime.ToLongTimeString(),
                    h.CoverageQuota.HasValue ? string.Format(CultureInfo.InvariantCulture, "\r\n{0} {1}% ({2}/{3})", WebUtility.HtmlEncode(ReportResources.Coverage2), h.CoverageQuota.Value, h.CoveredLines, h.CoverableLines) : null,
                    h.BranchCoverageQuota.HasValue ? string.Format(CultureInfo.InvariantCulture, "\r\n{0} {1}% ({2}/{3})", WebUtility.HtmlEncode(ReportResources.BranchCoverage2), h.BranchCoverageQuota.Value, h.CoveredBranches, h.TotalBranches) : null,
                    methodCoverageAvailable && h.CodeElementCoverageQuota.HasValue ? string.Format(CultureInfo.InvariantCulture, "\r\n{0} {1}% ({2}/{3})", WebUtility.HtmlEncode(ReportResources.CodeElementCoverageQuota2), h.CodeElementCoverageQuota.Value, h.CoveredCodeElements, h.TotalCodeElements) : null,
                    methodCoverageAvailable && h.FullCodeElementCoverageQuota.HasValue ? string.Format(CultureInfo.InvariantCulture, "\r\n{0} {1}% ({2}/{3})", WebUtility.HtmlEncode(ReportResources.FullCodeElementCoverageQuota), h.FullCodeElementCoverageQuota.Value, h.FullCoveredCodeElements, h.TotalCodeElements) : null,
                    string.Format(CultureInfo.InvariantCulture, "\r\n{0} {1}", WebUtility.HtmlEncode(ReportResources.TotalLines), h.TotalLines),
                    h.Tag != null ? string.Format(CultureInfo.InvariantCulture, "\r\n{0} {1}", WebUtility.HtmlEncode(ReportResources.Tag), h.Tag) : string.Empty))
                .ToArray();

            int numberOfLines = historicCoverages.Count;

            if (numberOfLines == 1)
            {
                numberOfLines = 2;
            }

            float totalWidth = 1190 - 50;
            float width = totalWidth / (numberOfLines - 1);

            float totalHeight = 115 - 15;

            if (historicCoverages.Any(h => h.CoverageQuota.HasValue))
            {
                sb.AppendLine("<g class=\"ct-series ct-series-a\">");

                sb.Append("<path d=\"");

                bool first = true;
                for (int i = 0; i < historicCoverages.Count; i++)
                {
                    if (!historicCoverages[i].CoverageQuota.HasValue)
                    {
                        continue;
                    }

                    float x = 50 + (i * width);
                    float y = 15 + (((100 - (float)historicCoverages[i].CoverageQuota.Value) * totalHeight) / 100);

                    sb.Append(first ? "M" : "L");
                    sb.Append(x.ToString("F3", CultureInfo.InvariantCulture));
                    sb.Append(",");
                    sb.Append(y.ToString("F3", CultureInfo.InvariantCulture));

                    first = false;
                }

                sb.AppendLine("\" class=\"ct-line\"></path>");

                for (int i = 0; i < historicCoverages.Count; i++)
                {
                    if (!historicCoverages[i].CoverageQuota.HasValue)
                    {
                        continue;
                    }

                    float x = 50 + (i * width);
                    float y = 15 + (((100 - (float)historicCoverages[i].CoverageQuota.Value) * totalHeight) / 100);

                    sb.AppendFormat(
                        "<line x1=\"{0}\" y1=\"{2}\" x2=\"{1}\" y2=\"{2}\" class=\"ct-point\"><title>{3}</title></line>",
                        x.ToString("F3", CultureInfo.InvariantCulture),
                        (x + 0.01f).ToString("F3", CultureInfo.InvariantCulture),
                        y.ToString("F3", CultureInfo.InvariantCulture),
                        toolTips[i]);
                }

                sb.AppendLine("</g>");
            }

            if (historicCoverages.Any(h => h.BranchCoverageQuota.HasValue))
            {
                sb.AppendLine("<g class=\"ct-series ct-series-b\">");

                sb.Append("<path d=\"");

                bool first = true;
                for (int i = 0; i < historicCoverages.Count; i++)
                {
                    if (!historicCoverages[i].BranchCoverageQuota.HasValue)
                    {
                        continue;
                    }

                    float x = 50 + (i * width);
                    float y = 15 + (((100 - (float)historicCoverages[i].BranchCoverageQuota.Value) * totalHeight) / 100);

                    sb.Append(first ? "M" : "L");
                    sb.Append(x.ToString("F3", CultureInfo.InvariantCulture));
                    sb.Append(",");
                    sb.Append(y.ToString("F3", CultureInfo.InvariantCulture));

                    first = false;
                }

                sb.AppendLine("\" class=\"ct-line\"></path>");

                for (int i = 0; i < historicCoverages.Count; i++)
                {
                    if (!historicCoverages[i].BranchCoverageQuota.HasValue)
                    {
                        continue;
                    }

                    float x = 50 + (i * width);
                    float y = 15 + (((100 - (float)historicCoverages[i].BranchCoverageQuota.Value) * totalHeight) / 100);

                    sb.AppendFormat(
                        "<line x1=\"{0}\" y1=\"{2}\" x2=\"{1}\" y2=\"{2}\" class=\"ct-point\"><title>{3}</title></line>",
                        x.ToString("F3", CultureInfo.InvariantCulture),
                        (x + 0.01f).ToString("F3", CultureInfo.InvariantCulture),
                        y.ToString("F3", CultureInfo.InvariantCulture),
                        toolTips[i]);
                }

                sb.AppendLine("</g>");
            }

            if (methodCoverageAvailable && historicCoverages.Any(h => h.CodeElementCoverageQuota.HasValue))
            {
                sb.AppendLine("<g class=\"ct-series ct-series-c\">");

                sb.Append("<path d=\"");

                bool first = true;
                for (int i = 0; i < historicCoverages.Count; i++)
                {
                    if (!historicCoverages[i].CodeElementCoverageQuota.HasValue)
                    {
                        continue;
                    }

                    float x = 50 + (i * width);
                    float y = 15 + (((100 - (float)historicCoverages[i].CodeElementCoverageQuota.Value) * totalHeight) / 100);

                    sb.Append(first ? "M" : "L");
                    sb.Append(x.ToString("F3", CultureInfo.InvariantCulture));
                    sb.Append(",");
                    sb.Append(y.ToString("F3", CultureInfo.InvariantCulture));

                    first = false;
                }

                sb.AppendLine("\" class=\"ct-line\"></path>");

                for (int i = 0; i < historicCoverages.Count; i++)
                {
                    if (!historicCoverages[i].CodeElementCoverageQuota.HasValue)
                    {
                        continue;
                    }

                    float x = 50 + (i * width);
                    float y = 15 + (((100 - (float)historicCoverages[i].CodeElementCoverageQuota.Value) * totalHeight) / 100);

                    sb.AppendFormat(
                        "<line x1=\"{0}\" y1=\"{2}\" x2=\"{1}\" y2=\"{2}\" class=\"ct-point\"><title>{3}</title></line>",
                        x.ToString("F3", CultureInfo.InvariantCulture),
                        (x + 0.01f).ToString("F3", CultureInfo.InvariantCulture),
                        y.ToString("F3", CultureInfo.InvariantCulture),
                        toolTips[i]);
                }

                sb.AppendLine("</g>");
            }

            if (methodCoverageAvailable && historicCoverages.Any(h => h.FullCodeElementCoverageQuota.HasValue))
            {
                sb.AppendLine("<g class=\"ct-series ct-series-d\">");

                sb.Append("<path d=\"");

                bool first = true;
                for (int i = 0; i < historicCoverages.Count; i++)
                {
                    if (!historicCoverages[i].FullCodeElementCoverageQuota.HasValue)
                    {
                        continue;
                    }

                    float x = 50 + (i * width);
                    float y = 15 + (((100 - (float)historicCoverages[i].FullCodeElementCoverageQuota.Value) * totalHeight) / 100);

                    sb.Append(first ? "M" : "L");
                    sb.Append(x.ToString("F3", CultureInfo.InvariantCulture));
                    sb.Append(",");
                    sb.Append(y.ToString("F3", CultureInfo.InvariantCulture));

                    first = false;
                }

                sb.AppendLine("\" class=\"ct-line\"></path>");

                for (int i = 0; i < historicCoverages.Count; i++)
                {
                    if (!historicCoverages[i].FullCodeElementCoverageQuota.HasValue)
                    {
                        continue;
                    }

                    float x = 50 + (i * width);
                    float y = 15 + (((100 - (float)historicCoverages[i].FullCodeElementCoverageQuota.Value) * totalHeight) / 100);

                    sb.AppendFormat(
                        "<line x1=\"{0}\" y1=\"{2}\" x2=\"{1}\" y2=\"{2}\" class=\"ct-point\"><title>{3}</title></line>",
                        x.ToString("F3", CultureInfo.InvariantCulture),
                        (x + 0.01f).ToString("F3", CultureInfo.InvariantCulture),
                        y.ToString("F3", CultureInfo.InvariantCulture),
                        toolTips[i]);
                }

                sb.AppendLine("</g>");
            }

            sb.AppendLine("</svg>");

            return sb.ToString();
        }
    }
}
