using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates xml report in SonarQube 'Generic Test Data' format.
    /// </summary>
    public class SonarQubeBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(SonarQubeBuilder));

        /// <summary>
        /// File elements by path.
        /// </summary>
        private readonly Dictionary<string, XElement> fileElementsByName = new Dictionary<string, XElement>();

        /// <summary>
        /// The resulting XDocument.
        /// </summary>
        private readonly XDocument document;

        /// <summary>
        /// Initializes a new instance of the <see cref="SonarQubeBuilder"/> class.
        /// </summary>
        public SonarQubeBuilder()
        {
            this.document = new XDocument(new XElement("coverage", new XAttribute("version", "1")));
        }

        /// <summary>
        /// Gets the type of the report.
        /// </summary>
        /// <value>
        /// The type of the report.
        /// </value>
        public string ReportType => "SonarQube";

        /// <summary>
        /// Gets or sets the report context.
        /// </summary>
        /// <value>
        /// The report context.
        /// </value>
        public IReportContext ReportContext { get; set; }

        /// <summary>
        /// Creates a class report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        public void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses)
        {
            foreach (var fileAnalysis in fileAnalyses)
            {
                XElement fileElement = null;

                if (!this.fileElementsByName.TryGetValue(fileAnalysis.Path, out fileElement))
                {
                    fileElement = new XElement("file", new XAttribute("path", fileAnalysis.Path));
                    this.document.Root.Add(fileElement);

                    this.fileElementsByName.Add(fileAnalysis.Path, fileElement);

                    AddLineElements(fileElement, fileAnalysis.Lines, false);
                }
                else
                {
                    AddLineElements(fileElement, fileAnalysis.Lines, true);
                }
            }
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        public void CreateSummaryReport(SummaryResult summaryResult)
        {
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

            string targetPath = Path.Combine(targetDirectory, "SonarQube.xml");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Encoding = new UTF8Encoding(false),
                Indent = true
            };

            using (XmlWriter writer = XmlWriter.Create(targetPath, settings))
            {
                this.document.Save(writer);
            }
        }

        /// <summary>
        /// Adds the lines to the given parent element.
        /// </summary>
        /// <param name="parent">The parent element.</param>
        /// <param name="lines">The lines to add.</param>
        /// <param name="existingFile">Indicated wheather the parent element already existed or not.</param>
        private static void AddLineElements(XElement parent, IEnumerable<LineAnalysis> lines, bool existingFile)
        {
            foreach (var line in lines)
            {
                if (line.LineVisitStatus == LineVisitStatus.NotCoverable)
                {
                    continue;
                }

                string lineNumber = line.LineNumber.ToString(CultureInfo.InvariantCulture);

                if (existingFile)
                {
                    var existingLineElement = parent.Elements("lineToCover").FirstOrDefault(l => l.Attribute("lineNumber").Value == lineNumber);

                    if (existingLineElement != null)
                    {
                        // Update existing line element
                        if (line.LineVisitStatus != LineVisitStatus.NotCovered)
                        {
                            existingLineElement.Attribute("covered").Value = "true";
                        }

                        if (line.TotalBranches.GetValueOrDefault() > 0)
                        {
                            if (existingLineElement.Attribute("branchesToCover") == null)
                            {
                                existingLineElement.Add(new XAttribute("branchesToCover", line.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)));
                                existingLineElement.Add(new XAttribute("coveredBranches", line.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)));
                            }
                            else
                            {
                                int branchesToCover = Math.Max(int.Parse(existingLineElement.Attribute("branchesToCover").Value), line.TotalBranches.GetValueOrDefault());
                                int coveredBranches = Math.Max(int.Parse(existingLineElement.Attribute("coveredBranches").Value), line.CoveredBranches.GetValueOrDefault());

                                existingLineElement.Attribute("branchesToCover").Value = branchesToCover.ToString(CultureInfo.InvariantCulture);
                                existingLineElement.Attribute("coveredBranches").Value = coveredBranches.ToString(CultureInfo.InvariantCulture);
                            }
                        }

                        continue;
                    }
                }

                // Create new line element
                var lineElement = new XElement(
                    "lineToCover",
                    new XAttribute("lineNumber", lineNumber),
                    new XAttribute("covered", line.LineVisitStatus == LineVisitStatus.NotCovered ? "false" : "true"));

                if (line.TotalBranches.GetValueOrDefault() > 0)
                {
                    lineElement.Add(new XAttribute("branchesToCover", line.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)));
                    lineElement.Add(new XAttribute("coveredBranches", line.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)));
                }

                parent.Add(lineElement);
            }
        }
    }
}
