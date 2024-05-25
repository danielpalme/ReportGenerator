using System;
using System.Collections.Generic;
using System.IO;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates MHTML which is a container for the complete HTML report.
    /// </summary>
    public class MhtmlReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(MhtmlReportBuilder));

        /// <summary>
        /// The <see cref="HtmlReportBuilder"/>.
        /// </summary>
        private readonly IReportBuilder htmlReportBuilder = new HtmlReportBuilder(false);

        /// <summary>
        /// The report context.
        /// </summary>
        private IReportContext reportContext;

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
        /// Gets or sets the report context.
        /// </summary>
        /// <value>
        /// The report context.
        /// </value>
        public IReportContext ReportContext
        {
            get
            {
                return this.reportContext;
            }

            set
            {
                this.reportContext = value;
                this.htmlReportTargetDirectory = Path.Combine(value.ReportConfiguration.TargetDirectory, "tmphtml");
                this.htmlReportBuilder.ReportContext = new HtmlReportBuilderReportContext(
                    new HtmlReportBuilderReportConfiguration(value.ReportConfiguration, this.htmlReportTargetDirectory),
                    value.Settings,
                    value.RiskHotspotAnalysisResult,
                    value.OverallHistoricCoverages);
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
            content = content.Replace("<script type=\"text/javascript\" src=\"main.js\"></script>", "<script type=\"text/javascript\" src=\"file:///main.js\"></script>");
            content = content.Replace("<script type=\"text/javascript\" src=\"class.js\"></script>", "<script type=\"text/javascript\" src=\"file:///class.js\"></script>");

            return content;
        }

        /// <summary>
        /// Creates the MHTML file.
        /// </summary>
        private void CreateMhtmlFile()
        {
            string targetDirectory = this.ReportContext.ReportConfiguration.TargetDirectory;
            string htmlReportTargetDirectory = this.htmlReportTargetDirectory;

            if (this.ReportContext.Settings.CreateSubdirectoryForAllReportTypes)
            {
                targetDirectory = Path.Combine(targetDirectory, this.ReportType);
                htmlReportTargetDirectory = Path.Combine(htmlReportTargetDirectory, this.htmlReportBuilder.ReportType);

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

            string targetPath = Path.Combine(targetDirectory, "Summary.mht");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            using (var writer = new StreamWriter(new FileStream(targetPath, FileMode.Create)))
            {
                writer.WriteLine("MIME-Version: 1.0");
                writer.WriteLine("Content-Type: multipart/related;");
                writer.WriteLine("\ttype=\"text/html\";");
                writer.WriteLine("\tboundary=\"----=_NextPart_000_0000_01D23618.54EBCBE0\"");
                writer.WriteLine();

                string file = "index.html";
                string content = File.ReadAllText(Path.Combine(htmlReportTargetDirectory, file));
                content = AddFilePrefixForCssAndJavaScript(content);
                content = content.Replace("<tr><td><a href=\"", "<tr><td><a href=\"file:///");
                WriteFile(writer, file, "text/html", content);

                foreach (var reportFile in Directory.EnumerateFiles(htmlReportTargetDirectory, "*.html"))
                {
                    if (reportFile.EndsWith("index.html"))
                    {
                        continue;
                    }

                    file = reportFile.Substring(reportFile.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                    content = File.ReadAllText(reportFile);
                    content = AddFilePrefixForCssAndJavaScript(content);
                    content = content.Replace("<a href=\"index.html\"", "<a href=\"file:///index.html\"");
                    WriteFile(writer, file, "text/html", content);
                }

                file = "main.js";
                content = File.ReadAllText(Path.Combine(htmlReportTargetDirectory, file));
                content = content.Replace(", \"reportPath\": \"", ", \"reportPath\": \"file:///");
                content = content.Replace(", \"rp\": \"", ", \"rp\": \"file:///");
                WriteFile(writer, file, "application/javascript", content);

                file = "class.js";
                if (File.Exists(Path.Combine(htmlReportTargetDirectory, file)))
                {
                    content = File.ReadAllText(Path.Combine(htmlReportTargetDirectory, file));
                    WriteFile(writer, file, "application/javascript", content);
                }

                file = "report.css";
                content = File.ReadAllText(Path.Combine(htmlReportTargetDirectory, file));
                WriteFile(writer, file, "text/css", content);

                writer.Write("------=_NextPart_000_0000_01D23618.54EBCBE0--");
            }
        }

        /// <summary>
        /// Wraps another <see cref="IReportConfiguration"/> but makes it possible to override the target directory.
        /// </summary>
        private class HtmlReportBuilderReportContext : IReportContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="HtmlReportBuilderReportContext"/> class.
            /// </summary>
            /// <param name="reportConfiguration">The configuration options.</param>
            /// <param name="settings">The settings.</param>
            /// <param name="riskHotspotAnalysisResult">The risk hotspot analysis result.</param>
            /// <param name="overallHistoricCoverages">The historic coverage elements.</param>
            public HtmlReportBuilderReportContext(IReportConfiguration reportConfiguration, Settings settings, RiskHotspotAnalysisResult riskHotspotAnalysisResult, IReadOnlyCollection<HistoricCoverage> overallHistoricCoverages)
            {
                this.ReportConfiguration = reportConfiguration;
                this.Settings = settings;
                this.RiskHotspotAnalysisResult = riskHotspotAnalysisResult;
                this.OverallHistoricCoverages = overallHistoricCoverages;
            }

            /// <summary>
            /// Gets the configuration options.
            /// </summary>
            public IReportConfiguration ReportConfiguration { get; }

            /// <summary>
            /// Gets the settings.
            /// </summary>
            public Settings Settings { get; }

            /// <summary>
            /// Gets or sets the risk hotspot analysis result.
            /// </summary>
            public RiskHotspotAnalysisResult RiskHotspotAnalysisResult { get; set; }

            /// <summary>
            /// Gets the historic coverage elements.
            /// </summary>
            public IReadOnlyCollection<HistoricCoverage> OverallHistoricCoverages { get; }
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
            /// Gets the files containing coverage information.
            /// </summary>
            public IReadOnlyCollection<string> ReportFiles => this.reportConfiguration.ReportFiles;

            /// <summary>
            /// Gets the target directory.
            /// </summary>
            public string TargetDirectory { get; private set; }

            /// <summary>
            /// Gets the source directories.
            /// </summary>
            public IReadOnlyCollection<string> SourceDirectories => this.reportConfiguration.SourceDirectories;

            /// <summary>
            /// Gets the history directory.
            /// </summary>
            public string HistoryDirectory => this.reportConfiguration.TargetDirectory;

            /// <summary>
            /// Gets the type of the report.
            /// </summary>
            public IReadOnlyCollection<string> ReportTypes => this.reportConfiguration.ReportTypes;

            /// <summary>
            /// Gets the plugins.
            /// </summary>
            public IReadOnlyCollection<string> Plugins => this.reportConfiguration.Plugins;

            /// <summary>
            /// Gets the assembly filters.
            /// </summary>
            public IReadOnlyCollection<string> AssemblyFilters => this.reportConfiguration.AssemblyFilters;

            /// <summary>
            /// Gets the class filters.
            /// </summary>
            public IReadOnlyCollection<string> ClassFilters => this.reportConfiguration.ClassFilters;

            /// <summary>
            /// Gets the file filters.
            /// </summary>
            public IReadOnlyCollection<string> FileFilters => this.reportConfiguration.FileFilters;

            /// <summary>
            /// Gets the assembly filters for risk hotspots.
            /// </summary>
            public IReadOnlyCollection<string> RiskHotspotAssemblyFilters => this.reportConfiguration.RiskHotspotAssemblyFilters;

            /// <summary>
            /// Gets the class filters for risk hotspots.
            /// </summary>
            public IReadOnlyCollection<string> RiskHotspotClassFilters => this.reportConfiguration.RiskHotspotClassFilters;

            /// <summary>
            /// Gets the verbosity level.
            /// </summary>
            public VerbosityLevel VerbosityLevel => this.reportConfiguration.VerbosityLevel;

            /// <summary>
            /// Gets the custom title.
            /// </summary>
            public string Title => this.reportConfiguration.Title;

            /// <summary>
            /// Gets the custom tag (e.g. build number).
            /// </summary>
            public string Tag => this.reportConfiguration.Tag;

            /// <summary>
            /// Gets the license.
            /// </summary>
            public string License => this.reportConfiguration.License;

            /// <summary>
            /// Gets the invalid file patters supplied by the user.
            /// </summary>
            public IReadOnlyCollection<string> InvalidReportFilePatterns => this.reportConfiguration.InvalidReportFilePatterns;

            /// <summary>
            /// Gets a value indicating whether the verbosity level was successfully parsed during initialization.
            /// </summary>
            public bool VerbosityLevelValid => this.reportConfiguration.VerbosityLevelValid;
        }
    }
}