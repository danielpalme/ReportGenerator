using System;
using Palmmedia.ReportGenerator.Core.Common;

namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Coverage information of a <see cref="Class"/> of a specific date.
    /// </summary>
    public class HistoricCoverage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HistoricCoverage" /> class.
        /// </summary>
        /// <param name="executionTime">The execution time.</param>
        /// <param name="tag">The custom tag (e.g. build number).</param>
        public HistoricCoverage(DateTime executionTime, string tag)
        {
            this.ExecutionTime = executionTime;
            this.Tag = tag;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoricCoverage" /> class.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="executionTime">The execution time.</param>
        /// <param name="tag">The custom tag (e.g. build number).</param>
        public HistoricCoverage(Class @class, DateTime executionTime, string tag)
            : this(executionTime, tag)
        {
            if (@class == null)
            {
                throw new ArgumentNullException(nameof(@class));
            }

            this.CoveredLines = @class.CoveredLines;
            this.CoverableLines = @class.CoverableLines;
            this.TotalLines = @class.TotalLines.GetValueOrDefault();
            this.CoveredBranches = @class.CoveredBranches.GetValueOrDefault();
            this.TotalBranches = @class.TotalBranches.GetValueOrDefault();
            this.CoveredCodeElements = @class.CoveredCodeElements;
            this.FullCoveredCodeElements = @class.FullCoveredCodeElements;
            this.TotalCodeElements = @class.TotalCodeElements;
        }

        /// <summary>
        /// Gets the execution time.
        /// </summary>
        /// <value>
        /// The execution time.
        /// </value>
        public DateTime ExecutionTime { get; }

        /// <summary>
        /// Gets the custom tag (e.g. build number).
        /// </summary>
        /// <value>
        /// The custom tag.
        /// </value>
        public string Tag { get; }

        /// <summary>
        /// Gets or sets the number of covered lines.
        /// </summary>
        /// <value>The covered lines.</value>
        public int CoveredLines { get; set; }

        /// <summary>
        /// Gets or sets the number of coverable lines.
        /// </summary>
        /// <value>The coverable lines.</value>
        public int CoverableLines { get; set; }

        /// <summary>
        /// Gets the coverage quota of the class.
        /// </summary>
        /// <value>The coverage quota.</value>
        public decimal? CoverageQuota => (this.CoverableLines == 0) ? (decimal?)null : MathExtensions.CalculatePercentage(this.CoveredLines, this.CoverableLines);

        /// <summary>
        /// Gets or sets the number of total lines.
        /// </summary>
        /// <value>The total lines.</value>
        public int TotalLines { get; set; }

        /// <summary>
        /// Gets or sets the number of covered branches.
        /// </summary>
        /// <value>
        /// The number of covered branches.
        /// </value>
        public int CoveredBranches { get; set; }

        /// <summary>
        /// Gets or sets the number of total branches.
        /// </summary>
        /// <value>
        /// The number of total branches.
        /// </value>
        public int TotalBranches { get; set; }

        /// <summary>
        /// Gets the branch coverage quota of the class.
        /// </summary>
        /// <value>The branch coverage quota.</value>
        public decimal? BranchCoverageQuota => (this.TotalBranches == 0) ? (decimal?)null : MathExtensions.CalculatePercentage(this.CoveredBranches, this.TotalBranches);

        /// <summary>
        /// Gets or sets the number of covered code elements.
        /// </summary>
        /// <value>
        /// The number of covered code elements.
        /// </value>
        public int? CoveredCodeElements { get; set; }

        /// <summary>
        /// Gets or sets the number of fully covered code elements.
        /// </summary>
        /// <value>
        /// The number of fully covered code elements.
        /// </value>
        public int? FullCoveredCodeElements { get; set; }

        /// <summary>
        /// Gets or sets the number of total code elements.
        /// </summary>
        /// <value>
        /// The number of total branches.
        /// </value>
        public int? TotalCodeElements { get; set; }

        /// <summary>
        /// Gets the code elements coverage quota.
        /// </summary>
        /// <value>The code elements coverage quota.</value>
        public decimal? CodeElementCoverageQuota => (this.TotalCodeElements.GetValueOrDefault() == 0) ? (decimal?)null : MathExtensions.CalculatePercentage(this.CoveredCodeElements.GetValueOrDefault(), this.TotalCodeElements.GetValueOrDefault());

        /// <summary>
        /// Gets the full code elements coverage quota.
        /// </summary>
        /// <value>The full code elements coverage quota.</value>
        public decimal? FullCodeElementCoverageQuota => (this.TotalCodeElements.GetValueOrDefault() == 0) ? (decimal?)null : MathExtensions.CalculatePercentage(this.FullCoveredCodeElements.GetValueOrDefault(), this.TotalCodeElements.GetValueOrDefault());

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !obj.GetType().Equals(typeof(HistoricCoverage)))
            {
                return false;
            }
            else
            {
                var historicCoverage = (HistoricCoverage)obj;
                return historicCoverage.CoveredLines == this.CoveredLines
                    && historicCoverage.CoverableLines == this.CoverableLines
                    && historicCoverage.TotalLines == this.TotalLines
                    && historicCoverage.CoveredBranches == this.CoveredBranches
                    && historicCoverage.TotalBranches == this.TotalBranches
                    && historicCoverage.CoveredCodeElements == this.CoveredCodeElements
                    && historicCoverage.TotalCodeElements == this.TotalCodeElements;
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => this.CoveredLines
                + this.CoverableLines
                + this.TotalLines
                + this.CoveredBranches
                + this.TotalBranches
                + this.CoveredCodeElements.GetValueOrDefault()
                + this.TotalCodeElements.GetValueOrDefault();
    }
}
