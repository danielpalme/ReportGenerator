using System;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates history chart in PNG format.
    /// </summary>
    [Obsolete("PngChart was replaced by SvgChart")]
    public class PngChartReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(PngChartReportBuilder));

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        public string ReportType => "PngChart";

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

            Logger.Warn("Report type 'PngChart' is no longer supported. Please use 'SvgChart' instead.");
        }
    }
}