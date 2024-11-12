using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Licensing;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates xml report in OpenCover format.
    /// </summary>
    public class OpenCoverReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(OpenCoverReportBuilder));

        /// <summary>
        /// Package elements by assembly name.
        /// </summary>
        private readonly Dictionary<string, XElement> packageElementsByName = new Dictionary<string, XElement>();

        /// <summary>
        /// File ids by path.
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, int>> fileIdByPathByName = new Dictionary<string, Dictionary<string, int>>();

        /// <summary>
        /// XSI namepace.
        /// </summary>
        private readonly XNamespace xsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

        /// <summary>
        /// Number of current file.
        /// </summary>
        private int fileCounter = 1;

        /// <summary>
        /// Gets the type of the report.
        /// </summary>
        /// <value>
        /// The type of the report.
        /// </value>
        public string ReportType => "OpenCover";

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
            bool proVersion = this.ReportContext.ReportConfiguration.License.DetermineLicenseType() == LicenseType.Pro;

            if (!proVersion)
            {
                return;
            }

            Dictionary<string, int> fileIdByPath;

            if (!this.packageElementsByName.TryGetValue(@class.Assembly.Name, out XElement packageElement))
            {
                packageElement = new XElement("Module", new XAttribute("hash", @class.Assembly.GetHashCode()));
                packageElement.Add(new XElement(
                    "Summary",
                    new XAttribute("numSequencePoints", @class.Assembly.CoverableLines.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("visitedSequencePoints", @class.Assembly.CoveredLines.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("numBranchPoints", @class.Assembly.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("visitedBranchPoints", @class.Assembly.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("sequenceCoverage", @class.Assembly.CoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("branchCoverage", @class.Assembly.BranchCoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("maxCyclomaticComplexity", @class.Assembly.Classes.SelectMany(c => c.Files).SelectMany(f => f.MethodMetrics).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CyclomaticComplexity).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Max().ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("minCyclomaticComplexity", @class.Assembly.Classes.SelectMany(c => c.Files).SelectMany(f => f.MethodMetrics).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CyclomaticComplexity).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Min().ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("maxCrapScore", @class.Assembly.Classes.SelectMany(c => c.Files).SelectMany(f => f.MethodMetrics).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CrapScore).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Max().ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("minCrapScore", @class.Assembly.Classes.SelectMany(c => c.Files).SelectMany(f => f.MethodMetrics).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CrapScore).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Min().ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("visitedClasses", @class.Assembly.Classes.Count(c => c.CoveredLines > 0).ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("numClasses", @class.Assembly.Classes.Count().ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("visitedMethods", @class.Assembly.CoveredCodeElements.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("numMethods", @class.Assembly.TotalCodeElements.ToString(CultureInfo.InvariantCulture))));

                packageElement.Add(new XElement("ModulePath", @class.Assembly.Name));
                packageElement.Add(new XElement("ModuleTime", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)));
                packageElement.Add(new XElement("ModuleName", @class.Assembly.Name));
                packageElement.Add(new XElement("Files"));
                packageElement.Add(new XElement("Classes"));

                this.packageElementsByName.Add(@class.Assembly.Name, packageElement);
                fileIdByPath = new Dictionary<string, int>();
                this.fileIdByPathByName.Add(@class.Assembly.Name, fileIdByPath);
            }
            else
            {
                fileIdByPath = this.fileIdByPathByName[@class.Assembly.Name];
            }

            var filesElement = packageElement.Element("Files");
            var classesElement = packageElement.Element("Classes");

            var classElement = new XElement("Class");
            classElement.Add(new XElement(
                "Summary",
                new XAttribute("numSequencePoints", @class.CoverableLines.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("visitedSequencePoints", @class.CoveredLines.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("numBranchPoints", @class.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("visitedBranchPoints", @class.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("sequenceCoverage", @class.CoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("branchCoverage", @class.BranchCoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("maxCyclomaticComplexity", @class.Files.SelectMany(f => f.MethodMetrics).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CyclomaticComplexity).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Max().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("minCyclomaticComplexity", @class.Files.SelectMany(f => f.MethodMetrics).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CyclomaticComplexity).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Min().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("maxCrapScore", @class.Files.SelectMany(f => f.MethodMetrics).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CrapScore).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Max().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("minCrapScore", @class.Files.SelectMany(f => f.MethodMetrics).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CrapScore).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Min().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("visitedClasses", @class.CoveredLines > 0 ? "1" : "0"),
                new XAttribute("numClasses", "1"),
                new XAttribute("visitedMethods", @class.CoveredCodeElements.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("numMethods", @class.TotalCodeElements.ToString(CultureInfo.InvariantCulture))));
            classElement.Add(new XElement("FullName", @class.Name));

            var methodsElement = new XElement("Methods");
            classElement.Add(methodsElement);

            classesElement.Add(classElement);

            foreach (var fileAnalysis in fileAnalyses)
            {
                foreach (var file in @class.Files)
                {
                    if (file.Path == fileAnalysis.Path)
                    {
                        if (!fileIdByPath.TryGetValue(file.Path, out int fileId))
                        {
                            fileId = this.fileCounter++;

                            fileIdByPath.Add(file.Path, fileId);
                            filesElement.Add(new XElement(
                                "File",
                                new XAttribute("uid", fileId),
                                new XAttribute("fullPath", file.Path)));
                        }

                        foreach (var codeElement in file.CodeElements)
                        {
                            var sequencePointsElement = new XElement("SequencePoints");
                            var branchPointsElement = new XElement("BranchPoints");

                            this.AddSequenceAndBranchPoints(
                                sequencePointsElement,
                                branchPointsElement,
                                file,
                                fileAnalysis.Lines.Skip(codeElement.FirstLine - 1).Take(codeElement.LastLine - codeElement.FirstLine + 1),
                                fileId,
                                out int coveredLines,
                                out int coverableLines,
                                out int methodVisits,
                                out int coveredBranches,
                                out int totalBranches);

                            var methodElement = new XElement(
                                "Method",
                                new XAttribute("visited", codeElement.CoverageQuota.GetValueOrDefault() > 0 ? "true" : "false"),
                                new XAttribute("cyclomaticComplexity", file.MethodMetrics.Where(m => m.FullName == codeElement.FullName).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CyclomaticComplexity).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Max().ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("nPathComplexity", file.MethodMetrics.Where(m => m.FullName == codeElement.FullName).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.NPathComplexity).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Max().ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("sequenceCoverage", codeElement.CoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("branchCoverage", this.CalculateQuota(coveredBranches, totalBranches).ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("crapScore", file.MethodMetrics.Where(m => m.FullName == codeElement.FullName).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CrapScore).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Max().ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("isConstructor", "false"), // Information not available
                                new XAttribute("isStatic", "false"), // Information not available
                                new XAttribute("isGetter", codeElement.FullName.Contains("get_") ? "true" : "false"),
                                new XAttribute("isSetter", codeElement.FullName.Contains("set_") ? "true" : "false"));

                            methodElement.Add(new XElement(
                                "Summary",
                                new XAttribute("numSequencePoints", coverableLines.ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("visitedSequencePoints", coveredLines.ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("numBranchPoints", totalBranches.ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("visitedBranchPoints", coveredBranches.ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("sequenceCoverage", this.CalculateQuota(coveredLines, coverableLines).ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("branchCoverage", this.CalculateQuota(coveredBranches, totalBranches).ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("maxCyclomaticComplexity", @class.Files.SelectMany(f => f.MethodMetrics).Where(m => m.FullName == codeElement.FullName).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CyclomaticComplexity).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Max().ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("minCyclomaticComplexity", @class.Files.SelectMany(f => f.MethodMetrics).Where(m => m.FullName == codeElement.FullName).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CyclomaticComplexity).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Min().ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("maxCrapScore", @class.Files.SelectMany(f => f.MethodMetrics).Where(m => m.FullName == codeElement.FullName).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CrapScore).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Max().ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("minCrapScore", @class.Files.SelectMany(f => f.MethodMetrics).Where(m => m.FullName == codeElement.FullName).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CrapScore).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Min().ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("visitedClasses", "0"),
                                new XAttribute("numClasses", "0"),
                                new XAttribute("visitedMethods", codeElement.CoverageQuota.GetValueOrDefault() > 0 ? "1" : "0"),
                                new XAttribute("numMethods", "1")));

                            methodElement.Add(new XElement("MetadataToken", codeElement.Name.GetHashCode().ToString(CultureInfo.InvariantCulture)));
                            methodElement.Add(new XElement("Name", codeElement.FullName));
                            methodElement.Add(new XElement(
                                "FileRef",
                                new XAttribute("uid", fileId.ToString(CultureInfo.InvariantCulture))));

                            methodElement.Add(sequencePointsElement);
                            methodElement.Add(branchPointsElement);

                            methodElement.Add(new XElement(
                                "MethodPoint",
                                new XAttribute(this.xsiNamespace + "type", "SequencePoint"),
                                new XAttribute("vc", methodVisits.ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("uspid", "0"), // Information not available
                                new XAttribute("ordinal", "0"), // Information not available
                                new XAttribute("offset", "0"), // Information not available
                                new XAttribute("sl", codeElement.FirstLine.ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("sc", "0"), // Information not available
                                new XAttribute("el", codeElement.LastLine.ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("ec", "0"), // Information not available
                                new XAttribute("bec", "0"), // Information not available
                                new XAttribute("bev", "0"), // Information not available
                                new XAttribute("fileid", fileId.ToString(CultureInfo.InvariantCulture))));

                            methodsElement.Add(methodElement);
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        public void CreateSummaryReport(SummaryResult summaryResult)
        {
            bool proVersion = this.ReportContext.ReportConfiguration.License.DetermineLicenseType() == LicenseType.Pro;

            if (!proVersion)
            {
                Logger.Warn(Resources.OpenCoverProVersion);
                return;
            }

            var assembliesWithClasses = summaryResult.Assemblies
                .Where(a => a.Classes.Any())
                .ToArray();

            var rootElement = new XElement(
                "CoverageSession",
                new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
                new XAttribute(XNamespace.Xmlns + "xsi", this.xsiNamespace),
                new XAttribute("Version", "4.6.601.0"));

            rootElement.Add(new XElement(
                "Summary",
                new XAttribute("numSequencePoints", summaryResult.CoverableLines.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("visitedSequencePoints", summaryResult.CoveredLines.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("numBranchPoints", summaryResult.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("visitedBranchPoints", summaryResult.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("sequenceCoverage", summaryResult.CoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("branchCoverage", summaryResult.BranchCoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("maxCyclomaticComplexity", assembliesWithClasses.SelectMany(f => f.Classes).SelectMany(c => c.Files).SelectMany(f => f.MethodMetrics).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CyclomaticComplexity).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Max().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("minCyclomaticComplexity", assembliesWithClasses.SelectMany(f => f.Classes).SelectMany(c => c.Files).SelectMany(f => f.MethodMetrics).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CyclomaticComplexity).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Min().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("maxCrapScore", assembliesWithClasses.SelectMany(f => f.Classes).SelectMany(c => c.Files).SelectMany(f => f.MethodMetrics).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CrapScore).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Max().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("minCrapScore", assembliesWithClasses.SelectMany(f => f.Classes).SelectMany(c => c.Files).SelectMany(f => f.MethodMetrics).SelectMany(m => m.Metrics).Where(m => m.Name == ReportResources.CrapScore).Select(m => m.Value.GetValueOrDefault()).DefaultIfEmpty().Min().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("visitedClasses", assembliesWithClasses.SelectMany(a => a.Classes).Count(c => c.CoverageQuota.GetValueOrDefault() > 0).ToString(CultureInfo.InvariantCulture)),
                new XAttribute("numClasses", assembliesWithClasses.SelectMany(a => a.Classes).Count().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("visitedMethods", summaryResult.CoveredCodeElements.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("numMethods", summaryResult.TotalCodeElements.ToString(CultureInfo.InvariantCulture))));

            var modulesElement = new XElement("Modules");
            rootElement.Add(modulesElement);

            foreach (var assembly in assembliesWithClasses)
            {
                if (this.packageElementsByName.TryGetValue(assembly.Name, out XElement packageElement))
                {
                    modulesElement.Add(packageElement);
                }
            }

            XDocument result = new XDocument(new XDeclaration("1.0", null, null), rootElement);

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

            string targetPath = Path.Combine(targetDirectory, "OpenCover.xml");

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
        /// <param name="sequencePointsElement">The SequencePoints element.</param>
        /// <param name="branchPointsElement">The BranchPoints element.</param>
        /// <param name="file">The code file.</param>
        /// <param name="lines">The lines to add.</param>
        /// <param name="fileId">The line rate.</param>
        /// <param name="coveredLines">The covered lines.</param>
        /// <param name="coverableLines">The coverable lines.</param>
        /// <param name="methodVisits">The number of method visits.</param>
        /// <param name="coveredBranches">The covered branches.</param>
        /// <param name="totalBranches">The ´total branches.</param>
        private void AddSequenceAndBranchPoints(
            XElement sequencePointsElement,
            XElement branchPointsElement,
            CodeFile file,
            IEnumerable<LineAnalysis> lines,
            int fileId,
            out int coveredLines,
            out int coverableLines,
            out int methodVisits,
            out int coveredBranches,
            out int totalBranches)
        {
            coveredLines = 0;
            coverableLines = 0;
            methodVisits = 0;
            coveredBranches = 0;
            totalBranches = 0;

            foreach (var line in lines)
            {
                if (line.LineVisitStatus == LineVisitStatus.NotCoverable)
                {
                    continue;
                }

                coverableLines++;

                if (line.LineVisits > 0)
                {
                    coveredLines++;
                }

                methodVisits = Math.Max(line.LineVisits, methodVisits);

                sequencePointsElement.Add(new XElement(
                    "SequencePoint",
                    new XAttribute("vc", line.LineVisits),
                    new XAttribute("uspid", "0"), // Information not available
                    new XAttribute("ordinal", "0"), // Information not available
                    new XAttribute("offset", "0"), // Information not available
                    new XAttribute("sl", line.LineNumber),
                    new XAttribute("sc", "0"), // Information not available
                    new XAttribute("el", line.LineNumber),
                    new XAttribute("ec", "0"), // Information not available
                    new XAttribute("bec", "0"), // Information not available
                    new XAttribute("bev", "0"), // Information not available
                    new XAttribute("fileid", fileId.ToString(CultureInfo.InvariantCulture))));

                if (file.BranchesByLine.TryGetValue(line.LineNumber, out var branches))
                {
                    int counter = 0;
                    foreach (var branch in branches)
                    {
                        branchPointsElement.Add(new XElement(
                            "BranchPoint",
                            new XAttribute("vc", branch.BranchVisits),
                            new XAttribute("uspid", "0"), // Information not available
                            new XAttribute("ordinal", "0"), // Information not available
                            new XAttribute("offset", "0"), // Information not available
                            new XAttribute("sl", line.LineNumber),
                            new XAttribute("path", counter),
                            new XAttribute("offsetend", "0"),
                            new XAttribute("fileid", fileId.ToString(CultureInfo.InvariantCulture))));

                        if (branch.BranchVisits > 0)
                        {
                            coveredBranches++;
                        }

                        counter++;
                        totalBranches++;
                    }
                }
            }
        }

        private decimal CalculateQuota(int covered, int total)
        {
            return (total == 0) ? 0m : MathExtensions.CalculatePercentage(covered, total);
        }
    }
}