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
    /// Creates xml report in Clover format.
    /// Corresponding XSD: https://bitbucket.org/atlassian/clover/src/default/etc/schema/clover.xsd.
    /// </summary>
    public class CloverReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(CloverReportBuilder));

        /// <summary>
        /// Package elements by assembly name.
        /// </summary>
        private readonly Dictionary<string, XElement> packageElementsByName = new Dictionary<string, XElement>();

        /// <summary>
        /// Total lines by file element.
        /// </summary>
        private readonly Dictionary<XElement, int> totalLinesByFileElement = new Dictionary<XElement, int>();

        /// <summary>
        /// Coverable lines by file element.
        /// </summary>
        private readonly Dictionary<XElement, int> coverableLinesByFileElement = new Dictionary<XElement, int>();

        /// <summary>
        /// Gets the type of the report.
        /// </summary>
        /// <value>
        /// The type of the report.
        /// </value>
        public string ReportType => "Clover";

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

                this.packageElementsByName.Add(@class.Assembly.Name, packageElement);
            }

            foreach (var fileAnalysis in fileAnalyses)
            {
                CodeFile codeFile = @class.Files.FirstOrDefault(f => f.Path == fileAnalysis.Path);

                var fileElement = packageElement.Elements("file")
                    .Where(f => f.Attribute("path").Value == fileAnalysis.Path)
                    .FirstOrDefault();

                bool newFile = false;

                if (fileElement == null)
                {
                    newFile = true;

                    fileElement = new XElement(
                        "file",
                        new XAttribute("path", fileAnalysis.Path),
                        new XAttribute("name", new DirectoryInfo(fileAnalysis.Path).Name));

                    packageElement.Add(fileElement);

                    this.totalLinesByFileElement[fileElement] = codeFile == null ? 0 : codeFile.TotalLines.GetValueOrDefault();
                    this.coverableLinesByFileElement[fileElement] = codeFile == null ? 0 : codeFile.CoverableLines;
                }
                else
                {
                    this.coverableLinesByFileElement[fileElement] += codeFile == null ? 0 : codeFile.CoverableLines;
                }

                int coveredMethods = 0;

                foreach (var line in fileAnalysis.Lines)
                {
                    var codeElement = codeFile?.CodeElements
                        .FirstOrDefault(c => c.FirstLine == line.LineNumber);

                    if (line.LineVisitStatus == LineVisitStatus.NotCoverable
                        && codeElement == null)
                    {
                        continue;
                    }

                    string lineNumber = line.LineNumber.ToString(CultureInfo.InvariantCulture);

                    if (newFile
                        || !fileElement.Elements("line").Any(e => e.Attribute("num").Value == lineNumber))
                    {
                        var lineElement = new XElement("line");

                        var metric = codeFile?.MethodMetrics
                            .FirstOrDefault(c => c.Line == line.LineNumber);

                        if (metric != null)
                        {
                            var complexityMetric = metric.Metrics
                                .FirstOrDefault(m => m.Name == ReportResources.CyclomaticComplexity);

                            if (complexityMetric != null && complexityMetric.Value.HasValue)
                            {
                                lineElement.Add(new XAttribute("complexity", complexityMetric.Value.Value.ToString(CultureInfo.InvariantCulture)));
                            }
                        }

                        if (codeElement != null)
                        {
                            int lineVisits = 0;

                            int maxLine = codeElement.LastLine;

                            if (codeElement.FirstLine == codeElement.LastLine)
                            {
                                var nextCodeElement = codeFile.CodeElements
                                    .Where(c => c.FirstLine > codeElement.FirstLine)
                                    .OrderBy(c => c.FirstLine)
                                    .FirstOrDefault();

                                if (nextCodeElement != null)
                                {
                                    maxLine = nextCodeElement.FirstLine - 1;
                                }
                                else
                                {
                                    maxLine = codeFile.LineCoverage.Count - 1;
                                }
                            }

                            for (int i = codeElement.FirstLine; i <= maxLine; i++)
                            {
                                if (codeFile.LineCoverage.Count > i)
                                {
                                    lineVisits = Math.Max(lineVisits, codeFile.LineCoverage[i]);
                                }
                            }

                            if (lineVisits > 0)
                            {
                                coveredMethods++;
                            }

                            lineElement.Add(new XAttribute("signature", codeElement.Name));
                            lineElement.Add(new XAttribute("num", lineNumber));
                            lineElement.Add(new XAttribute("count", lineVisits));
                            lineElement.Add(new XAttribute("type", "method"));
                        }
                        else
                        {
                            if (codeFile != null && codeFile.BranchesByLine.TryGetValue(line.LineNumber, out ICollection<Branch> branches))
                            {
                                lineElement.Add(new XAttribute("falsecount", branches.Count(b => b.BranchVisits == 0).ToString(CultureInfo.InvariantCulture)));
                                lineElement.Add(new XAttribute("truecount", branches.Count(b => b.BranchVisits > 0).ToString(CultureInfo.InvariantCulture)));
                                lineElement.Add(new XAttribute("num", lineNumber));
                                lineElement.Add(new XAttribute("type", "cond"));
                            }
                            else
                            {
                                lineElement.Add(new XAttribute("num", lineNumber));
                                lineElement.Add(new XAttribute("count", line.LineVisits));
                                lineElement.Add(new XAttribute("type", "stmt"));
                            }
                        }

                        fileElement.Add(lineElement);
                    }
                }

                var classElement = new XElement(
                    "class",
                    new XAttribute("name", @class.Name));

                classElement.Add(new XElement(
                    "metrics",
                    new XAttribute("complexity", "0"), // Not available
                    new XAttribute("elements", codeFile == null ? "0" : codeFile.CoverableLines.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("coveredelements", codeFile == null ? "0" : codeFile.CoveredLines.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("conditionals", codeFile == null ? "0" : codeFile.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("coveredconditionals", codeFile == null ? "0" : codeFile.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("statements", codeFile == null ? "0" : codeFile.CoverableLines.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("coveredstatements", codeFile == null ? "0" : codeFile.CoveredLines.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("coveredmethods", coveredMethods.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("methods", codeFile == null ? "0" : codeFile.CodeElements.Count().ToString(CultureInfo.InvariantCulture))));

                fileElement.AddFirst(classElement);
            }
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        public void CreateSummaryReport(SummaryResult summaryResult)
        {
            string timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

            var rootElement = new XElement("coverage");

            rootElement.Add(new XAttribute("generated", timeStamp));
            rootElement.Add(new XAttribute("clover", "4.3.1"));

            var projectElement = new XElement(
                "project",
                new XAttribute("timestamp", timeStamp));

            rootElement.Add(projectElement);

            int totalProjectElements = 0;
            int totalProjectCoveredelements = 0;
            int totalProjectConditionals = 0;
            int totalProjectCoveredconditionals = 0;
            int totalProjectStatements = 0;
            int totalProjectCoveredstatements = 0;
            int totalProjectCoveredmethods = 0;
            int totalProjectMethods = 0;
            int totalProjectClasses = 0;
            int totalProjectLoc = 0;
            int totalProjectNcloc = 0;
            int totalProjectFiles = 0;

            var assembliesWithClasses = summaryResult.Assemblies
                .Where(a => a.Classes.Any())
                .ToArray();

            foreach (var assembly in assembliesWithClasses)
            {
                if (this.packageElementsByName.TryGetValue(assembly.Name, out XElement packageElement))
                {
                    int totalPackageElements = 0;
                    int totalPackageCoveredelements = 0;
                    int totalPackageConditionals = 0;
                    int totalPackageCoveredconditionals = 0;
                    int totalPackageStatements = 0;
                    int totalPackageCoveredstatements = 0;
                    int totalPackageCoveredmethods = 0;
                    int totalPackageMethods = 0;
                    int totalPackageClasses = 0;
                    int totalPackageLoc = 0;
                    int totalPackageNcloc = 0;

                    var fileElements = packageElement.Elements("file").ToArray();
                    totalProjectFiles += fileElements.Length;

                    foreach (var fileElement in fileElements)
                    {
                        var classMetricElements = fileElement.Elements("class").Elements("metrics").ToArray();

                        int elements = classMetricElements.SafeSum(e => int.Parse(e.Attribute("elements").Value, CultureInfo.InvariantCulture));
                        int coveredelements = classMetricElements.SafeSum(e => int.Parse(e.Attribute("coveredelements").Value, CultureInfo.InvariantCulture));
                        int conditionals = classMetricElements.SafeSum(e => int.Parse(e.Attribute("conditionals").Value, CultureInfo.InvariantCulture));
                        int coveredconditionals = classMetricElements.SafeSum(e => int.Parse(e.Attribute("coveredconditionals").Value, CultureInfo.InvariantCulture));
                        int statements = classMetricElements.SafeSum(e => int.Parse(e.Attribute("statements").Value, CultureInfo.InvariantCulture));
                        int coveredstatements = classMetricElements.SafeSum(e => int.Parse(e.Attribute("coveredstatements").Value, CultureInfo.InvariantCulture));
                        int coveredmethods = classMetricElements.SafeSum(e => int.Parse(e.Attribute("coveredmethods").Value, CultureInfo.InvariantCulture));
                        int methods = classMetricElements.SafeSum(e => int.Parse(e.Attribute("methods").Value, CultureInfo.InvariantCulture));
                        int classes = classMetricElements.Length;
                        int loc = this.totalLinesByFileElement[fileElement];
                        int ncloc = this.coverableLinesByFileElement[fileElement];

                        totalPackageElements += elements;
                        totalPackageCoveredelements += coveredelements;
                        totalPackageConditionals += conditionals;
                        totalPackageCoveredconditionals += coveredconditionals;
                        totalPackageStatements += statements;
                        totalPackageCoveredstatements += coveredstatements;
                        totalPackageCoveredmethods += coveredmethods;
                        totalPackageMethods += methods;
                        totalPackageClasses += classes;
                        totalPackageLoc += loc;
                        totalPackageNcloc += ncloc;

                        totalProjectElements += elements;
                        totalProjectCoveredelements += coveredelements;
                        totalProjectConditionals += conditionals;
                        totalProjectCoveredconditionals += coveredconditionals;
                        totalProjectStatements += statements;
                        totalProjectCoveredstatements += coveredstatements;
                        totalProjectCoveredmethods += coveredmethods;
                        totalProjectMethods += methods;
                        totalProjectClasses += classes;
                        totalProjectLoc += loc;
                        totalProjectNcloc += ncloc;

                        fileElement.AddFirst(new XElement(
                            "metrics",
                            new XAttribute("complexity", "0"), // Not available
                            new XAttribute("elements", elements.ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("coveredelements", coveredelements.ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("conditionals", conditionals.ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("coveredconditionals", coveredconditionals.ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("statements", statements.ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("coveredstatements", coveredstatements.ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("coveredmethods", coveredmethods.ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("methods", methods.ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("classes", classes.ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("loc", loc.ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("ncloc", ncloc.ToString(CultureInfo.InvariantCulture))));
                    }

                    packageElement.AddFirst(new XElement(
                        "metrics",
                        new XAttribute("complexity", "0"), // Not available
                        new XAttribute("elements", totalPackageElements.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("coveredelements", totalPackageCoveredelements.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("conditionals", totalPackageConditionals.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("coveredconditionals", totalPackageCoveredconditionals.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("statements", totalPackageStatements.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("coveredstatements", totalPackageCoveredstatements.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("coveredmethods", totalPackageCoveredmethods.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("methods", totalPackageMethods.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("classes", totalPackageClasses.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("loc", totalPackageLoc.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("ncloc", totalPackageNcloc.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("files", fileElements.Length.ToString(CultureInfo.InvariantCulture))));

                    projectElement.Add(packageElement);
                }
            }

            projectElement.AddFirst(new XElement(
                "metrics",
                new XAttribute("complexity", "0"), // Not available
                new XAttribute("elements", totalProjectElements.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("coveredelements", totalProjectCoveredelements.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("conditionals", totalProjectConditionals.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("coveredconditionals", totalProjectCoveredconditionals.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("statements", totalProjectStatements.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("coveredstatements", totalProjectCoveredstatements.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("coveredmethods", totalProjectCoveredmethods.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("methods", totalProjectMethods.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("classes", totalProjectClasses.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("loc", totalProjectLoc.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("ncloc", totalProjectNcloc.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("files", totalProjectFiles.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("packages", assembliesWithClasses.Length.ToString(CultureInfo.InvariantCulture))));

            var testProjectElement = new XElement(
                "testproject",
                new XAttribute("timestamp", timeStamp));

            testProjectElement.Add(new XElement(
                "metrics",
                new XAttribute("complexity", "0"), // Not available
                new XAttribute("elements", "0"), // Not available
                new XAttribute("coveredelements", "0"), // Not available
                new XAttribute("conditionals", "0"), // Not available
                new XAttribute("coveredconditionals", "0"), // Not available
                new XAttribute("statements", "0"), // Not available
                new XAttribute("coveredstatements", "0"), // Not available
                new XAttribute("coveredmethods", "0"), // Not available
                new XAttribute("methods", "0"), // Not available
                new XAttribute("classes", "0"), // Not available
                new XAttribute("loc", "0"), // Not available
                new XAttribute("ncloc", "0"), // Not available
                new XAttribute("files", "0"), // Not available
                new XAttribute("packages", "0"))); // Not available

            rootElement.Add(testProjectElement);

            XDocument result = new XDocument(new XDeclaration("1.0", "UTF-8", null), rootElement);

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

            string targetPath = Path.Combine(targetDirectory, "Clover.xml");

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
    }
}
