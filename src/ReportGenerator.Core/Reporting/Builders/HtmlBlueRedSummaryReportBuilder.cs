using System.Collections.Generic;
using System.IO;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates summary report in HTML format (no reports for classes are generated).
    /// </summary>
    public class HtmlBlueRedSummaryReportBuilder : HtmlReportBuilderBase
    {
        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report format.
        /// </value>
        public override string ReportType => "Html_BlueRed_Summary";

        /// <summary>
        /// Creates a class report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        public override void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses)
        {
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        public override void CreateSummaryReport(SummaryResult summaryResult)
        {
            using (var renderer = new HtmlRenderer(new Dictionary<string, string>(), true, HtmlMode.InlineCssAndJavaScript, new string[] { "custom_adaptive.css", "custom_bluered.css" }, "custom.css"))
            {
                this.CreateSummaryReport(renderer, summaryResult);
            }

            string targetDirectory = this.CreateTargetDirectory();

            string sourcePath = Path.Combine(targetDirectory, "summary.html");

            if (File.Exists(sourcePath))
            {
                File.Copy(
                    sourcePath,
                    Path.Combine(targetDirectory, "summary.htm"),
                    true);

                string targetFile = Path.Combine(targetDirectory, "index.html");

                if (!File.Exists(targetFile))
                {
                    File.Copy(
                        sourcePath,
                        targetFile,
                        true);
                }
            }
        }
    }
}
