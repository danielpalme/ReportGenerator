using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;
using Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates report in XML format.
    /// </summary>
    public class XmlReportBuilder : XmlSummaryReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(XmlReportBuilder));

        /// <summary>
        /// Dictionary containing the filenames of the class reports by class.
        /// </summary>
        private static readonly Dictionary<string, string> FileNameByClass = new Dictionary<string, string>();

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report format.
        /// </value>
        public override string ReportType => "Xml";

        /// <summary>
        /// Creates a class report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        public override void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses)
        {
            var rootElement = new XElement("CoverageReport", new XAttribute("scope", @class.Name));
            var summaryElement = new XElement("Summary");

            summaryElement.Add(new XElement("Class", @class.Name));
            summaryElement.Add(new XElement("Assembly", @class.Assembly.ShortName));

            var filesElement = new XElement("Files");

            foreach (var file in fileAnalyses)
            {
                filesElement.Add(new XElement("File", file.Path));
            }

            summaryElement.Add(filesElement);

            summaryElement.Add(new XElement("Coveredlines", @class.CoveredLines.ToString(CultureInfo.InvariantCulture)));
            summaryElement.Add(new XElement("Uncoveredlines", (@class.CoverableLines - @class.CoveredLines).ToString(CultureInfo.InvariantCulture)));
            summaryElement.Add(new XElement("Coverablelines", @class.CoverableLines.ToString(CultureInfo.InvariantCulture)));
            summaryElement.Add(new XElement("Totallines", @class.TotalLines.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)));
            summaryElement.Add(new XElement("Linecoverage", @class.CoverageQuota.HasValue ? @class.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));

            if (@class.CoveredBranches.HasValue && @class.TotalBranches.HasValue)
            {
                summaryElement.Add(new XElement("Coveredbranches", @class.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)));
                summaryElement.Add(new XElement("Totalbranches", @class.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)));

                if (@class.BranchCoverageQuota.HasValue)
                {
                    summaryElement.Add(new XElement("Branchcoverage", @class.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)));
                }
            }

            summaryElement.Add(new XElement("Coveredmethods", @class.CoveredCodeElements.ToString(CultureInfo.InvariantCulture)));
            summaryElement.Add(new XElement("Fullcoveredmethods", @class.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture)));
            summaryElement.Add(new XElement("Totalmethods", @class.TotalCodeElements.ToString(CultureInfo.InvariantCulture)));
            summaryElement.Add(new XElement("Methodcoverage", @class.CodeElementCoverageQuota.HasValue ? @class.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
            summaryElement.Add(new XElement("Fullmethodcoverage", @class.FullCodeElementCoverageQuota.HasValue ? @class.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));

            if (this.ReportContext.ReportConfiguration.Title != null)
            {
                summaryElement.Add(new XElement("Title", this.ReportContext.ReportConfiguration.Title));
            }

            if (this.ReportContext.ReportConfiguration.Tag != null)
            {
                summaryElement.Add(new XElement("Tag", this.ReportContext.ReportConfiguration.Tag));
            }

            rootElement.Add(summaryElement);

            if (@class.Files.Any(f => f.MethodMetrics.Any()))
            {
                var metricsElement = new XElement("Metrics");

                foreach (var fileAnalysis in @class.Files)
                {
                    foreach (var metric in fileAnalysis.MethodMetrics.OrderBy(c => c.Line))
                    {
                        var metricElement = new XElement("Element", new XAttribute("name", StringHelper.ReplaceNonLetterChars(metric.ShortName)));

                        foreach (var m in metric.Metrics)
                        {
                            var element = new XElement(StringHelper.ReplaceNonLetterChars(m.Name));

                            if (m.Value.HasValue)
                            {
                                element.Value = m.Value.Value.ToString(CultureInfo.InvariantCulture);
                            }

                            metricElement.Add(element);
                        }

                        metricsElement.Add(metricElement);
                    }
                }

                rootElement.Add(metricsElement);
            }

            filesElement = new XElement("Files");
            foreach (var fileAnalysis in fileAnalyses)
            {
                var fileElement = new XElement("File", new XAttribute("name", fileAnalysis.Path));

                foreach (var line in fileAnalysis.Lines)
                {
                    var lineElement = new XElement("LineAnalysis");

                    lineElement.Add(new XAttribute("line", line.LineNumber.ToString(CultureInfo.InvariantCulture)));
                    lineElement.Add(new XAttribute("visits", line.LineVisits.ToString(CultureInfo.InvariantCulture)));
                    lineElement.Add(new XAttribute("coverage", line.LineVisitStatus.ToString()));
                    lineElement.Add(new XAttribute("coveredbranches", line.CoveredBranches.HasValue ? line.CoveredBranches.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
                    lineElement.Add(new XAttribute("totalbranches", line.TotalBranches.HasValue ? line.TotalBranches.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
                    lineElement.Add(new XAttribute("content", StringHelper.ReplaceInvalidXmlChars(line.LineContent)));

                    fileElement.Add(lineElement);
                }

                filesElement.Add(fileElement);
            }

            rootElement.Add(filesElement);

            XDocument result = new XDocument(new XDeclaration("1.0", "utf-8", null), rootElement);

            string targetPath = Path.Combine(
                this.CreateTargetDirectory(),
                GetClassReportFilename(@class.Assembly.Name, @class.Name));

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Encoding = new UTF8Encoding(false),
                Indent = true
            };

            using (XmlWriter writer = XmlWriter.Create(targetPath, settings))
            {
                result.Save(writer);
            }
        }

        /// <summary>
        /// Gets the file name of the report file for the given class.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns>The file name.</returns>
        private static string GetClassReportFilename(string assemblyName, string className)
        {
            string key = assemblyName + "_" + className;

            string fileName = null;

            if (!FileNameByClass.TryGetValue(key, out fileName))
            {
                string shortClassName = null;

                if (assemblyName == "Default" && className.Contains(Path.DirectorySeparatorChar))
                {
                    assemblyName = className.Substring(0, className.LastIndexOf(Path.DirectorySeparatorChar));
                    shortClassName = className.Substring(className.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                }
                else
                {
                    if (className.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                    {
                        shortClassName = className.Substring(0, className.LastIndexOf('.'));
                    }
                    else
                    {
                        shortClassName = className.Substring(className.LastIndexOf('.') + 1);
                    }
                }

                fileName = StringHelper.ReplaceInvalidPathChars(assemblyName + "_" + shortClassName) + ".xml";

                if (fileName.Length > 100)
                {
                    string firstPart = fileName.Substring(0, 50);
                    string lastPart = fileName.Substring(fileName.Length - 45, 45);

                    fileName = firstPart + lastPart;
                }

                if (FileNameByClass.Values.Any(v => v.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    int counter = 2;
                    string fileNameWithoutExtension = fileName.Substring(0, fileName.Length - 4);

                    do
                    {
                        fileName = fileNameWithoutExtension + counter + ".xml";
                        counter++;
                    }
                    while (FileNameByClass.Values.Any(v => v.Equals(fileName, StringComparison.OrdinalIgnoreCase)));
                }

                FileNameByClass.Add(key, fileName);
            }

            return fileName;
        }
    }
}
