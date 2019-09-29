using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
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

            string targetPath = Path.Combine(this.ReportContext.ReportConfiguration.TargetDirectory, "lcov.info");

            Logger.InfoFormat("  " + Resources.WritingReportFile, targetPath);

            using (var reportTextWriter = new StreamWriter(new FileStream(targetPath, FileMode.Create), Encoding.UTF8))
            {
                reportTextWriter.WriteLine("TN:");

                foreach (var assembly in summaryResult.Assemblies)
                {
                    foreach (var @class in assembly.Classes)
                    {
                        foreach (var file in @class.Files)
                        {
                            reportTextWriter.WriteLine($"SF:{file.Path}");

                            foreach (var codeElement in file.CodeElements)
                            {
                                reportTextWriter.WriteLine($"FN:{codeElement.FirstLine.ToString(CultureInfo.InvariantCulture)},{codeElement.Name}");
                            }

                            int coveredLines = 0;
                            int instrumentedLines = 0;

                            for (int i = 1; i < file.LineCoverage.Count; i++)
                            {
                                int coverage = file.LineCoverage[i];

                                if (coverage >= 0)
                                {
                                    reportTextWriter.WriteLine($"DA:{i.ToString(CultureInfo.InvariantCulture)},{coverage.ToString(CultureInfo.InvariantCulture)}");

                                    instrumentedLines++;

                                    if (coverage > 0)
                                    {
                                        coveredLines++;
                                    }
                                }
                            }

                            // LF:<number of instrumented lines>
                            reportTextWriter.WriteLine($"LF:{instrumentedLines.ToString(CultureInfo.InvariantCulture)}");

                            // LH:<number of lines with a non-zero execution count>
                            reportTextWriter.WriteLine($"LH:{coveredLines.ToString(CultureInfo.InvariantCulture)}");

                            reportTextWriter.WriteLine("end_of_record");
                        }
                    }
                }

                reportTextWriter.Flush();
            }
        }
    }
}
