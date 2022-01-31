using System.Collections.Generic;
using System.IO;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates report in HTML format.
    /// </summary>
    public class HtmlInlineAzurePipelinesDarkReportBuilder : HtmlReportBuilderBase
    {
        /// <summary>
        /// Dictionary containing the filenames of the class reports by class.
        /// </summary>
        private readonly IDictionary<string, string> fileNameByClass = new Dictionary<string, string>();

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report format.
        /// </value>
        public override string ReportType => "HtmlInline_AzurePipelines_Dark";

        /// <summary>
        /// Creates a class report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        public override void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses)
        {
            using (var renderer = new HtmlRenderer(this.fileNameByClass, false, HtmlMode.InlineCssAndJavaScript, "custom-azurepipelines.css", "custom-azurepipelines_dark.css"))
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
            using (var renderer = new HtmlRenderer(this.fileNameByClass, false, HtmlMode.InlineCssAndJavaScript, "custom-azurepipelines.css", "custom-azurepipelines_dark.css"))
            {
                this.CreateSummaryReport(renderer, summaryResult);
            }

            string targetDirectory = this.CreateTargetDirectory();

            File.Copy(
                Path.Combine(targetDirectory, "index.html"),
                Path.Combine(targetDirectory, "index.htm"),
                true);
        }
    }
}