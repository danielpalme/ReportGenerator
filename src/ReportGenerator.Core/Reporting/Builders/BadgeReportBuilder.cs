using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
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
            @keyframes fade1 {{
                0% {{ visibility: visible; opacity: 1; }}
               23% {{ visibility: visible; opacity: 1; }}
               25% {{ visibility: hidden; opacity: 0; }}
               48% {{ visibility: hidden; opacity: 0; }}
               50% {{ visibility: hidden; opacity: 0; }}
               73% {{ visibility: hidden; opacity: 0; }}
               75% {{ visibility: hidden; opacity: 0; }}
               98% {{ visibility: hidden; opacity: 0; }}
              100% {{ visibility: visible; opacity: 1; }}
            }}
            @keyframes fade2 {{
                0% {{ visibility: hidden; opacity: 0; }}
               23% {{ visibility: hidden; opacity: 0; }}
               25% {{ visibility: visible; opacity: 1; }}
               48% {{ visibility: visible; opacity: 1; }}
               50% {{ visibility: hidden; opacity: 0; }}
               73% {{ visibility: hidden; opacity: 0; }}
               75% {{ visibility: hidden; opacity: 0; }}
               98% {{ visibility: hidden; opacity: 0; }}
              100% {{ visibility: hidden; opacity: 0; }}
            }}
            @keyframes fade3 {{
                0% {{ visibility: hidden; opacity: 0; }}
               23% {{ visibility: hidden; opacity: 0; }}
               25% {{ visibility: hidden; opacity: 0; }}
               48% {{ visibility: hidden; opacity: 0; }}
               50% {{ visibility: visible; opacity: 1; }}
               73% {{ visibility: visible; opacity: 1; }}
               75% {{ visibility: hidden; opacity: 0; }}
               98% {{ visibility: hidden; opacity: 0; }}
              100% {{ visibility: hidden; opacity: 0; }}
            }}
            @keyframes fade4 {{
                0% {{ visibility: hidden; opacity: 0; }}
               23% {{ visibility: hidden; opacity: 0; }}
               25% {{ visibility: hidden; opacity: 0; }}
               48% {{ visibility: hidden; opacity: 0; }}
               50% {{ visibility: hidden; opacity: 0; }}
               73% {{ visibility: hidden; opacity: 0; }}
               75% {{ visibility: visible; opacity: 1; }}
               98% {{ visibility: visible; opacity: 1; }}
              100% {{ visibility: hidden; opacity: 0; }}
            }}
            .linecoverage {{
                animation-duration: 15s;
                animation-name: fade1;
                animation-iteration-count: infinite;
            }}
            .branchcoverage {{
                animation-duration: 15s;
                animation-name: fade2;
                animation-iteration-count: infinite;
            }}
            .methodcoverage {{
                animation-duration: 15s;
                animation-name: fade3;
                animation-iteration-count: infinite;
            }}
            .fullmethodcoverage {{
                animation-duration: 15s;
                animation-name: fade4;
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

        <linearGradient id=""c"">
          <stop offset=""0"" stop-color=""#d40000""/>
          <stop offset=""1"" stop-color=""#ff2a2a""/>
        </linearGradient>
        <linearGradient id=""a"">
          <stop offset=""0"" stop-color=""#e0e0de""/>
          <stop offset=""1"" stop-color=""#fff""/>
        </linearGradient>
        <linearGradient id=""b"">
          <stop offset=""0"" stop-color=""#37c837""/>
          <stop offset=""1"" stop-color=""#217821""/>
        </linearGradient>
        <linearGradient xlink:href=""#a"" id=""e"" x1=""106.44"" x2=""69.96"" y1=""-11.96"" y2=""-46.84"" gradientTransform=""matrix(-.8426 -.00045 -.00045 -.8426 -94.27 -75.82)"" gradientUnits=""userSpaceOnUse""/>
        <linearGradient xlink:href=""#b"" id=""f"" x1=""56.19"" x2=""77.97"" y1=""-23.45"" y2=""10.62"" gradientTransform=""matrix(.8426 .00045 .00045 .8426 94.27 75.82)"" gradientUnits=""userSpaceOnUse""/>
        <linearGradient xlink:href=""#c"" id=""g"" x1=""79.98"" x2=""132.9"" y1=""10.79"" y2=""10.79"" gradientTransform=""matrix(.8426 .00045 .00045 .8426 94.27 75.82)"" gradientUnits=""userSpaceOnUse""/>

        <mask id=""mask"">
            <rect width=""155"" height=""20"" rx=""3"" fill=""#fff""/>
        </mask>

        <g id=""icon"" transform=""matrix(.04486 0 0 .04481 -.48 -.63)"">
            <rect width=""52.92"" height=""52.92"" x=""-109.72"" y=""-27.13"" fill=""url(#e)"" transform=""rotate(-135)""/>
            <rect width=""52.92"" height=""52.92"" x=""70.19"" y=""-39.18"" fill=""url(#f)"" transform=""rotate(45)""/>
            <rect width=""52.92"" height=""52.92"" x=""80.05"" y=""-15.74"" fill=""url(#g)"" transform=""rotate(45)""/>
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
        {3}
        {4}
    </g>
    
    <g fill=""#fff"" text-anchor=""middle"" font-family=""Verdana,Arial,Geneva,sans-serif"" font-size=""11"">
        <a xlink:href=""https://github.com/danielpalme/ReportGenerator"" target=""_top"">
            <title>{5}</title>
            <use xlink:href=""#icon"" transform=""translate(3,1) scale(3.5)""/>
        </a>

        <text x=""53"" y=""15"" fill=""#010101"" fill-opacity="".3"">{6}</text>
        <text x=""53"" y=""14"" fill=""#fff"">{6}</text>
        {7}
        {8}
        {9}
        {10}
    </g>

    <g>
        {11}
        {12}
        {13}
        {14}
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
        /// The template for the method coverage symbol.
        /// </summary>
        private const string MethodCoverageSymbol = @"<path class=""{0}"" fill=""#fff"" d=""m 100.5384,15.628605 5.3846,-2.9363 v -5.35096 l -5.3846,1.96033 z M 100,8.350955 105.8726,6.213935 100,4.076925 94.1274,6.213935 Z m 7,-2.12019 v 6.46154 q 0,0.29447 -0.1515,0.54687 -0.1514,0.25241 -0.4122,0.39544 l -5.9231,3.23076 q -0.2356,0.13462 -0.5132,0.13462 -0.2777,0 -0.5133,-0.13462 l -5.923,-3.23076 Q 93.3028,13.491585 93.1514,13.239175 93,12.986775 93,12.692305 v -6.46154 q 0,-0.33654 0.1935,-0.61418 0.1935,-0.27765 0.5132,-0.39543 l 5.9231,-2.15385 q 0.1851,-0.0673 0.3702,-0.0673 0.1851,0 0.3702,0.0673 l 5.923,2.15385 q 0.3197,0.11778 0.5133,0.39543 Q 107,5.894225 107,6.230765 Z""/>";

        /// <summary>
        /// The template for the full method coverage symbol.
        /// </summary>
        private const string FullMethodCoverageSymbol = @"<path class=""{0}"" fill=""#fff"" d=""m 107,6.230765 v 6.46154 c 0,0.196313 -0.0505,0.378603 -0.1515,0.54687 -0.10093,0.168273 -0.23833,0.300087 -0.4122,0.39544 l -5.9231,3.23076 c -0.15707,0.08975 -0.32813,0.13462 -0.5132,0.13462 -0.185133,0 -0.356233,-0.04487 -0.5133,-0.13462 l -5.923,-3.23076 C 93.389767,13.539262 93.252333,13.407448 93.1514,13.239175 93.050467,13.070908 93,12.888618 93,12.692305 v -6.46154 c 0,-0.22436 0.0645,-0.4290867 0.1935,-0.61418 0.129,-0.1851 0.300067,-0.31691 0.5132,-0.39543 l 5.9231,-2.15385 c 0.1234,-0.044867 0.2468,-0.0673 0.3702,-0.0673 0.1234,0 0.2468,0.022433 0.3702,0.0673 l 5.923,2.15385 c 0.21313,0.07852 0.38423,0.21033 0.5133,0.39543 0.129,0.1850933 0.1935,0.38982 0.1935,0.61418 z""/>
          <path class=""{0}""
             style=""fill:none;stroke:#cc0000;stroke-width:1;stroke-linecap:round;stroke-dasharray:none;stroke-opacity:1""
             d=""m 100.0644,16.676777 -0.002,-7.833027 6.90625,-2.546875"" />
          <path class=""{0}""
             style=""fill:none;stroke:#cc0000;stroke-width:1;stroke-linecap:round;stroke-dasharray:none;stroke-opacity:1""
             d=""M 99.914297,8.7645146 93.052236,5.882392"" />";

        /// <summary>
        /// The template for the coverage text.
        /// </summary>
        private const string CoverageText = @"<text class=""{0}"" x=""132.5"" y=""15"" fill=""#010101"" fill-opacity="".3"">{1}</text><text class=""{0}"" x=""132.5"" y=""14"">{1}</text>";

        /// <summary>
        /// The template for the coverage tooltip.
        /// </summary>
        private const string CoverageTooltip = @"<rect class=""{0}"" x=""90"" y=""0"" width=""65"" height=""20"" fill-opacity=""0""><title>{1}</title></rect>";

        /// <summary>
        /// The SVG template (Shields IO format).
        /// </summary>
        private const string ShieldsIoTemplate = @"<svg xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"" width=""{0}"" height=""20""><linearGradient id=""b"" x2=""0"" y2=""100%""><stop offset=""0"" stop-color=""#bbb"" stop-opacity="".1""/><stop offset=""1"" stop-opacity="".1""/></linearGradient><clipPath id=""a""><rect width=""{0}"" height=""20"" rx=""3"" fill=""#fff""/></clipPath><g clip-path=""url(#a)""><path fill=""#555"" d=""M0 0h61v20H0z""/><path fill=""#{1}"" d=""M61 0h{2}v20H61z""/><path fill=""url(#b)"" d=""M0 0h{0}v20H0z""/></g><g fill=""#fff"" text-anchor=""middle"" font-family=""DejaVu Sans,Verdana,Geneva,sans-serif"" font-size=""110""> <text x=""315"" y=""150"" fill=""#010101"" fill-opacity="".3"" transform=""scale(.1)"" textLength=""510"">coverage</text><text x=""315"" y=""140"" transform=""scale(.1)"" textLength=""510"">coverage</text><text x=""{3}"" y=""150"" fill=""#010101"" fill-opacity="".3"" transform=""scale(.1)"" textLength=""{4}"">{5}</text><text x=""{3}"" y=""140"" transform=""scale(.1)"" textLength=""{4}"">{5}</text></g></svg>";

        /// <summary>
        /// Colors for ShiedsIo badges.
        /// </summary>
        private static readonly Tuple<string, string>[] ShieldIoColors = new[]
            {
                Tuple.Create("brightgreen", "4CC61E"),
                Tuple.Create("green", "97C50F"),
                Tuple.Create("yellowgreen", "A3A52A"),
                Tuple.Create("yellow", "D9B226"),
                Tuple.Create("orange", "E87435"),
                Tuple.Create("red", "CA553E"),
                Tuple.Create("lightgrey", "9F9F9F"),
                Tuple.Create("blue", "0374B5"),
            };

        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(BadgeReportBuilder));

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

            string targetPath = Path.Combine(targetDirectory, "badge_linecoverage.svg");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            File.WriteAllText(
                targetPath,
                this.CreateSvgBadge(summaryResult, true, false, false, false));

            foreach (var color in ShieldIoColors)
            {
                targetPath = Path.Combine(targetDirectory, $"badge_shieldsio_linecoverage_{color.Item1}.svg");

                Logger.InfoFormat(Resources.WritingReportFile, targetPath);

                File.WriteAllText(
                    targetPath,
                    this.CreateShieldsIoSvgBadge(summaryResult.CoverageQuota, color.Item2));
            }

            targetPath = Path.Combine(targetDirectory, "badge_branchcoverage.svg");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            File.WriteAllText(
                targetPath,
                this.CreateSvgBadge(summaryResult, false, true, false, false));

            foreach (var color in ShieldIoColors)
            {
                targetPath = Path.Combine(targetDirectory, $"badge_shieldsio_branchcoverage_{color.Item1}.svg");

                Logger.InfoFormat(Resources.WritingReportFile, targetPath);

                File.WriteAllText(
                    targetPath,
                    this.CreateShieldsIoSvgBadge(summaryResult.BranchCoverageQuota, color.Item2));
            }

            targetPath = Path.Combine(targetDirectory, "badge_methodcoverage.svg");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            File.WriteAllText(
                targetPath,
                this.CreateSvgBadge(summaryResult, false, false, true, false));

            foreach (var color in ShieldIoColors)
            {
                targetPath = Path.Combine(targetDirectory, $"badge_shieldsio_methodcoverage_{color.Item1}.svg");

                Logger.InfoFormat(Resources.WritingReportFile, targetPath);

                File.WriteAllText(
                    targetPath,
                    this.CreateShieldsIoSvgBadge(summaryResult.CodeElementCoverageQuota, color.Item2));
            }

            targetPath = Path.Combine(targetDirectory, "badge_fullmethodcoverage.svg");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            File.WriteAllText(
                targetPath,
                this.CreateSvgBadge(summaryResult, false, false, false, true));

            foreach (var color in ShieldIoColors)
            {
                targetPath = Path.Combine(targetDirectory, $"badge_shieldsio_fullmethodcoverage_{color.Item1}.svg");

                Logger.InfoFormat(Resources.WritingReportFile, targetPath);

                File.WriteAllText(
                    targetPath,
                    this.CreateShieldsIoSvgBadge(summaryResult.FullCodeElementCoverageQuota, color.Item2));
            }

            targetPath = Path.Combine(targetDirectory, "badge_combined.svg");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            File.WriteAllText(
                targetPath,
                this.CreateSvgBadge(summaryResult, true, true, true, true));
        }

        /// <summary>
        /// Renderes the SVG.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        /// <param name="includeLineCoverage">Indicates whether line coverage should be included.</param>
        /// <param name="includeBranchCoverage">Indicates whether branch coverage should be included.</param>
        /// <param name="includeMethodCoverage">Indicates whether method coverage should be included.</param>
        /// <param name="includeFullMethodCoverage">Indicates whether full method coverage should be included.</param>
        /// <returns>The rendered SVG.</returns>
        private string CreateSvgBadge(SummaryResult summaryResult, bool includeLineCoverage, bool includeBranchCoverage, bool includeMethodCoverage, bool includeFullMethodCoverage)
        {
            bool all = includeLineCoverage && includeBranchCoverage && includeMethodCoverage && includeFullMethodCoverage;

            string lineCoverageClass = all ? "linecoverage" : string.Empty;
            string branchCoverageClass = all ? "branchcoverage" : string.Empty;
            string methodCoverageClass = all ? "methodcoverage" : string.Empty;
            string fullMethodCoverageClass = all ? "fullmethodcoverage" : string.Empty;

            string lineCoverage = "N/A";
            string branchCoverage = "N/A";
            string methodCoverage = "N/A";
            string fullMethodCoverage = "N/A";

            if (summaryResult.CoverageQuota.HasValue)
            {
                lineCoverage = $"{summaryResult.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}%";
            }

            if (summaryResult.BranchCoverageQuota.HasValue)
            {
                branchCoverage = $"{summaryResult.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}%";
            }

            if (summaryResult.CodeElementCoverageQuota.HasValue)
            {
                methodCoverage = $"{summaryResult.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}%";
            }

            if (summaryResult.FullCodeElementCoverageQuota.HasValue)
            {
                fullMethodCoverage = $"{summaryResult.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)}%";
            }

            return string.Format(
                Template,
                ReportResources.CodeCoverage,
                includeLineCoverage ? string.Format(LineCoverageSymbol, lineCoverageClass) : string.Empty,
                includeBranchCoverage ? string.Format(BranchCoverageSymbol, branchCoverageClass) : string.Empty,
                includeMethodCoverage ? string.Format(MethodCoverageSymbol, methodCoverageClass) : string.Empty,
                includeFullMethodCoverage ? string.Format(FullMethodCoverageSymbol, fullMethodCoverageClass) : string.Empty,
                $"{ReportResources.GeneratedBy} ReportGenerator {typeof(IReportBuilder).Assembly.GetName().Version}",
                ReportResources.Coverage3,
                includeLineCoverage ? string.Format(CoverageText, lineCoverageClass, lineCoverage) : string.Empty,
                includeBranchCoverage ? string.Format(CoverageText, branchCoverageClass, branchCoverage) : string.Empty,
                includeMethodCoverage ? string.Format(CoverageText, methodCoverageClass, methodCoverage) : string.Empty,
                includeFullMethodCoverage ? string.Format(CoverageText, fullMethodCoverageClass, fullMethodCoverage) : string.Empty,
                includeLineCoverage ? string.Format(CoverageTooltip, lineCoverageClass, ReportResources.Coverage) : string.Empty,
                includeBranchCoverage ? string.Format(CoverageTooltip, branchCoverageClass, ReportResources.BranchCoverage) : string.Empty,
                includeMethodCoverage ? string.Format(CoverageTooltip, methodCoverageClass, ReportResources.CodeElementCoverageQuota) : string.Empty,
                includeFullMethodCoverage ? string.Format(CoverageTooltip, fullMethodCoverageClass, ReportResources.FullCodeElementCoverageQuota) : string.Empty);
        }

        /// <summary>
        /// Renderes the SVG in ShieldsIO format.
        /// </summary>
        /// <param name="coverage">The coverage.</param>
        /// <param name="backgroundColor">The background color.</param>
        /// <returns>The rendered SVG.</returns>
        private string CreateShieldsIoSvgBadge(decimal? coverage, string backgroundColor)
        {
            string text = "N/A";
            bool wide = false;

            if (coverage.HasValue)
            {
                coverage = Math.Floor(coverage.Value);
                text = $"{coverage.Value.ToString(CultureInfo.InvariantCulture)}%";
                wide = coverage.Value >= 100m;
            }

            return string.Format(
                ShieldsIoTemplate,
                wide ? "104" : "96",
                backgroundColor,
                wide ? "43" : "35",
                wide ? "815" : "775",
                wide ? "330" : "250",
                text);
        }
    }
}