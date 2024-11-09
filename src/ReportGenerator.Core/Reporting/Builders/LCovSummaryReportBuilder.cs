using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates summary report in lcov (See: https://github.com/linux-test-project/lcov, http://ltp.sourceforge.net/coverage/lcov/geninfo.1.php) (no reports for classes are generated).
    /// </summary>
    public class LCovSummaryReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(LCovSummaryReportBuilder));

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report type.
        /// </value>
        public string ReportType => "lcov";

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
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        public void CreateSummaryReport(SummaryResult summaryResult)
        {
            if (summaryResult == null)
            {
                throw new ArgumentNullException(nameof(summaryResult));
            }

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

            string targetPath = Path.Combine(targetDirectory, "lcov.info");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            using (var reportTextWriter = File.CreateText(targetPath))
            {
                reportTextWriter.WriteLine("TN:");
                long branchCounter = 0;

                var assembliesWithClasses = summaryResult.Assemblies
                    .Where(a => a.Classes.Any())
                    .ToArray();

                foreach (var assembly in assembliesWithClasses)
                {
                    foreach (var @class in assembly.Classes)
                    {
                        foreach (var file in @class.Files)
                        {
                            // SF:<absolute path to the source file>
                            reportTextWriter.WriteLine($"SF:{file.Path}");

                            foreach (var codeElement in file.CodeElements)
                            {
                                // FN:<line number of function start>,<function name>
                                reportTextWriter.WriteLine($"FN:{codeElement.FirstLine.ToString(CultureInfo.InvariantCulture)},{codeElement.Name}");
                            }

                            foreach (var codeElement in file.CodeElements)
                            {
                                if (file.LineCoverage.Count > codeElement.FirstLine)
                                {
                                    // FNDA:<execution count>,<function name>
                                    int coverage = file.LineCoverage[codeElement.FirstLine];

                                    if (coverage < 0)
                                    {
                                        coverage = 0;
                                    }

                                    reportTextWriter.WriteLine($"FNDA:{coverage.ToString(CultureInfo.InvariantCulture)},{codeElement.Name}");
                                }
                            }

                            // FNF:<number of functions found>
                            reportTextWriter.WriteLine($"FNF:{file.TotalCodeElements.ToString(CultureInfo.InvariantCulture)}");

                            // FNH:<number of function hit>
                            reportTextWriter.WriteLine($"FNH:{file.CoveredCodeElements.ToString(CultureInfo.InvariantCulture)}");

                            foreach (var branchesOfLine in file.BranchesByLine)
                            {
                                foreach (var branch in branchesOfLine.Value)
                                {
                                    string visits = branch.BranchVisits > 0 ? branch.BranchVisits.ToString(CultureInfo.InvariantCulture) : "-";

                                    // BRDA:<line number>,<block number>,<branch number>,<taken>
                                    reportTextWriter.WriteLine($"BRDA:{branchesOfLine.Key.ToString(CultureInfo.InvariantCulture)},{branchesOfLine.Key.ToString(CultureInfo.InvariantCulture)},{branchCounter++.ToString(CultureInfo.InvariantCulture)},{visits}");
                                }
                            }

                            // BRF:<number of branches found>
                            reportTextWriter.WriteLine($"BRF:{file.TotalBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)}");

                            // BRH:<number of branches hit>
                            reportTextWriter.WriteLine($"BRH:{file.CoveredBranches.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)}");

                            int coveredLines = 0;
                            int instrumentedLines = 0;

                            for (int i = 1; i < file.LineCoverage.Count; i++)
                            {
                                int coverage = file.LineCoverage[i];

                                if (coverage >= 0)
                                {
                                    // DA:<line number>,<execution count>[,<checksum>]
                                    reportTextWriter.WriteLine($"DA:{i.ToString(CultureInfo.InvariantCulture)},{coverage.ToString(CultureInfo.InvariantCulture)}");

                                    instrumentedLines++;

                                    if (coverage > 0)
                                    {
                                        coveredLines++;
                                    }
                                }
                            }

                            // LH:<number of lines with a non-zero execution count>
                            reportTextWriter.WriteLine($"LH:{coveredLines.ToString(CultureInfo.InvariantCulture)}");

                            // LF:<number of instrumented lines>
                            reportTextWriter.WriteLine($"LF:{instrumentedLines.ToString(CultureInfo.InvariantCulture)}");

                            reportTextWriter.WriteLine("end_of_record");
                        }
                    }
                }

                reportTextWriter.Flush();
            }
        }
    }
}
