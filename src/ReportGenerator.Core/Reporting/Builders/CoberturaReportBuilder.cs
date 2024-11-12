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
    /// Creates xml report in Cobertura format.
    /// </summary>
    public class CoberturaReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(CoberturaReportBuilder));

        /// <summary>
        /// Package elements by assembly name.
        /// </summary>
        private readonly Dictionary<string, XElement> packageElementsByName = new Dictionary<string, XElement>();

        /// <summary>
        /// Gets the type of the report.
        /// </summary>
        /// <value>
        /// The type of the report.
        /// </value>
        public string ReportType => "Cobertura";

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
            if (!this.packageElementsByName.TryGetValue(@class.Assembly.Name, out XElement packageElement))
            {
                packageElement = new XElement("package", new XAttribute("name", @class.Assembly.Name));
                packageElement.Add(new XElement("classes"));

                this.packageElementsByName.Add(@class.Assembly.Name, packageElement);
            }

            var classesElement = packageElement.Element("classes");

            foreach (var fileAnalysis in fileAnalyses)
            {
                decimal? fileComplexity = null;
                var classElement = new XElement(
                    "class",
                    new XAttribute("name", @class.Name),
                    new XAttribute("filename", fileAnalysis.Path));

                var methodsElement = new XElement("methods");

                foreach (var file in @class.Files)
                {
                    if (file.Path == fileAnalysis.Path)
                    {
                        foreach (var codeElement in file.CodeElements)
                        {
                            int index = codeElement.Name.LastIndexOf('(');

                            var methodLinesElement = new XElement("lines");

                            var methodElement = new XElement(
                                "method",
                                new XAttribute("name", index == -1 ? codeElement.Name : codeElement.Name.Substring(0, index)),
                                new XAttribute("signature", index == -1 ? string.Empty : codeElement.Name.Substring(index)),
                                methodLinesElement);

                            this.AddLineElements(
                                methodLinesElement,
                                fileAnalysis.Lines.Skip(codeElement.FirstLine - 1).Take(codeElement.LastLine - codeElement.FirstLine + 1),
                                out double methodLineRate,
                                out double methodBranchRate);

                            methodElement.Add(new XAttribute("line-rate", methodLineRate.ToString(CultureInfo.InvariantCulture)));
                            methodElement.Add(new XAttribute("branch-rate", methodBranchRate.ToString(CultureInfo.InvariantCulture)));

                            var methodMetrics = file.MethodMetrics
                                .FirstOrDefault(q => q.FullName == codeElement.FullName
                                    && q.Line == codeElement.FirstLine);

                            if (methodMetrics != null)
                            {
                                var complexityMetric = methodMetrics.Metrics.FirstOrDefault(m => m.Name == ReportResources.CyclomaticComplexity);
                                if (complexityMetric != null && complexityMetric.Value.HasValue)
                                {
                                    if (!fileComplexity.HasValue)
                                    {
                                        fileComplexity = 0;
                                    }

                                    fileComplexity += complexityMetric.Value.Value;
                                    methodElement.Add(new XAttribute("complexity", complexityMetric.Value.Value.ToString(CultureInfo.InvariantCulture)));
                                }
                            }
                            else
                            {
                                methodElement.Add(new XAttribute("complexity", "NaN"));
                            }

                            methodsElement.Add(methodElement);
                        }

                        break;
                    }
                }

                classElement.Add(methodsElement);

                var linesElement = new XElement("lines");

                this.AddLineElements(linesElement, fileAnalysis.Lines, out double lineRate, out double branchRate);

                classElement.Add(new XAttribute("line-rate", lineRate.ToString(CultureInfo.InvariantCulture)));
                classElement.Add(new XAttribute("branch-rate", branchRate.ToString(CultureInfo.InvariantCulture)));
                classElement.Add(new XAttribute("complexity", fileComplexity.HasValue ? fileComplexity.Value.ToString(CultureInfo.InvariantCulture) : "NaN"));

                classElement.Add(linesElement);

                classesElement.Add(classElement);
            }
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        public void CreateSummaryReport(SummaryResult summaryResult)
        {
            decimal? summaryComplexity = null;

            var assembliesWithClasses = summaryResult.Assemblies
                .Where(a => a.Classes.Any())
                .ToArray();

            foreach (var assembly in assembliesWithClasses)
            {
                decimal? assemblyComplexity = null;
                if (this.packageElementsByName.TryGetValue(assembly.Name, out XElement packageElement))
                {
                    double packageLineRate = assembly.CoverableLines == 0 ? 1 : assembly.CoveredLines / (double)assembly.CoverableLines;
                    double packageBranchRate = assembly.TotalBranches.GetValueOrDefault() == 0 ? 1 : assembly.CoveredBranches.GetValueOrDefault() / (double)assembly.TotalBranches.GetValueOrDefault();

                    packageElement.Add(new XAttribute("line-rate", packageLineRate.ToString(CultureInfo.InvariantCulture)));
                    packageElement.Add(new XAttribute("branch-rate", packageBranchRate.ToString(CultureInfo.InvariantCulture)));

                    foreach (var @class in assembly.Classes)
                    {
                        foreach (var file in @class.Files)
                        {
                            foreach (var methodMetric in file.MethodMetrics)
                            {
                                var metric = methodMetric.Metrics.FirstOrDefault(m =>
                                    m.Name == ReportResources.CyclomaticComplexity
                                    && m.Value.HasValue);

                                if (metric != null)
                                {
                                    if (!assemblyComplexity.HasValue)
                                    {
                                        assemblyComplexity = 0;
                                    }

                                    if (!summaryComplexity.HasValue)
                                    {
                                        summaryComplexity = 0;
                                    }

                                    assemblyComplexity += metric.Value.Value;
                                    summaryComplexity += metric.Value.Value;
                                }
                            }
                        }
                    }

                    packageElement.Add(new XAttribute("complexity", assemblyComplexity.HasValue ? assemblyComplexity.Value.ToString(CultureInfo.InvariantCulture) : "NaN"));
                }
            }

            double lineRate = summaryResult.CoverableLines == 0 ? 1 : summaryResult.CoveredLines / (double)summaryResult.CoverableLines;
            double branchRate = summaryResult.TotalBranches.GetValueOrDefault() == 0 ? 1 : summaryResult.CoveredBranches.GetValueOrDefault() / (double)summaryResult.TotalBranches.GetValueOrDefault();

            var rootElement = new XElement("coverage");

            rootElement.Add(new XAttribute("line-rate", lineRate.ToString(CultureInfo.InvariantCulture)));
            rootElement.Add(new XAttribute("branch-rate", branchRate.ToString(CultureInfo.InvariantCulture)));
            rootElement.Add(new XAttribute("lines-covered", summaryResult.CoveredLines.ToString(CultureInfo.InvariantCulture)));
            rootElement.Add(new XAttribute("lines-valid", summaryResult.CoverableLines.ToString(CultureInfo.InvariantCulture)));
            rootElement.Add(new XAttribute("branches-covered", summaryResult.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)));
            rootElement.Add(new XAttribute("branches-valid", summaryResult.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)));
            rootElement.Add(new XAttribute("complexity", summaryComplexity.HasValue ? summaryComplexity.Value.ToString(CultureInfo.InvariantCulture) : "NaN"));
            rootElement.Add(new XAttribute("version", 0));
            rootElement.Add(new XAttribute("timestamp", ((long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds).ToString(CultureInfo.InvariantCulture)));

            var sourcesElement = new XElement("sources");

            foreach (var sourceDirectory in summaryResult.SourceDirectories)
            {
                sourcesElement.Add(new XElement("source", sourceDirectory));
            }

            rootElement.Add(sourcesElement);

            var packagesElement = new XElement("packages");

            foreach (var packageElement in this.packageElementsByName.Values)
            {
                packagesElement.Add(packageElement);
            }

            rootElement.Add(packagesElement);

            XDocument result = new XDocument(new XDeclaration("1.0", null, null), rootElement);
            result.AddFirst(new XDocumentType("coverage", null, "http://cobertura.sourceforge.net/xml/coverage-04.dtd", null));

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

            string targetPath = Path.Combine(targetDirectory, "Cobertura.xml");

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
        /// Adds the lines to the given parent element.
        /// </summary>
        /// <param name="parent">The parent element.</param>
        /// <param name="lines">The lines to add.</param>
        /// <param name="lineRate">The line rate.</param>
        /// <param name="branchRate">The branch rate.</param>
        private void AddLineElements(XElement parent, IEnumerable<LineAnalysis> lines, out double lineRate, out double branchRate)
        {
            int coveredLines = 0;
            int totalLines = 0;

            int coveredBranches = 0;
            int totalBranches = 0;

            foreach (var line in lines)
            {
                if (line.LineVisitStatus == LineVisitStatus.NotCoverable)
                {
                    continue;
                }

                totalLines++;

                if (line.LineVisits > 0)
                {
                    coveredLines++;
                }

                bool hasBranch = line.TotalBranches.GetValueOrDefault() > 0;

                var lineElement = new XElement(
                    "line",
                    new XAttribute("number", line.LineNumber),
                    new XAttribute("hits", line.LineVisits.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("branch", hasBranch ? "true" : "false"));

                if (hasBranch)
                {
                    int visitedBranchesLine = line.CoveredBranches.GetValueOrDefault();
                    int totalBranchesLine = line.TotalBranches.GetValueOrDefault();

                    coveredBranches += visitedBranchesLine;
                    totalBranches += totalBranchesLine;

                    double coverage = visitedBranchesLine / (double)totalBranchesLine;
                    lineElement.Add(new XAttribute(
                        "condition-coverage",
                        string.Format("{0}% ({1}/{2})", Math.Round(coverage * 100, MidpointRounding.AwayFromZero), visitedBranchesLine, totalBranchesLine)));
                }

                parent.Add(lineElement);
            }

            lineRate = totalLines == 0 ? 1 : coveredLines / (double)totalLines;
            branchRate = totalBranches == 0 ? 1 : coveredBranches / (double)totalBranches;
        }
    }
}
