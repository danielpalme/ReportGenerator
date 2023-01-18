using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
               27% {{ visibility: visible; opacity: 1; }}
               33% {{ visibility: hidden; opacity: 0; }}
               60% {{ visibility: hidden; opacity: 0; }}
               66% {{ visibility: hidden; opacity: 0; }}
               93% {{ visibility: hidden; opacity: 0; }}
              100% {{ visibility: visible; opacity: 1; }}
            }}
            @keyframes fade2 {{
                0% {{ visibility: hidden; opacity: 0; }}
               27% {{ visibility: hidden; opacity: 0; }}
               33% {{ visibility: visible; opacity: 1; }}
               60% {{ visibility: visible; opacity: 1; }}
               66% {{ visibility: hidden; opacity: 0; }}
               93% {{ visibility: hidden; opacity: 0; }}
              100% {{ visibility: hidden; opacity: 0; }}
            }}
            @keyframes fade3 {{
                0% {{ visibility: hidden; opacity: 0; }}
               27% {{ visibility: hidden; opacity: 0; }}
               33% {{ visibility: hidden; opacity: 0; }}
               60% {{ visibility: hidden; opacity: 0; }}
               66% {{ visibility: visible; opacity: 1; }}
               93% {{ visibility: visible; opacity: 1; }}
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
    </g>
    
    <g fill=""#fff"" text-anchor=""middle"" font-family=""Verdana,Arial,Geneva,sans-serif"" font-size=""11"">
        <a xlink:href=""https://github.com/danielpalme/ReportGenerator"" target=""_top"">
            <title>{4}</title>
            <use xlink:href=""#icon"" transform=""translate(3,1) scale(3.5)""/>
        </a>

        <text x=""53"" y=""15"" fill=""#010101"" fill-opacity="".3"">{5}</text>
        <text x=""53"" y=""14"" fill=""#fff"">{5}</text>
        {6}
        {7}
        {8}
    </g>

    <g>
        {9}
        {10}
        {11}
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
        /// The template for line coverage badge in PNG format.
        /// </summary>
        private const string LineCoveragePngTemplate = "iVBORw0KGgoAAAANSUhEUgAAAJsAAAAUCAMAAACtWb+zAAABgFBMVEUreCtOTk4QtBA+Pj4BpQFOTk49PT0StRIBpAFLS0s/Pz89PT1NTU1CQkIOsg5ISEhPT08MsAwKrgoIrAgGqgYCpgJEREQApABGRkZKSkrGDg4FqAUEpwQRtBEQsxAStRK+Bgb////EDAzBCQm8BATJEhLIERHHEBDCCwu/CAi6AwO5AgK4AQG3AAA1NTXe3t5SUlKCgoLT1NNaWlpWVlY3Nzf7KCjs7OwyMjLh4eHQ0NC5ubmysrKmpqba2trLy8vBwcGsrKygoKB6enpkZGS9vb22trY7Ozv2IiLuGRnlERHdCQnUAQH6+vr09fTw8fDl5eWUlJRfX19KTkp7OjryHR3pFRXhDQ3YBATo6OfX19fGx8Zqamo8pDw3yDdvMjIrmitzc3NwcHBCUkJ2ODgzujMxtDEtpS2MTRj8/PyampqYmJiQkJCIiIhvzm9ms2bPY188PDw2jzY1wjUvri8piSmVSiGBRBKRkZFCiEI5mzmjNjYvqS8nkCePHx+EURZSNd53AAAACXRSTlMG5+fm5YqJiYgKZ7i6AAACsUlEQVRIx83O11LbQBSAYVECWNZmnbZZpZv0pi5kyZJ7oblCigsQeodQ05NXj4SLnMxeIA+e5LtY6ZyVZn6KutQX6I3HBPfPrX/YTmN65QnBXQ+GqT6myefbYma+MBfnKcE9D/opX1ssWJubn5vxXZRbBJe9aLVBXzqX35uft+P+tzYWhvOFvbXV1aOjP+PKwj9sYx3Qlylo++u/No7X1la/zrBNAVPj81Kd7c6oLdjBma94QUEHa2qV/R+fpt9vrK9/Pz5kYUM4ClQumYRQkKErUG7d+2T57FUWYPvhGiXook2RKtKhnTb1buODbRk2tpsBDCENoFmsSoqcFyCs7UKLj+opmDalXDkf4XnZ/pnnC7swFYnqcdjhGcFVLyjsQIqhf3TSJt+enpws44ZFDWGHaKhcwkTpHaxoOFlTVTGNwzVFxQccF9/CmQTHhUVs1FU1ksKuhwTdtCGlmTbx5rSVhpe0xjP+GeMUj5ckvG0hKxKLRas4LAKEtw1eS2BdxvbI5mKxWEX8qy3YwZmveUGhhp+ttPGxb6gpsCkjR7yEUD2CQDHFy8iyBEFgUUZEaEdiuFIC6SmEwiLMKYLAYOR6TtBV28J0K21sFjWBTFRASFxMShBZcYRKUhSgRZ4BoH7WlrAADCeQaQpLugiiJQAUBbkeEFz3ggJNC1OtNNBGmwW+IJX98WLVwAAwuSQA/pJW1SyQEQEQKrxuJABOF9NGEgQMnS+Wgevi2sDCZCvNxYVgNkQDLovsE9ChrL1TQ5BT7ZU9cwwXUgFtHxHZuXW+cj2yBTs4820vKODGTbTTXDTdPF3uRPudMxmJ8XF/e++2EdzxgqJdK+Njs7R3fiEFOcL+BcENL6iBzrgVuhsHWT9p/ZLAS9ogNeTvlVcENz0YoaihAX9vvCY4f9ngCPUbjSNAsjHFfhsAAAAASUVORK5CYII=";

        /// <summary>
        /// The template for branch coverage badge in PNG format.
        /// </summary>
        private const string BranchCoveragePngTemplate = "iVBORw0KGgoAAAANSUhEUgAAAJsAAAAUCAMAAACtWb+zAAAB+1BMVEU+Pj4BpQFOTk49PT0StRIBpAFJSUkAqgBNTU0Psw9PT08StRJLS0s/Pz89PT1NTU1JSUkNsQ1BQUEKrgoHqwcEqARPT08CpgIApABFRUVDQ0NHR0cPsg8MrwwJrAkGqQYRtBEQsxAStRLJERG4AAC+Bga8BATACAjFDg7HEBC5AgLGDw+6AwM1NTXDDAz////CCwvBCgrEDQ2Dg4M3NzfV1dRYWFgyMjLe3t7d3d2lpaVbW1tVVVU7Ozvj4+PPz8/Ly8vBwcGgoKBoaGhSUlJQUFDy8vHn5+b55eW9vb22trazs7N6Ojr2IiLlERHdCQn6+vrY2NiUlJTji4txcXFfX19KTkozuTMqmSr6KirJKir7JibyHR3uGRnpFRXhDQ3YBATUAADu7u7q6urGx8a6urq4uLisrKyqqqp6enrTZGM3yDctpi319vT77+/s7evg4ODa2trR0dHz0NDwxMTppKSZmZmQkJDfhIR2dnbcbW1jY2PWWVl0NjbOMzNvMTEwrzDHJibKICCTSx+ITha/FRX8/Pz88/Pyycnuvr6wsLCKiop+fn5vzm9ms2bUSUlBUUE8pTw3lzc1wjUoiyiBRBL33t7O2c3qsbHrra3jk5PeeHjZcnJiYmLSUFDNRkZEVURCiEI+oz48pDw3izajNjaPHx/sHh7TBQUJLNt4AAAADHRSTlPm5YqJiYgHBufn5ub2Cs1oAAADMklEQVRIx83OV1vTUBjA8bhX08ao6DnHrUgdIUKTdO/dotJFFTd7b9kgMpQNAu69/ZhmlSD2wlR4Hn8X7/ucN7n4Yzu37s3bHGeyOP3X9m3fhW3VbZZsbSdV2I7t1sk0Go+u7NMGtp3P4pQKezDNKjdd9bnxaZlmo1wU9IcmRy8qjqqRaSM04dLmucbGDYzTC0KjiTd6ff9kQi/arwYmpxHVzYG5dDq9vPzlt7iU/9/auhKdC/rR58NdCzm0ESJNpKJ1vml8fGQknV4qI2T+aIU5wHqJ3BQLhulXxcXnQ4Odw+Izl7Zoa3D++rP798abmppGlghZjQsy1GwfQfg4QpGXImQaq1XcVpuyFIWCcvoBP/vpRKHomBoYEtjY4IuvfNq1u2PXeR+RdH2cx0+cJKKOGMtZm/0IVc0gi8nVNoDC02xpKhAzmznEsSZzYAa9i7naatEalwV8Gz976cHLogNqSG2AczoahLSrd8ZWVhaRJFkJxW1wMpQnCsI9iKsA9VUMMxtGNVU2BtkpqrYbRTwUVW1ATi/DmL1IUSTg24qKBp+0TBSJ1LUBEbTJabdvjS0CWbJS2rU9AAyYQZIF3RZgaXe7XTFQY4AAdTtNlRbgsAJQbSBK3W530AAUBYJH9FQiRD95XyA5rgYGJN8zaVdufgMy/2OruOviAHjNgAx6TRywWHw+PwEifEVPh46Ke4BjQGrjfD4d+rONpltelxfIDqqBQcnDn5m0GyVQRkZcfggNffUdCFrqIIyzLhImTToSemHEAKGnjkQ1Hjgd9SXbDNAVJ0nOBhX5gkd0S0Nh/iq1bXLcNSUtA48GTAE2pa11xJwIQl1pPYTaeCtbYRHbbJVmh9MDUTgYdtZDf4fDFEytbyunX+avcUINjMx4eFVKI9egjITdiJOUHfKTxI12/sYYCYrhT/yb0lFGhsT50W7lv4p/Kc4Jpno7P5xTHFJDaSOHfqymKXBcngrlhWuF2dfuNtVp5fv6ttBE79uc23DF0JUbJbh6Wp+XoLLcLwkaQl0TlxSH1cDWxpUM4bmw27XZzheyUJO2Bdum3Sxnsziiwg5s5zZMK/rP2rbs2PULwDaL24J71s4AAAAASUVORK5CYII=";

        /// <summary>
        /// The template for method coverage badge in PNG format.
        /// </summary>
        private const string MethodCoveragePngTemplate = "iVBORw0KGgoAAAANSUhEUgAAAJsAAAAUCAMAAACtWb+zAAAB3VBMVEU+Pj4BpQE9PT0StRIBpAFJSUkAqgBNTU0Psw9PT08StRJPT09OTk5AQEBLS0s9PT1OTk5JSUkOsg4MsAwKrgoHqwcEqARNTU1CQkJEREQApAD///8JrAkGqQYDpgMRtBEQsxACpQIStRLIEBBGRkZHR0e5AQHFDQ1ISEjJEhK+Bga3AADBCQm8BAQ1NTXGDw/DCwve3t5RUVG6AwPRz883NzfLv7/7KCi/CAi3t7dcXFypQ0MyMjL7+/tVVVU7Ozvm5ubi4uLa2trBwcGzs7OlpaWhoaGpVlb19vXt7e3b19fT09O7u7uVlZWCgoJiYmJXV1f2IiLuGRnlERHdCQnUAQHV1dW4nJxxcXFZWVlKTkryHR3pFRXhDQ3YBATz8/Px8fDq6urg4ODKysrGx8a9vb2/qqqpqamuf397e3t3d3esdXVnZ2c8pDw3yDcrmivMzMysrKybm5uQkJCEhISAgICqbW1ra2tCUkKrPT18Ozt5OTkzujMxtDEvrC8tpS0ojSiIThbNw8OwsLC1lJSKiopqwGrPY188ijw3lzd0Nzc1wjVxNDRtMDCwIyO2FRWBRBLO2c3JvLzBrq6urq6wh4dx1XFkrGSpYWGjNjasNDSXSSSTTB+PHx+QTRwn1+yJAAAADXRSTlPm5YmJiAcG5+fm5oqKkukDfQAAA0dJREFUSMe11Gd30lAYwPG4Z4C4uFfcJhIRIokJQti7bFyU1tG9a+2wSzvde+/1Wb23JE318MJb29+L5LlP8uJ/OJxQW3btsKyP03Wc+Gc7N22ldlt0Vqt1TdtO1nGUwCZqm1VjMo1amp9Y1069tmMEtlOmZXFb582hm82mtXJWM/3A0/tKm/eQ0NsYU1ew4dPQ0BrG2bFvH7MNjf7GhnsDr/BxNW0Mk29ofTQ7M/P8+Z9xqvQ/bel3tu60HUu/D3ZPk7YxS0y+1syj+cXFudnZmafNjEZKZMSW3CSzOqjIM2E3THjs9r0ktLZEJvJ54c2VG4vz83NzTxmNrwMoQl8/imxiDBZVn0xqba+GGKwJ3wxut9tmQ7+bG0t322SP203WxmOhSuTtE5R2+fqvW8gUX9sGLRzP04BJRLO5stoi8XznQz4pdrSX+K77laDaEhPFJr5cEcXWh3wp1tE+wq+A27Cl/1vkgd+P2vaRoDgMluXoY5x26dqPhYUprqbaBjmsKCtCKgG7xrhyhu3rVJTxPOfrDClcWBA+jHK+lCDki5w8qSixQc7gdDptmsZpdPB7nE6yNhaDMKSlXbz6fYrVVNtq95Exli2JbLXCjiZhMhaPd2RZ3ziAXEEW21JsVGXZfJEJxuPxSJE1rGhDY63tEAkK1vzU0y6c/wo1UlCFWE8BwsEYpCODYhNMJiVJYqDvNoRjOYtQSMFoCULcVpYkCwcNXq9Xb/Nifo/Xu5+E3vbyip52/g7UgMSwBGGxvy/Hw2QPhIXcMA2rooUGk0ttqR7A+1LwfkKqto+D4QJNl0PQ4HA49DYH5vc4HGRtQPPysp4GltGJVvQNKZlHolmZA8Aa7AfAXMhUMknguw1AqE2MyinA5SNdch+Q5KgYKQHD321fUNthEhTQvbikpRloIcCEAzQQwhBdAR0Io6USYAQFrfBjqxBQ8FtKTMVP8VsGl8vVq7WhMZ2V0y7XARIUvezFRZR2lyZhxpf+WFzsQdPfXNhArc0vZyfQibTN8OwCSiNnlgYZoc7+XA2uu9c+8HrpcIQEtTLu7jN6NcJhc731KV2vrfe1Nh4nsIHaaF4vp+o4SGAztWUjZV4fZ+r497INm7f+BiZYjIrB4yPmAAAAAElFTkSuQmCC";

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
                this.CreateSvgBadge(summaryResult, true, false, false));

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
                this.CreateSvgBadge(summaryResult, false, true, false));

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
                this.CreateSvgBadge(summaryResult, false, false, true));

            foreach (var color in ShieldIoColors)
            {
                targetPath = Path.Combine(targetDirectory, $"badge_shieldsio_methodcoverage_{color.Item1}.svg");

                Logger.InfoFormat(Resources.WritingReportFile, targetPath);

                File.WriteAllText(
                    targetPath,
                    this.CreateShieldsIoSvgBadge(summaryResult.CodeElementCoverageQuota, color.Item2));
            }

            targetPath = Path.Combine(targetDirectory, "badge_combined.svg");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            File.WriteAllText(
                targetPath,
                this.CreateSvgBadge(summaryResult, true, true, true));

            targetPath = Path.Combine(targetDirectory, "badge_linecoverage.png");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            File.WriteAllBytes(
                targetPath,
                this.CreatePngBadge(summaryResult.CoverageQuota, LineCoveragePngTemplate));

            targetPath = Path.Combine(targetDirectory, "badge_branchcoverage.png");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            File.WriteAllBytes(
                targetPath,
                this.CreatePngBadge(summaryResult.BranchCoverageQuota, BranchCoveragePngTemplate));

            targetPath = Path.Combine(targetDirectory, "badge_methodcoverage.png");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            File.WriteAllBytes(
                targetPath,
                this.CreatePngBadge(summaryResult.CodeElementCoverageQuota, MethodCoveragePngTemplate));
        }

        /// <summary>
        /// Renderes the SVG.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        /// <param name="includeLineCoverage">Indicates whether line coverage should be included.</param>
        /// <param name="includeBranchCoverage">Indicates whether branch coverage should be included.</param>
        /// <param name="includeMethodCoverage">Indicates whether method coverage should be included.</param>
        /// <returns>The rendered SVG.</returns>
        private string CreateSvgBadge(SummaryResult summaryResult, bool includeLineCoverage, bool includeBranchCoverage, bool includeMethodCoverage)
        {
            string lineCoverageClass = includeLineCoverage && includeBranchCoverage && includeMethodCoverage ? "linecoverage" : string.Empty;
            string branchCoverageClass = includeLineCoverage && includeBranchCoverage && includeMethodCoverage ? "branchcoverage" : string.Empty;
            string methodCoverageClass = includeLineCoverage && includeBranchCoverage && includeMethodCoverage ? "methodcoverage" : string.Empty;

            string lineCoverage = "N/A";
            string branchCoverage = "N/A";
            string methodCoverage = "N/A";

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

            return string.Format(
                Template,
                ReportResources.CodeCoverage,
                includeLineCoverage ? string.Format(LineCoverageSymbol, lineCoverageClass) : string.Empty,
                includeBranchCoverage ? string.Format(BranchCoverageSymbol, branchCoverageClass) : string.Empty,
                includeMethodCoverage ? string.Format(MethodCoverageSymbol, methodCoverageClass) : string.Empty,
                $"{ReportResources.GeneratedBy} ReportGenerator {typeof(IReportBuilder).Assembly.GetName().Version}",
                ReportResources.Coverage3,
                includeLineCoverage ? string.Format(CoverageText, lineCoverageClass, lineCoverage) : string.Empty,
                includeBranchCoverage ? string.Format(CoverageText, branchCoverageClass, branchCoverage) : string.Empty,
                includeMethodCoverage ? string.Format(CoverageText, methodCoverageClass, methodCoverage) : string.Empty,
                includeLineCoverage ? string.Format(CoverageTooltip, lineCoverageClass, ReportResources.Coverage) : string.Empty,
                includeBranchCoverage ? string.Format(CoverageTooltip, branchCoverageClass, ReportResources.BranchCoverage) : string.Empty,
                includeMethodCoverage ? string.Format(CoverageTooltip, methodCoverageClass, ReportResources.CodeElementCoverageQuota) : string.Empty);
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

        /// <summary>
        /// Renderes the PNG.
        /// </summary>
        /// <param name="coverage">The coverage.</param>
        /// <param name="template">The template to use.</param>
        /// <returns>The rendered PNG.</returns>
        private byte[] CreatePngBadge(decimal? coverage, string template)
        {
            string text = "N/A";

            if (coverage.HasValue)
            {
                coverage = Math.Floor(coverage.Value);
                text = $"{coverage.Value.ToString(CultureInfo.InvariantCulture)}%";
            }

            using (var ms = new MemoryStream(Convert.FromBase64String(template)))
            using (var image = Image.Load<Rgba32>(ms))
            using (MemoryStream output = new MemoryStream())
            {
                Font font = null;

                try
                {
                    font = SystemFonts.CreateFont("Arial", 12, FontStyle.Regular);
                }
                catch (FontFamilyNotFoundException)
                {
                    throw new InvalidOperationException(Resources.ErrorFontNotFound);
                }

                image.Mutate(ctx => ctx.DrawText(text, font, Color.White, new PointF(113, 3)));

                image.Save(output, new PngEncoder());
                return output.ToArray();
            }
        }
    }
}