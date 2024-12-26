using System;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Validates the coverage thresholds.
    /// </summary>
    public class MinimumCoverageThresholdsValidator
    {
        /// <summary>
        /// The minimum coverage thresholds.
        /// </summary>
        private readonly MinimumCoverageThresholds minimumCoverageThresholds;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinimumCoverageThresholdsValidator" /> class.
        /// </summary>
        /// <param name="minimumCoverageThresholds">The minimum coverage thresholds.</param>
        public MinimumCoverageThresholdsValidator(MinimumCoverageThresholds minimumCoverageThresholds)
        {
            if (minimumCoverageThresholds == null)
            {
                throw new ArgumentNullException(nameof(minimumCoverageThresholds));
            }

            this.minimumCoverageThresholds = minimumCoverageThresholds;
        }

        /// <summary>
        /// Validates the coverage thresholds.
        /// </summary>
        /// <param name="parserResult">The parser result.</param>
        public void Validate(ParserResult parserResult)
        {
            if (!this.minimumCoverageThresholds.LineCoverage.HasValue
                && !this.minimumCoverageThresholds.BranchCoverage.HasValue
                && !this.minimumCoverageThresholds.MethodCoverage.HasValue)
            {
                return;
            }

            var errors = new List<string>();

            var summaryResult = new SummaryResult(parserResult);

            if (this.minimumCoverageThresholds.LineCoverage.HasValue
                && summaryResult.CoverageQuota.HasValue
                && summaryResult.CoverageQuota.Value < this.minimumCoverageThresholds.LineCoverage.Value)
            {
                errors.Add(string.Format(Resources.ErrorLowLineCoverage, summaryResult.CoverageQuota.Value, this.minimumCoverageThresholds.LineCoverage.Value));
            }

            if (this.minimumCoverageThresholds.BranchCoverage.HasValue
                && summaryResult.BranchCoverageQuota.HasValue
                && summaryResult.BranchCoverageQuota.Value < this.minimumCoverageThresholds.BranchCoverage.Value)
            {
                errors.Add(string.Format(Resources.ErrorLowBranchCoverage, summaryResult.BranchCoverageQuota.Value, this.minimumCoverageThresholds.BranchCoverage.Value));
            }

            if (this.minimumCoverageThresholds.MethodCoverage.HasValue
                && summaryResult.CodeElementCoverageQuota.HasValue
                && summaryResult.CodeElementCoverageQuota < this.minimumCoverageThresholds.MethodCoverage.Value)
            {
                errors.Add(string.Format(Resources.ErrorLowMethodCoverage, summaryResult.CodeElementCoverageQuota.Value, this.minimumCoverageThresholds.MethodCoverage.Value));
            }

            if (this.minimumCoverageThresholds.FullMethodCoverage.HasValue
                && summaryResult.FullCodeElementCoverageQuota.HasValue
                && summaryResult.FullCodeElementCoverageQuota < this.minimumCoverageThresholds.FullMethodCoverage.Value)
            {
                errors.Add(string.Format(Resources.ErrorLowFullMethodCoverage, summaryResult.FullCodeElementCoverageQuota.Value, this.minimumCoverageThresholds.FullMethodCoverage.Value));
            }

            if (errors.Count > 0)
            {
                throw new LowCoverageException(string.Join("\r\n", errors));
            }
        }
    }
}