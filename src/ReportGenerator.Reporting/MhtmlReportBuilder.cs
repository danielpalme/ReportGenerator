using System.Collections.Generic;
using System.IO;
using Palmmedia.ReportGenerator.Logging;
using Palmmedia.ReportGenerator.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Creates MHTML which is a container for the complete HTML report.
    /// </summary>
    [System.ComponentModel.Composition.Export(typeof(IReportBuilder))]
    public class MhtmlReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The <see cref="HtmlReportBuilder"/>.
        /// </summary>
        private readonly IReportBuilder htmlReportBuilder = new HtmlReportBuilder();

        /// <summary>
        /// The report configuration.
        /// </summary>
        private IReportConfiguration reportConfiguration;

        /// <summary>
        /// The temporary HTML report target directory.
        /// </summary>
        private string htmlReportTargetDirectory;

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        public string ReportType => "MHtml";

        /// <summary>
        /// Gets or sets the report configuration.
        /// </summary>
        /// <value>
        /// The report configuration.
        /// </value>
        public IReportConfiguration ReportConfiguration
        {
            get
            {
                return this.reportConfiguration;
            }

            set
            {
                this.reportConfiguration = value;
                this.htmlReportTargetDirectory = Path.Combine(value.TargetDirectory, "tmphtml");
                this.htmlReportBuilder.ReportConfiguration = new HtmlReportBuilderReportConfiguration(value, this.htmlReportTargetDirectory);
            }
        }

        /// <summary>
        /// Creates a class report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        public void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses)
        {
            if (!Directory.Exists(this.htmlReportTargetDirectory))
            {
                Directory.CreateDirectory(this.htmlReportTargetDirectory);
            }

            this.htmlReportBuilder.CreateClassReport(@class, fileAnalyses);
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        public void CreateSummaryReport(SummaryResult summaryResult)
        {
            this.htmlReportBuilder.CreateSummaryReport(summaryResult);

            this.CreateMhtmlFile();

            Directory.Delete(this.htmlReportTargetDirectory, true);
        }

        /// <summary>
        /// Writes a file to the given StreamWriter.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="content">The content.</param>
        private static void WriteFile(StreamWriter writer, string filePath, string contentType, string content)
        {
            writer.WriteLine("------=_NextPart_000_0000_01D23618.54EBCBE0");
            writer.Write("Content-Type: ");
            writer.Write(contentType);
            writer.WriteLine(";");
            writer.WriteLine("\tcharset=\"utf-8\"");
            writer.WriteLine("Content-Transfer-Encoding: 8bit");
            writer.Write("Content-Location: file:///");
            writer.WriteLine(filePath);
            writer.WriteLine();
            writer.WriteLine(content);
            writer.WriteLine();
        }

        /// <summary>
        /// Adds the 'file:///' prefix for CSS and java script links.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The processed content.</returns>
        private static string AddFilePrefixForCssAndJavaScript(string content)
        {
            content = content.Replace("<link rel=\"stylesheet\" type=\"text/css\" href=\"report.css\" />", "<link rel=\"stylesheet\" type=\"text/css\" href=\"file:///report.css\" />");
            content = content.Replace("<script type=\"text/javascript\" src=\"combined.js\"></script>", "<script type=\"text/javascript\" src=\"file:///combined.js\"></script>");

            return content;
        }

        /// <summary>
        /// Creates the MHTML file.
        /// </summary>
        private void CreateMhtmlFile()
        {
            using (var writer = new StreamWriter(new FileStream(Path.Combine(this.ReportConfiguration.TargetDirectory, "Summary.mht"), FileMode.Create)))
            {
                writer.WriteLine("MIME-Version: 1.0");
                writer.WriteLine("Content-Type: multipart/related;");
                writer.WriteLine("\ttype=\"text/html\";");
                writer.WriteLine("\tboundary=\"----=_NextPart_000_0000_01D23618.54EBCBE0\"");
                writer.WriteLine();

                string file = "index.htm";
                string content = File.ReadAllText(Path.Combine(this.htmlReportTargetDirectory, file));
                content = AddFilePrefixForCssAndJavaScript(content);
                content = content.Replace("<tr><td><a href=\"", "<tr><td><a href=\"file:///");
                WriteFile(writer, file, "text/html", content);

                foreach (var reportFile in Directory.EnumerateFiles(this.htmlReportTargetDirectory, "*.htm"))
                {
                    if (reportFile.EndsWith("index.htm"))
                    {
                        continue;
                    }

                    file = reportFile.Substring(reportFile.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                    content = File.ReadAllText(reportFile);
                    content = AddFilePrefixForCssAndJavaScript(content);
                    WriteFile(writer, file, "text/html", content);
                }

                file = "combined.js";
                content = File.ReadAllText(Path.Combine(this.htmlReportTargetDirectory, file));
                content = content.Replace(", \"reportPath\" : \"", ", \"reportPath\" : \"file:///");
                WriteFile(writer, file, "application/javascript", content);

                file = "report.css";
                content = File.ReadAllText(Path.Combine(this.htmlReportTargetDirectory, file));
                WriteFile(writer, file, "text/css", content);

                writer.Write("------=_NextPart_000_0000_01D23618.54EBCBE0--");
            }
        }

        /// <summary>
        /// Wraps another <see cref="IReportConfiguration"/> but makes it possible to override the target directory.
        /// </summary>
        private class HtmlReportBuilderReportConfiguration : IReportConfiguration
        {
            /// <summary>
            /// The wrapped <see cref="IReportConfiguration"/> instance.
            /// </summary>
            private readonly IReportConfiguration reportConfiguration;

            /// <summary>
            /// Initializes a new instance of the <see cref="HtmlReportBuilderReportConfiguration"/> class.
            /// </summary>
            /// <param name="reportConfiguration">The wrapped <see cref="IReportConfiguration"/> instance.</param>
            /// <param name="targetDirectory">The custom target directory.</param>
            public HtmlReportBuilderReportConfiguration(IReportConfiguration reportConfiguration, string targetDirectory)
            {
                this.reportConfiguration = reportConfiguration;
                this.TargetDirectory = targetDirectory;
            }

            /// <summary>
            /// Gets the target directory.
            /// </summary>
            public string TargetDirectory { get; private set; }

            /// <summary>
            /// Gets the history directory.
            /// </summary>
            public string HistoryDirectory => this.reportConfiguration.TargetDirectory;

            /// <summary>
            /// Gets the type of the report.
            /// </summary>
            public IEnumerable<string> ReportTypes => this.reportConfiguration.ReportTypes;

            /// <summary>
            /// Gets the source directories.
            /// </summary>
            public IEnumerable<string> SourceDirectories => this.reportConfiguration.SourceDirectories;

            /// <summary>
            /// Gets the assembly filters.
            /// </summary>
            public IEnumerable<string> AssemblyFilters => this.reportConfiguration.AssemblyFilters;

            /// <summary>
            /// Gets the class filters.
            /// </summary>
            public IEnumerable<string> ClassFilters => this.reportConfiguration.ClassFilters;

            /// <summary>
            /// Gets the file filters.
            /// </summary>
            public IEnumerable<string> FileFilters => this.reportConfiguration.FileFilters;

            /// <summary>
            /// Gets the verbosity level.
            /// </summary>
            public VerbosityLevel VerbosityLevel => this.reportConfiguration.VerbosityLevel;

            public IEnumerable<HistoricCoverage> OverallHistoricCoverages => this.reportConfiguration.OverallHistoricCoverages;

            /// <summary>
            /// Gets the custom tag (e.g. build number).
            /// </summary>
            public string Tag => this.reportConfiguration.Tag;
        }
    }
}