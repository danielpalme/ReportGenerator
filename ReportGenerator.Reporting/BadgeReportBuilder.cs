using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using Palmmedia.ReportGenerator.Parser.Analysis;
using Palmmedia.ReportGenerator.Properties;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Creates badges in SVG format.
    /// </summary>
    [Export(typeof(IReportBuilder))]
    public class BadgeReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The SVG template.
        /// </summary>
        private const string Template = @"<svg xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"" width=""155"" height=""20"">
    <style type=""text/css"">
          <![CDATA[
            @keyframes fadeout {{
              0 % {{ visibility: visible; opacity: 1; }}
              40% {{ visibility: visible; opacity: 1; }}
              50% {{ visibility: hidden; opacity: 0; }}
              90% {{ visibility: hidden; opacity: 0; }}
              100% {{ visibility: visible; opacity: 1; }}
            }}
            @keyframes fadein {{
              0% {{ visibility: hidden; opacity: 0; }}
              40% {{ visibility: hidden; opacity: 0; }}
              50% {{ visibility: visible; opacity: 1; }}
              90% {{ visibility: visible; opacity: 1; }}
              100% {{ visibility: hidden; opacity: 0; }}
            }}
            .linecoverage {{
                animation-duration: 10s;
                animation-name: fadeout;
                animation-iteration-count: infinite;
            }}
            .branchcoverage {{
                animation-duration: 10s;
                animation-name: fadein;
                animation-iteration-count: infinite;
            }}
          ]]>
    </style>
    <title>{0}</title>
    <defs>
        <linearGradient id=""gradient"" x2=""0"" y2=""100%"">
            <stop offset=""0"" stop-color=""#bbb"" stop-opacity="".1""/>
            <stop offset=""1"" stop-opacity="".1""/>
        </linearGradient>

        <linearGradient id=""green"" x2=""0"" y2=""100%"">
            <stop offset=""0"" stop-color=""#00A410""/>
            <stop offset=""1"" stop-color=""#53FF63""/>
        </linearGradient>
        <linearGradient id=""red"" x2=""0"" y2=""100%"">
            <stop offset=""0"" stop-color=""#C00""/>
            <stop offset=""1"" stop-color=""#FF2525""/>
        </linearGradient>
        <linearGradient id=""gray"" x2=""0"" y2=""100%"">
            <stop offset=""0"" stop-color=""#9B9B9B""/>
            <stop offset=""1"" stop-color=""#F3F3F3""/>
        </linearGradient>
        
        <mask id=""mask"">
            <rect width=""155"" height=""20"" rx=""3"" fill=""#fff""/>
        </mask>
        <g id=""icon"">
            <path style=""fill:url(#green);"" d=""M205,202.5 l0,-200 a200,200 0 1,1 -117.558,361.803 z""/>
            <path style=""fill:url(#red);"" d=""M200,202.5 l-117.558,161.803 a200,200 0 0,1 0,-323.607 z""/>
            <path style=""fill:url(#gray);"" d=""M202.5,200 l-117.558,-161.803 a200,200 0 0,1 117.558,-38.196 z""/>
        </g>
    </defs>
    
    <g mask=""url(#mask)"">
        <rect x=""0"" y=""0"" width=""90"" height=""20"" fill=""#444""/>
        <rect x=""90"" y=""0"" width=""20"" height=""20"" fill=""#c00""/>
        <rect x=""110"" y=""0"" width=""45"" height=""20"" fill=""#00B600""/>
        <rect x=""0"" y=""0"" width=""155"" height=""20"" fill=""url(#gradient)""/>
    </g>

    <g>
        {1}
        {2}
    </g>
    
    <g fill=""#fff"" text-anchor=""middle"" font-family=""Verdana,Arial,Geneva,sans-serif"" font-size=""11"">
        <a xlink:href=""https://github.com/danielpalme/ReportGenerator"" target=""_top"">
            <title>{3}</title>
            <use xlink:href=""#icon"" transform=""translate(3,2) scale(.04)""/>
        </a>

        <text x=""53"" y=""15"" fill=""#010101"" fill-opacity="".3"">{4}</text>
        <text x=""53"" y=""14"" fill=""#fff"">{4}</text>
        {5}
        {6}
    </g>

    <g>
        {7}
        {8}
    </g>
</svg>";

        /// <summary>
        /// The template for the line coverage symbol.
        /// </summary>
        private const string LineCoverageSymbol = @"<path class=""{0}"" stroke=""#fff"" d=""M94 6.5 h12 M94 10.5 h12 M94 14.5 h12""/>";

        /// <summary>
        /// The template for the branch coverage symbol.
        /// </summary>
        private const string BranchCoverageSymbol = @"<path class=""{0}"" fill=""#fff"" d=""m 97.627847,15.246584 q 0,-0.36435 -0.255043,-0.619412 -0.255042,-0.254975 -0.619388,-0.254975 -0.364346,0 -0.619389,0.254975 -0.255042,0.255062 -0.255042,0.619412 0,0.36435 0.255042,0.619412 0.255043,0.254975 0.619389,0.254975 0.364346,0 0.619388,-0.254975 0.255043,-0.255062 0.255043,-0.619412 z m 0,-10.4931686 q 0,-0.3643498 -0.255043,-0.6194121 -0.255042,-0.2550624 -0.619388,-0.2550624 -0.364346,0 -0.619389,0.2550624 -0.255042,0.2550623 -0.255042,0.6194121 0,0.3643498 0.255042,0.6193246 0.255043,0.2551499 0.619389,0.2551499 0.364346,0 0.619388,-0.2551499 0.255043,-0.2549748 0.255043,-0.6193246 z m 5.829537,1.1659368 q 0,-0.3643498 -0.255042,-0.6194121 -0.255042,-0.2550624 -0.619388,-0.2550624 -0.364347,0 -0.619389,0.2550624 -0.255042,0.2550623 -0.255042,0.6194121 0,0.3643497 0.255042,0.6193246 0.255042,0.2550623 0.619389,0.2550623 0.364346,0 0.619388,-0.2550623 0.255042,-0.2549749 0.255042,-0.6193246 z m 0.874431,0 q 0,0.4736372 -0.236825,0.8789369 -0.236824,0.4052998 -0.637606,0.6330621 -0.01822,2.6142358 -2.058555,3.7709858 -0.619388,0.346149 -1.849057,0.737799 -1.165908,0.36435 -1.543916,0.646712 -0.378009,0.282363 -0.378009,0.910875 l 0,0.236862 q 0.40078,0.227675 0.637605,0.633062 0.236825,0.4053 0.236825,0.878937 0,0.7287 -0.510084,1.238824 -0.510085,0.510038 -1.238777,0.510038 -0.728692,0 -1.238777,-0.510038 -0.510085,-0.510124 -0.510085,-1.238824 0,-0.473637 0.236825,-0.878937 0.236826,-0.405387 0.637606,-0.633062 l 0,-7.469083 q -0.40078,-0.2277624 -0.637606,-0.6331496 -0.236825,-0.4052998 -0.236825,-0.878937 0,-0.7286996 0.510085,-1.2388242 0.510085,-0.5100372 1.238777,-0.5100372 0.728692,0 1.238777,0.5100372 0.510084,0.5101246 0.510084,1.2388242 0,0.4736372 -0.236825,0.878937 -0.236825,0.4053872 -0.637605,0.6331496 l 0,4.526985 q 0.491866,-0.236862 1.402732,-0.519225 0.500976,-0.154875 0.797007,-0.268712 0.296031,-0.1138373 0.64216,-0.2823623 0.346129,-0.168525 0.537411,-0.3598 0.191281,-0.191275 0.3689,-0.4645374 0.177619,-0.2732623 0.255042,-0.6330621 0.07742,-0.3597998 0.07742,-0.833437 -0.40078,-0.2277623 -0.637606,-0.6330621 -0.236824,-0.4052997 -0.236824,-0.8789369 0,-0.7286996 0.510084,-1.2388243 0.510085,-0.5101246 1.238777,-0.5101246 0.728693,0 1.238777,0.5101246 0.510084,0.5101247 0.510084,1.2388243 z""/>";

        /// <summary>
        /// The template for the coverage text.
        /// </summary>
        private const string CoverageText = @"<text class=""{0}"" x=""132.5"" y=""15"" fill=""#010101"" fill-opacity="".3"">{1}%</text><text class=""{0}"" x=""132.5"" y=""14"">{1}%</text>";

        /// <summary>
        /// The template for the coverage tooltip.
        /// </summary>
        private const string CoverageTooltip = @"<rect class=""{0}"" x=""90"" y=""0"" width=""65"" height=""20"" fill-opacity=""0""><title>{1}</title></rect>";

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        public string ReportType => "Badges";

        /// <summary>
        /// Gets or sets the target directory where reports are stored.
        /// </summary>
        /// <value>
        /// The target directory.
        /// </value>
        public string TargetDirectory { get; set; }

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

            if (summaryResult.CoverageQuota.HasValue)
            {
                File.WriteAllText(
                    Path.Combine(this.TargetDirectory, "badge_linecoverage.svg"),
                    this.CreateBadge(summaryResult, true, false));
            }

            if (summaryResult.BranchCoverageQuota.HasValue)
            {
                File.WriteAllText(
                    Path.Combine(this.TargetDirectory, "badge_branchcoverage.svg"),
                    this.CreateBadge(summaryResult, false, true));
            }

            if (summaryResult.CoverageQuota.HasValue && summaryResult.BranchCoverageQuota.HasValue)
            {
                File.WriteAllText(
                    Path.Combine(this.TargetDirectory, "badge_combined.svg"),
                    this.CreateBadge(summaryResult, true, true));
            }
        }

        /// <summary>
        /// Renderes the SVG.
        /// </summary>
        /// <param name="summaryResult">Indicates whether </param>
        /// <param name="includeLineCoverage">Indicates whether line coverage should be included.</param>
        /// <param name="includeBranchCoverage">Indicates whether branch coverage should be included.</param>
        /// <returns>The rendered SVG.</returns>
        private string CreateBadge(SummaryResult summaryResult, bool includeLineCoverage, bool includeBranchCoverage)
        {
            string lineCoverageClass = includeLineCoverage && includeBranchCoverage ? "linecoverage" : string.Empty;
            string branchCoverageClass = includeLineCoverage && includeBranchCoverage ? "branchcoverage" : string.Empty;

            return string.Format(
                Template,
                ReportResources.CodeCoverage,
                includeLineCoverage ? string.Format(LineCoverageSymbol, lineCoverageClass) : string.Empty,
                includeBranchCoverage ? string.Format(BranchCoverageSymbol, branchCoverageClass) : string.Empty,
                string.Format("{0} {1} {2}", ReportResources.GeneratedBy, typeof(IReportBuilder).Assembly.GetName().Name, typeof(IReportBuilder).Assembly.GetName().Version),
                ReportResources.Coverage3,
                includeLineCoverage ? string.Format(CoverageText, lineCoverageClass, summaryResult.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture)) : string.Empty,
                includeBranchCoverage ? string.Format(CoverageText, branchCoverageClass, summaryResult.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)) : string.Empty,
                includeLineCoverage ? string.Format(CoverageTooltip, lineCoverageClass, ReportResources.Coverage) : string.Empty,
                includeBranchCoverage ? string.Format(CoverageTooltip, branchCoverageClass, ReportResources.BranchCoverage) : string.Empty);
        }
    }
}
