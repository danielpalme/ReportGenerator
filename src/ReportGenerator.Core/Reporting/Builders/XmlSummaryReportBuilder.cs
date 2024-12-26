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
using Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates summary report in XML format (no reports for classes are generated).
    /// </summary>
    public class XmlSummaryReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(XmlSummaryReportBuilder));

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report format.
        /// </value>
        public virtual string ReportType => "XmlSummary";

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
        public virtual void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses)
        {
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        public void CreateSummaryReport(SummaryResult summaryResult)
        {
            var assembliesWithClasses = summaryResult.Assemblies
                .Where(a => a.Classes.Any())
                .ToArray();

            var rootElement = new XElement("CoverageReport", new XAttribute("scope", "Summary"));
            var summaryElement = new XElement("Summary");

            summaryElement.Add(new XElement("Generatedon", DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString()));
            summaryElement.Add(new XElement("Parser", summaryResult.UsedParser));
            summaryElement.Add(new XElement("Assemblies", assembliesWithClasses.Count().ToString(CultureInfo.InvariantCulture)));
            summaryElement.Add(new XElement("Classes", assembliesWithClasses.SelectMany(a => a.Classes).Count().ToString(CultureInfo.InvariantCulture)));
            summaryElement.Add(new XElement("Files", assembliesWithClasses.SelectMany(a => a.Classes).SelectMany(a => a.Files).Distinct().Count().ToString(CultureInfo.InvariantCulture)));

            summaryElement.Add(new XElement("Coveredlines", summaryResult.CoveredLines.ToString(CultureInfo.InvariantCulture)));
            summaryElement.Add(new XElement("Uncoveredlines", (summaryResult.CoverableLines - summaryResult.CoveredLines).ToString(CultureInfo.InvariantCulture)));
            summaryElement.Add(new XElement("Coverablelines", summaryResult.CoverableLines.ToString(CultureInfo.InvariantCulture)));
            summaryElement.Add(new XElement("Totallines", summaryResult.TotalLines.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)));
            summaryElement.Add(new XElement("Linecoverage", summaryResult.CoverageQuota.HasValue ? summaryResult.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));

            if (summaryResult.CoveredBranches.HasValue && summaryResult.TotalBranches.HasValue)
            {
                summaryElement.Add(new XElement("Coveredbranches", summaryResult.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)));
                summaryElement.Add(new XElement("Totalbranches", summaryResult.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)));

                if (summaryResult.BranchCoverageQuota.HasValue)
                {
                    summaryElement.Add(new XElement("Branchcoverage", summaryResult.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture)));
                }
            }

            summaryElement.Add(new XElement("Coveredmethods", summaryResult.CoveredCodeElements.ToString(CultureInfo.InvariantCulture)));
            summaryElement.Add(new XElement("Fullcoveredmethods", summaryResult.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture)));
            summaryElement.Add(new XElement("Totalmethods", summaryResult.TotalCodeElements.ToString(CultureInfo.InvariantCulture)));
            summaryElement.Add(new XElement("Methodcoverage", summaryResult.CodeElementCoverageQuota.HasValue ? summaryResult.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
            summaryElement.Add(new XElement("Fullmethodcoverage", summaryResult.FullCodeElementCoverageQuota.HasValue ? summaryResult.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));

            if (this.ReportContext.ReportConfiguration.Title != null)
            {
                summaryElement.Add(new XElement("Title", this.ReportContext.ReportConfiguration.Title));
            }

            if (this.ReportContext.ReportConfiguration.Tag != null)
            {
                summaryElement.Add(new XElement("Tag", this.ReportContext.ReportConfiguration.Tag));
            }

            rootElement.Add(summaryElement);

            var sumableMetrics = summaryResult.SumableMetrics;

            if (sumableMetrics.Count > 0)
            {
                var metricsElement = new XElement("Metrics");
                var metricElement = new XElement("Element", new XAttribute("name", "Total"));

                foreach (var m in sumableMetrics)
                {
                    var element = new XElement(StringHelper.ReplaceNonLetterChars(m.Name));

                    if (m.Value.HasValue)
                    {
                        element.Value = m.Value.Value.ToString(CultureInfo.InvariantCulture);
                    }

                    metricElement.Add(element);
                }

                metricsElement.Add(metricElement);
                rootElement.Add(metricsElement);
            }

            var coverageElement = new XElement("Coverage");

            foreach (var assembly in assembliesWithClasses)
            {
                var assemblyElement = new XElement("Assembly");

                assemblyElement.Add(new XAttribute("name", assembly.Name));
                assemblyElement.Add(new XAttribute("classes", assembly.Classes.Count().ToString(CultureInfo.InvariantCulture)));
                assemblyElement.Add(new XAttribute("coverage", assembly.CoverageQuota.HasValue ? assembly.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
                assemblyElement.Add(new XAttribute("coveredlines", assembly.CoveredLines.ToString(CultureInfo.InvariantCulture)));
                assemblyElement.Add(new XAttribute("coverablelines", assembly.CoverableLines.ToString(CultureInfo.InvariantCulture)));
                assemblyElement.Add(new XAttribute("totallines", assembly.TotalLines.HasValue ? assembly.TotalLines.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
                assemblyElement.Add(new XAttribute("branchcoverage", assembly.BranchCoverageQuota.HasValue ? assembly.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
                assemblyElement.Add(new XAttribute("coveredbranches", assembly.CoveredBranches.HasValue ? assembly.CoveredBranches.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
                assemblyElement.Add(new XAttribute("totalbranches", assembly.TotalBranches.HasValue ? assembly.TotalBranches.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
                assemblyElement.Add(new XAttribute("coveredmethods", assembly.CoveredCodeElements.ToString(CultureInfo.InvariantCulture)));
                assemblyElement.Add(new XAttribute("fullcoveredmethods", assembly.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture)));
                assemblyElement.Add(new XAttribute("totalmethods", assembly.TotalCodeElements.ToString(CultureInfo.InvariantCulture)));
                assemblyElement.Add(new XAttribute("methodcoverage", assembly.CodeElementCoverageQuota.HasValue ? assembly.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
                assemblyElement.Add(new XAttribute("fullmethodcoverage", assembly.FullCodeElementCoverageQuota.HasValue ? assembly.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));

                foreach (var @class in assembly.Classes)
                {
                    var classElement = new XElement("Class");

                    classElement.Add(new XAttribute("name", @class.Name));
                    classElement.Add(new XAttribute("coverage", @class.CoverageQuota.HasValue ? @class.CoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
                    classElement.Add(new XAttribute("coveredlines", @class.CoveredLines.ToString(CultureInfo.InvariantCulture)));
                    classElement.Add(new XAttribute("coverablelines", @class.CoverableLines.ToString(CultureInfo.InvariantCulture)));
                    classElement.Add(new XAttribute("totallines", @class.TotalLines.HasValue ? @class.TotalLines.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
                    classElement.Add(new XAttribute("branchcoverage", @class.BranchCoverageQuota.HasValue ? @class.BranchCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
                    classElement.Add(new XAttribute("coveredbranches", @class.CoveredBranches.HasValue ? @class.CoveredBranches.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
                    classElement.Add(new XAttribute("totalbranches", @class.TotalBranches.HasValue ? @class.TotalBranches.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
                    classElement.Add(new XAttribute("coveredmethods", @class.CoveredCodeElements.ToString(CultureInfo.InvariantCulture)));
                    classElement.Add(new XAttribute("fullcoveredmethods", @class.FullCoveredCodeElements.ToString(CultureInfo.InvariantCulture)));
                    classElement.Add(new XAttribute("totalmethods", @class.TotalCodeElements.ToString(CultureInfo.InvariantCulture)));
                    classElement.Add(new XAttribute("methodcoverage", @class.CodeElementCoverageQuota.HasValue ? @class.CodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
                    classElement.Add(new XAttribute("fullmethodcoverage", @class.FullCodeElementCoverageQuota.HasValue ? @class.FullCodeElementCoverageQuota.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));

                    assemblyElement.Add(classElement);
                }

                coverageElement.Add(assemblyElement);
            }

            rootElement.Add(coverageElement);

            XDocument result = new XDocument(new XDeclaration("1.0", "utf-8", null), rootElement);

            string targetPath = Path.Combine(
                this.CreateTargetDirectory(),
                "Summary.xml");

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
        /// Creates the target directory.
        /// </summary>
        /// <returns>The target directory.</returns>
        protected string CreateTargetDirectory()
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
                        throw;
                    }
                }
            }

            return targetDirectory;
        }
    }
}
