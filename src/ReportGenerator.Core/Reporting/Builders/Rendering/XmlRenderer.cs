using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering
{
    /// <summary>
    /// XML report renderer.
    /// </summary>
    internal class XmlRenderer : RendererBase, IReportRenderer
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(XmlRenderer));

        /// <summary>
        /// Indicates that the current node representing an assembly has to be closed before adding further elements.
        /// </summary>
        private bool closeAssemblyNode;

        /// <summary>
        /// The report builder.
        /// </summary>
        private XmlWriter reportTextWriter;

        /// <summary>
        /// Gets a value indicating whether renderer support rendering of charts.
        /// </summary>
        public bool SupportsCharts => false;

        /// <summary>
        /// Gets a value indicating whether renderer support rendering risk hotspots.
        /// </summary>
        public bool SupportsRiskHotsSpots => false;

        /// <summary>
        /// Begins the summary report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="title">The title.</param>
        public void BeginSummaryReport(string targetDirectory, string fileName, string title)
        {
            string targetPath = Path.Combine(targetDirectory, "Summary.xml");

            Logger.InfoFormat("  " + Resources.WritingReportFile, targetPath);
            this.CreateXmlWriter(targetPath);

            this.reportTextWriter.WriteStartElement("CoverageReport");
            this.reportTextWriter.WriteAttributeString("scope", title);
        }

        /// <summary>
        /// Adds custom summary elements to the report.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="riskHotspots">The risk hotspots.</param>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        public void CustomSummary(IEnumerable<Assembly> assemblies, IEnumerable<RiskHotspot> riskHotspots, bool branchCoverageAvailable)
        {
        }

        /// <summary>
        /// Begins the class report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        public void BeginClassReport(string targetDirectory, string assemblyName, string className)
        {
            string targetPath = Path.Combine(targetDirectory, ReplaceInvalidPathChars(assemblyName + "_" + className) + ".xml");

            Logger.DebugFormat("  " + Resources.WritingReportFile, targetPath);
            this.CreateXmlWriter(targetPath);

            this.reportTextWriter.WriteStartElement("CoverageReport");
            this.reportTextWriter.WriteAttributeString("scope", className);
        }

        /// <summary>
        /// Adds a header to the report.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Header(string text)
        {
            this.reportTextWriter.WriteStartElement(XmlRenderer.ReplaceNonLetterChars(text));
        }

        /// <summary>
        /// Adds the test methods to the report.
        /// </summary>
        /// <param name="testMethods">The test methods.</param>
        /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
        /// <param name="codeElementsByFileIndex">Code elements by file index.</param>
        public void TestMethods(IEnumerable<TestMethod> testMethods, IEnumerable<FileAnalysis> fileAnalyses, IDictionary<int, IEnumerable<CodeElement>> codeElementsByFileIndex)
        {
        }

        /// <summary>
        /// Adds a file of a class to a report.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        public void File(string path)
        {
            this.reportTextWriter.WriteStartElement("File");
            this.reportTextWriter.WriteAttributeString("name", path);
        }

        /// <summary>
        /// Adds a paragraph to the report.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Paragraph(string text)
        {
        }

        /// <summary>
        /// Adds a table with two columns to the report.
        /// </summary>
        public void BeginKeyValueTable()
        {
        }

        /// <summary>
        /// Start of risk summary table section.
        /// </summary>
        public void BeginSummaryTable()
        {
        }

        /// <summary>
        /// End of risk summary table section.
        /// </summary>
        public void FinishSummaryTable()
        {
        }

        /// <summary>
        /// Adds a summary table to the report.
        /// </summary>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        public void BeginSummaryTable(bool branchCoverageAvailable)
        {
        }

        /// <summary>
        /// Adds a file analysis table to the report.
        /// </summary>
        /// <param name="headers">The headers.</param>
        public void BeginLineAnalysisTable(IEnumerable<string> headers)
        {
        }

        /// <summary>
        /// Adds a table row with two cells to the report.
        /// </summary>
        /// <param name="key">The text of the first column.</param>
        /// <param name="value">The text of the second column.</param>
        public void KeyValueRow(string key, string value)
        {
            this.reportTextWriter.WriteStartElement(XmlRenderer.ReplaceNonLetterChars(key));
            this.reportTextWriter.WriteValue(value);
            this.reportTextWriter.WriteEndElement();
        }

        /// <summary>
        /// Adds a table row with two cells to the report.
        /// </summary>
        /// <param name="key">The text of the first column.</param>
        /// <param name="files">The files.</param>
        public void KeyValueRow(string key, IEnumerable<string> files)
        {
            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            this.reportTextWriter.WriteStartElement(XmlRenderer.ReplaceNonLetterChars(key));

            foreach (var file in files)
            {
                this.reportTextWriter.WriteStartElement("File");
                this.reportTextWriter.WriteValue(file);
                this.reportTextWriter.WriteEndElement();
            }

            this.reportTextWriter.WriteEndElement();
        }

        /// <summary>
        /// Adds metrics to the report
        /// </summary>
        /// <param name="class">The class.</param>
        public void MetricsTable(Class @class)
        {
            if (@class == null)
            {
                throw new ArgumentNullException(nameof(@class));
            }

            var methodMetrics = @class.Files.SelectMany(f => f.MethodMetrics);

            this.MetricsTable(methodMetrics);
        }

        /// <summary>
        /// Adds metrics to the report
        /// </summary>
        /// <param name="methodMetrics">The method metrics.</param>
        public void MetricsTable(IEnumerable<MethodMetric> methodMetrics)
        {
            if (methodMetrics == null)
            {
                throw new ArgumentNullException(nameof(methodMetrics));
            }

            foreach (var methodMetric in methodMetrics.OrderBy(c => c.Line))
            {
                this.reportTextWriter.WriteStartElement("Element");
                this.reportTextWriter.WriteAttributeString("name", XmlRenderer.ReplaceNonLetterChars(methodMetric.ShortName));

                foreach (var m in methodMetric.Metrics)
                {
                    this.reportTextWriter.WriteStartElement(XmlRenderer.ReplaceNonLetterChars(m.Name));
                    if (m.Value.HasValue)
                    {
                        this.reportTextWriter.WriteValue(m.Value.Value.ToString(CultureInfo.InvariantCulture));
                    }

                    this.reportTextWriter.WriteEndElement();
                }

                this.reportTextWriter.WriteEndElement();
            }

            this.reportTextWriter.WriteEndElement();
        }

        /// <summary>
        /// Adds the coverage information of a single line of a file to the report.
        /// </summary>
        /// <param name="fileIndex">The index of the file.</param>
        /// <param name="analysis">The line analysis.</param>
        public void LineAnalysis(int fileIndex, LineAnalysis analysis)
        {
            if (analysis == null)
            {
                throw new ArgumentNullException(nameof(analysis));
            }

            this.reportTextWriter.WriteStartElement("LineAnalysis");
            this.reportTextWriter.WriteAttributeString("line", analysis.LineNumber.ToString(CultureInfo.InvariantCulture));
            this.reportTextWriter.WriteAttributeString("visits", analysis.LineVisits.ToString(CultureInfo.InvariantCulture));
            this.reportTextWriter.WriteAttributeString("coverage", analysis.LineVisitStatus.ToString());
            this.reportTextWriter.WriteAttributeString("coveredbranches", analysis.CoveredBranches.HasValue ? analysis.CoveredBranches.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            this.reportTextWriter.WriteAttributeString("totalbranches", analysis.TotalBranches.HasValue ? analysis.TotalBranches.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);

            this.reportTextWriter.WriteAttributeString("content", XmlRenderer.ReplaceInvalidXmlChars(analysis.LineContent));
            this.reportTextWriter.WriteEndElement();
        }

        /// <summary>
        /// Finishes the current table.
        /// </summary>
        public void FinishTable()
        {
            if (this.closeAssemblyNode)
            {
                this.reportTextWriter.WriteEndElement();
                this.closeAssemblyNode = false;
            }

            this.reportTextWriter.WriteEndElement();
        }

        /// <summary>
        /// Renderes a chart with the given historic coverages.
        /// </summary>
        /// <param name="historicCoverages">The historic coverages.</param>
        /// <param name="renderPngFallBackImage">Indicates whether PNG images are rendered as a fallback</param>
        public void Chart(IEnumerable<HistoricCoverage> historicCoverages, bool renderPngFallBackImage)
        {
        }

        /// <summary>
        /// Start of risk hotspots section.
        /// </summary>
        public void BeginRiskHotspots()
        {
        }

        /// <summary>
        /// End of risk hotspots section.
        /// </summary>
        public void FinishRiskHotspots()
        {
        }

        /// <summary>
        /// Summary of risk hotspots.
        /// </summary>
        /// <param name="riskHotspots">The risk hotspots.</param>
        public void RiskHotspots(IEnumerable<RiskHotspot> riskHotspots)
        {
        }

        /// <summary>
        /// Adds the coverage information of an assembly to the report.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        public void SummaryAssembly(Assembly assembly, bool branchCoverageAvailable)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (this.closeAssemblyNode)
            {
                this.reportTextWriter.WriteEndElement();
            }

            this.reportTextWriter.WriteStartElement("Assembly");
            this.reportTextWriter.WriteAttributeString("name", assembly.Name);
            this.reportTextWriter.WriteAttributeString("classes", assembly.Classes.Count().ToString(CultureInfo.InvariantCulture));
            this.reportTextWriter.WriteAttributeString("coverage", assembly.CoverageQuota.HasValue ? assembly.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            this.reportTextWriter.WriteAttributeString("coveredlines", assembly.CoveredLines.ToString(CultureInfo.InvariantCulture));
            this.reportTextWriter.WriteAttributeString("coverablelines", assembly.CoverableLines.ToString(CultureInfo.InvariantCulture));
            this.reportTextWriter.WriteAttributeString("totallines", assembly.TotalLines.HasValue ? assembly.TotalLines.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            this.reportTextWriter.WriteAttributeString("branchcoverage", assembly.BranchCoverageQuota.HasValue ? assembly.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            this.reportTextWriter.WriteAttributeString("coveredbranches", assembly.CoveredBranches.HasValue ? assembly.CoveredBranches.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            this.reportTextWriter.WriteAttributeString("totalbranches", assembly.TotalBranches.HasValue ? assembly.TotalBranches.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);

            this.closeAssemblyNode = true;
        }

        /// <summary>
        /// Adds the coverage information of a class to the report.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="branchCoverageAvailable">if set to <c>true</c> branch coverage is available.</param>
        public void SummaryClass(Class @class, bool branchCoverageAvailable)
        {
            if (@class == null)
            {
                throw new ArgumentNullException(nameof(@class));
            }

            this.reportTextWriter.WriteStartElement("Class");
            this.reportTextWriter.WriteAttributeString("name", @class.Name);
            this.reportTextWriter.WriteAttributeString("coverage", @class.CoverageQuota.HasValue ? @class.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            this.reportTextWriter.WriteAttributeString("coveredlines", @class.CoveredLines.ToString(CultureInfo.InvariantCulture));
            this.reportTextWriter.WriteAttributeString("coverablelines", @class.CoverableLines.ToString(CultureInfo.InvariantCulture));
            this.reportTextWriter.WriteAttributeString("totallines", @class.TotalLines.HasValue ? @class.TotalLines.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            this.reportTextWriter.WriteAttributeString("branchcoverage", @class.BranchCoverageQuota.HasValue ? @class.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            this.reportTextWriter.WriteAttributeString("coveredbranches", @class.CoveredBranches.HasValue ? @class.CoveredBranches.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            this.reportTextWriter.WriteAttributeString("totalbranches", @class.TotalBranches.HasValue ? @class.TotalBranches.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            this.reportTextWriter.WriteEndElement();
        }

        /// <summary>
        /// Adds the footer to the report.
        /// </summary>
        public void AddFooter()
        {
        }

        /// <summary>
        /// Saves a summary report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        public void SaveSummaryReport(string targetDirectory)
        {
            this.SaveReport();
        }

        /// <summary>
        /// Saves a class report.
        /// </summary>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        public void SaveClassReport(string targetDirectory, string assemblyName, string className)
        {
            this.SaveReport();
        }

        /// <summary>
        /// Initializes the xml writer.
        /// </summary>
        /// <param name="targetPath">The target path.</param>
        private void CreateXmlWriter(string targetPath)
        {
            var xmlWriterSettings = new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                OmitXmlDeclaration = false,
                CloseOutput = true
            };

            this.reportTextWriter = XmlWriter.Create(new FileStream(targetPath, FileMode.Create), xmlWriterSettings);
        }

        /// <summary>
        /// Saves the report.
        /// </summary>
        private void SaveReport()
        {
            this.reportTextWriter.Flush();
            this.reportTextWriter.Close();

            this.reportTextWriter = null;
        }
    }
}
