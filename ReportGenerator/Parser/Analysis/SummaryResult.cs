using System;
using System.Collections.Generic;
using System.Linq;

namespace Palmmedia.ReportGenerator.Parser.Analysis
{
    /// <summary>
    /// Overall result of all assemblies.
    /// </summary>
    public class SummaryResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryResult" /> class.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="usedParser">The used parser.</param>
        internal SummaryResult(IEnumerable<Assembly> assemblies, string usedParser)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException("assemblies");
            }

            if (usedParser == null)
            {
                throw new ArgumentNullException("usedParser");
            }

            this.Assemblies = assemblies;
            this.UsedParser = usedParser;
        }

        /// <summary>
        /// Gets the assemblies.
        /// </summary>
        /// <value>
        /// The assemblies.
        /// </value>
        public IEnumerable<Assembly> Assemblies { get; private set; }

        /// <summary>
        /// Gets the used parser.
        /// </summary>
        /// <value>
        /// The used parser.
        /// </value>
        public string UsedParser { get; private set; }

        /// <summary>
        /// Gets the number of covered lines.
        /// </summary>
        /// <value>The covered lines.</value>
        public int CoveredLines
        {
            get
            {
                return this.Assemblies.Sum(a => a.CoveredLines);
            }
        }

        /// <summary>
        /// Gets the number of coverable lines.
        /// </summary>
        /// <value>The coverable lines.</value>
        public int CoverableLines
        {
            get
            {
                return this.Assemblies.Sum(a => a.CoverableLines);
            }
        }

        /// <summary>
        /// Gets the number of total lines.
        /// </summary>
        /// <value>The total lines.</value>
        public int? TotalLines
        {
            get
            {
                return this.Assemblies.Sum(a => a.TotalLines);
            }
        }

        /// <summary>
        /// Gets the coverage quota of the class.
        /// </summary>
        /// <value>The coverage quota.</value>
        public decimal? CoverageQuota
        {
            get
            {
                return (this.CoverableLines == 0) ? (decimal?)null : (decimal)Math.Truncate(1000 * (double)this.CoveredLines / (double)this.CoverableLines) / 10;
            }
        }
        
        /// <summary>
        /// Gets the number of covered branches.
        /// </summary>
        /// <value>
        /// The number of covered branches.
        /// </value>
        public int? CoveredBranches
        {
            get
            {
                return this.Assemblies.Sum(f => f.CoveredBranches);
            }
        }

        /// <summary>
        /// Gets the number of total branches.
        /// </summary>
        /// <value>
        /// The number of total branches.
        /// </value>
        public int? TotalBranches
        {
            get
            {
                return this.Assemblies.Sum(f => f.TotalBranches);
            }
        }

        /// <summary>
        /// Gets the branch coverage quota of the class.
        /// </summary>
        /// <value>The branch coverage quota.</value>
        public decimal? BranchCoverageQuota
        {
            get
            {
                return (this.TotalBranches == 0) ? (decimal?)null : (decimal)Math.Truncate(1000 * (double)this.CoveredBranches / (double)this.TotalBranches) / 10;
            }
        }
    }
}
