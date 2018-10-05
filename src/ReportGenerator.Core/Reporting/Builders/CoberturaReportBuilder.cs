using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates xml report in Cobertura format
    /// </summary>
    public class CoberturaReportBuilder : IReportBuilder
    {
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
            XElement packageElement = null;

            if (!this.packageElementsByName.TryGetValue(@class.Assembly.Name, out packageElement))
            {
                packageElement = new XElement("package", new XAttribute("name", @class.Assembly.Name));
                packageElement.Add(new XElement("classes"));

                this.packageElementsByName.Add(@class.Assembly.Name, packageElement);
            }

            var classesElement = packageElement.Element("classes");

            foreach (var fileAnalysis in fileAnalyses)
            {
                int fileCoveredLines = fileAnalysis.Lines.Count(l => l.LineVisits > 0);
                int fileTotalLines = fileAnalysis.Lines.Count(l => l.LineVisits >= 0);

                int fileCoveredBranches = fileAnalysis.Lines.Sum(l => l.CoveredBranches.GetValueOrDefault());
                int fileTotalBranches = fileAnalysis.Lines.Sum(l => l.TotalBranches.GetValueOrDefault());

                double lineRate = fileTotalLines == 0 ? 1 : fileCoveredLines / (double)fileTotalLines;
                double branchRate = fileTotalBranches == 0 ? 1 : fileCoveredBranches / (double)fileTotalBranches;

                var classElement = new XElement(
                    "class",
                    new XElement("methods"),
                    new XAttribute("name", @class.Name),
                    new XAttribute("filename", fileAnalysis.Path),
                    new XAttribute("line-rate", lineRate.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("branch-rate", branchRate.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("complexity", "NaN"));

                var linesElement = new XElement("lines");

                foreach (var line in fileAnalysis.Lines)
                {
                    if (line.LineVisitStatus == LineVisitStatus.NotCoverable)
                    {
                        continue;
                    }

                    bool hasBranch = line.TotalBranches.GetValueOrDefault() > 0;

                    var lineElement = new XElement(
                        "line",
                        new XAttribute("number", line.LineNumber),
                        new XAttribute("hits", line.LineVisits.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("branch", hasBranch ? "true" : "false"));

                    if (hasBranch)
                    {
                        int visitedBranches = line.CoveredBranches.GetValueOrDefault();
                        int totalBranches = line.TotalBranches.GetValueOrDefault();

                        double coverage = visitedBranches / (double)totalBranches;
                        lineElement.Add(new XAttribute(
                            "condition-coverage",
                            string.Format("{0}% ({1}/{2})", Math.Round(coverage * 100, MidpointRounding.AwayFromZero), visitedBranches, totalBranches)));
                    }

                    linesElement.Add(lineElement);
                }

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
            foreach (var assembly in summaryResult.Assemblies)
            {
                if (this.packageElementsByName.TryGetValue(assembly.Name, out XElement packageElement))
                {
                    double packageLineRate = assembly.CoverableLines == 0 ? 1 : assembly.CoveredLines / (double)assembly.CoverableLines;
                    double packageBranchRate = assembly.TotalBranches.GetValueOrDefault() == 0 ? 1 : assembly.CoveredBranches.GetValueOrDefault() / (double)assembly.TotalBranches.GetValueOrDefault();

                    packageElement.Add(new XAttribute("line-rate", packageLineRate.ToString(CultureInfo.InvariantCulture)));
                    packageElement.Add(new XAttribute("branch-rate", packageBranchRate.ToString(CultureInfo.InvariantCulture)));
                    packageElement.Add(new XAttribute("complexity", "NaN"));
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
            rootElement.Add(new XAttribute("complexity", "NaN"));
            rootElement.Add(new XAttribute("version", 0));
            rootElement.Add(new XAttribute("timestamp", ((long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds).ToString(CultureInfo.InvariantCulture)));

            rootElement.Add(new XElement("sources"));

            var packagesElement = new XElement("packages");

            foreach (var packageElement in this.packageElementsByName.Values)
            {
                packagesElement.Add(packageElement);
            }

            rootElement.Add(packagesElement);

            XDocument result = new XDocument(new XDeclaration("1.0", null, null), rootElement);
            result.AddFirst(new XDocumentType("coverage", null, "http://cobertura.sourceforge.net/xml/coverage-04.dtd", null));

            result.Save(Path.Combine(this.ReportContext.ReportConfiguration.TargetDirectory, "Cobertura.xml"));
        }
    }
}
