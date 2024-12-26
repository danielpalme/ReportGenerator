using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Common;

namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Helper class to operator on <see cref="HistoricCoverage"/>.
    /// </summary>
    internal static class HistoricCoverages
    {
        /// <summary>
        /// Gets the overall historic coverages from all classes grouped by execution time.
        /// </summary>
        /// <param name="overallHistoricCoverages">All historic coverage elements.</param>
        /// <returns>
        /// The overall historic coverages from all classes grouped by execution time.
        /// </returns>
        public static IEnumerable<HistoricCoverage> GetOverallHistoricCoverages(IEnumerable<HistoricCoverage> overallHistoricCoverages)
        {
            var executionTimes = overallHistoricCoverages
                .Select(h => h.ExecutionTime)
                .Distinct()
                .OrderBy(e => e);

            var result = new List<HistoricCoverage>();

            foreach (var executionTime in executionTimes)
            {
                var historicCoveragesOfExecutionTime = overallHistoricCoverages
                    .Where(h => h.ExecutionTime.Equals(executionTime))
                    .ToArray();

                result.Add(new HistoricCoverage(executionTime, historicCoveragesOfExecutionTime[0].Tag)
                {
                    CoveredLines = historicCoveragesOfExecutionTime.SafeSum(h => h.CoveredLines),
                    CoverableLines = historicCoveragesOfExecutionTime.SafeSum(h => h.CoverableLines),
                    CoveredBranches = historicCoveragesOfExecutionTime.SafeSum(h => h.CoveredBranches),
                    TotalBranches = historicCoveragesOfExecutionTime.SafeSum(h => h.TotalBranches),
                    TotalLines = historicCoveragesOfExecutionTime.SafeSum(h => h.TotalLines),
                    CoveredCodeElements = historicCoveragesOfExecutionTime.SafeSum(h => h.CoveredCodeElements),
                    FullCoveredCodeElements = historicCoveragesOfExecutionTime.SafeSum(h => h.FullCoveredCodeElements),
                    TotalCodeElements = historicCoveragesOfExecutionTime.SafeSum(h => h.TotalCodeElements)
                });
            }

            return result;
        }
    }
}
