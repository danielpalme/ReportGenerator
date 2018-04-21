using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;
using Palmmedia.ReportGenerator.Core.Reporting;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Creates badges in SVG format.
    /// </summary>
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
        /// The template for line coverage badge in PNG format.
        /// </summary>
        private const string LineCoveragePngTemplate = "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";

        /// <summary>
        /// The template for branch coverage badge in PNG format.
        /// </summary>
        private const string BranchCoveragePngTemplate = "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";

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        public string ReportType => "Badges";

        /// <summary>
        /// Gets or sets the report configuration.
        /// </summary>
        /// <value>
        /// The report configuration.
        /// </value>
        public IReportContext ReportContext { get; set; }

        /// <summary>
        /// Gets a value indicating whether class reports can be generated in parallel.
        /// </summary>
        public bool SupportsParallelClassReportExecution => true;

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
                    Path.Combine(this.ReportContext.ReportConfiguration.TargetDirectory, "badge_linecoverage.svg"),
                    this.CreateSvgBadge(summaryResult, true, false));

                File.WriteAllBytes(
                    Path.Combine(this.ReportContext.ReportConfiguration.TargetDirectory, "badge_linecoverage.png"),
                    this.CreatePngBadge(summaryResult, true));
            }

            if (summaryResult.BranchCoverageQuota.HasValue)
            {
                File.WriteAllText(
                    Path.Combine(this.ReportContext.ReportConfiguration.TargetDirectory, "badge_branchcoverage.svg"),
                    this.CreateSvgBadge(summaryResult, false, true));

                File.WriteAllBytes(
                    Path.Combine(this.ReportContext.ReportConfiguration.TargetDirectory, "badge_branchcoverage.png"),
                    this.CreatePngBadge(summaryResult, false));
            }

            if (summaryResult.CoverageQuota.HasValue && summaryResult.BranchCoverageQuota.HasValue)
            {
                File.WriteAllText(
                    Path.Combine(this.ReportContext.ReportConfiguration.TargetDirectory, "badge_combined.svg"),
                    this.CreateSvgBadge(summaryResult, true, true));
            }
        }

        /// <summary>
        /// Renderes the SVG.
        /// </summary>
        /// <param name="summaryResult">Indicates whether </param>
        /// <param name="includeLineCoverage">Indicates whether line coverage should be included.</param>
        /// <param name="includeBranchCoverage">Indicates whether branch coverage should be included.</param>
        /// <returns>The rendered SVG.</returns>
        private string CreateSvgBadge(SummaryResult summaryResult, bool includeLineCoverage, bool includeBranchCoverage)
        {
            string lineCoverageClass = includeLineCoverage && includeBranchCoverage ? "linecoverage" : string.Empty;
            string branchCoverageClass = includeLineCoverage && includeBranchCoverage ? "branchcoverage" : string.Empty;

            return string.Format(
                Template,
                ReportResources.CodeCoverage,
                includeLineCoverage ? string.Format(LineCoverageSymbol, lineCoverageClass) : string.Empty,
                includeBranchCoverage ? string.Format(BranchCoverageSymbol, branchCoverageClass) : string.Empty,
                $"{ReportResources.GeneratedBy} ReportGenerator {typeof(IReportBuilder).Assembly.GetName().Version}",
                ReportResources.Coverage3,
                includeLineCoverage ? string.Format(CoverageText, lineCoverageClass, summaryResult.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture)) : string.Empty,
                includeBranchCoverage ? string.Format(CoverageText, branchCoverageClass, summaryResult.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)) : string.Empty,
                includeLineCoverage ? string.Format(CoverageTooltip, lineCoverageClass, ReportResources.Coverage) : string.Empty,
                includeBranchCoverage ? string.Format(CoverageTooltip, branchCoverageClass, ReportResources.BranchCoverage) : string.Empty);
        }

        /// <summary>
        /// Renderes the PNG.
        /// </summary>
        /// <param name="summaryResult">Indicates whether </param>
        /// <param name="lineCoverage">Indicates whether line coverage or branch coverage should be displayed.</param>
        /// <returns>The rendered PNG.</returns>
        private byte[] CreatePngBadge(SummaryResult summaryResult, bool lineCoverage)
        {
            string template = lineCoverage ? LineCoveragePngTemplate : BranchCoveragePngTemplate;
            string text = (lineCoverage ? summaryResult.CoverageQuota.Value : summaryResult.BranchCoverageQuota.Value).ToString(CultureInfo.InvariantCulture) + "%";

            using (var ms = new MemoryStream(Convert.FromBase64String(template)))
            using (var image = Image.Load<Rgba32>(ms))
            using (MemoryStream output = new MemoryStream())
            {
                var font = SystemFonts.CreateFont("Arial", 12, FontStyle.Regular);
                image.Mutate(ctx => ctx.DrawText(text, font, Rgba32.White, new SixLabors.Primitives.PointF(113, 1)));

                image.Save(output, new PngEncoder());
                return output.ToArray();
            }
        }
    }
}