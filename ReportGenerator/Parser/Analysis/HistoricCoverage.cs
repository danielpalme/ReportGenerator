using System;

namespace Palmmedia.ReportGenerator.Parser.Analysis
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
        public HistoricCoverage(DateTime executionTime)
        {
            this.ExecutionTime = executionTime;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoricCoverage" /> class.
        /// </summary>
        /// <param name="class">The class.</param>
        /// <param name="executionTime">The execution time.</param>
        public HistoricCoverage(Class @class, DateTime executionTime)
            : this(executionTime)
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
        }

        /// <summary>
        /// Gets the execution time.
        /// </summary>
        /// <value>
        /// The execution time.
        /// </value>
        public DateTime ExecutionTime { get; }

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
        public decimal? CoverageQuota => (this.CoverableLines == 0) ? (decimal?)null : (decimal)Math.Truncate(1000 * (double)this.CoveredLines / (double)this.CoverableLines) / 10;

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
        public decimal? BranchCoverageQuota => (this.TotalBranches == 0) ? (decimal?)null : (decimal)Math.Truncate(1000 * (double)this.CoveredBranches / (double)this.TotalBranches) / 10;

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
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
                    && historicCoverage.TotalBranches == this.TotalBranches;
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
                + this.TotalBranches;
    }
}
