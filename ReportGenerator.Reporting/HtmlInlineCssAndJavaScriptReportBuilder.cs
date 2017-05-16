using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using Palmmedia.ReportGenerator.Parser.Analysis;
using Palmmedia.ReportGenerator.Reporting.Rendering;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Creates report in HTML format.
    /// </summary>
    [Export(typeof(IReportBuilder))]
    public class HtmlInlineCssAndJavaScriptReportBuilder : ReportBuilderBase
    {
        /// <summary>
        /// Contains report specific JavaScript content.
        /// </summary>
        private readonly StringBuilder javaScriptContent = new StringBuilder();

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report format.
        /// </value>
        public override string ReportType => "HtmlInline";

        /// <summary>
        /// Creates a class report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        public override void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses)
        {
            using (var renderer = new HtmlRenderer(false, true, this.javaScriptContent))
            {
                this.CreateClassReport(renderer, @class, fileAnalyses);
            }
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        public override void CreateSummaryReport(SummaryResult summaryResult)
        {
            using (var renderer = new HtmlRenderer(false, true, this.javaScriptContent))
            {
                this.CreateSummaryReport(renderer, summaryResult);
            }
        }
    }
}
