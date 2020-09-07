using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Writes TeamCity statistics messages.
    /// </summary>
    public class TeamCitySummaryReportBuilder : IReportBuilder
    {
        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report format.
        /// </value>
        public string ReportType => "TeamCitySummary";

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
            var allClasses = summaryResult.Assemblies
                .SelectMany(x => x.Classes)
                .Where(x => x.CoverableLines > 0)
                .ToList();

            WriteStatistics('S', summaryResult.CoveredLines, summaryResult.CoverableLines);
            WriteStatistics('R', summaryResult.CoveredBranches.GetValueOrDefault(), summaryResult.TotalBranches.GetValueOrDefault());
            WriteStatistics('C', allClasses.Count(y => y.CoveredLines > 0), allClasses.Count);
            WriteStatistics('M', summaryResult.CoveredCodeElements, summaryResult.TotalCodeElements);
        }

        /// <summary>
        /// Calculate and write teamcity coverage statistics.
        /// </summary>
        /// <param name="type">S - Line, C- Class, B- Branch, M- Method.</param>
        /// <param name="covered">The covered cases.</param>
        /// <param name="total">The total number od cases.</param>
        private static void WriteStatistics(char type, decimal covered, decimal total)
        {
            if (total != 0)
            {
                WriteStatistic($"CodeCoverage{type}", Math.Round(covered / total * 100, 2, MidpointRounding.AwayFromZero));
                WriteStatistic($"CodeCoverageAbs{type}Covered", covered);
                WriteStatistic($"CodeCoverageAbs{type}Total", total);
            }
        }

        /// <summary>
        /// Write teamcity build statistic.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        private static void WriteStatistic(string name, decimal value)
        {
            Console.WriteLine($"##teamcity[buildStatisticValue key='{name}' value='{value.ToString(CultureInfo.InvariantCulture)}']");
        }
    }
}
