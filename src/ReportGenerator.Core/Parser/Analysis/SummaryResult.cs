using System;
using System.Collections.Generic;
using System.Linq;

namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
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
        /// <param name="supportsBranchCoverage">if set to <c>true</c> the used parser supports branch coverage.</param>
        /// <param name="sourceDirectories">The source directories.</param>
        internal SummaryResult(IReadOnlyCollection<Assembly> assemblies, string usedParser, bool supportsBranchCoverage, IReadOnlyCollection<string> sourceDirectories)
        {
            this.Assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
            this.UsedParser = usedParser ?? throw new ArgumentNullException(nameof(usedParser));
            this.SupportsBranchCoverage = supportsBranchCoverage;
            this.SourceDirectories = sourceDirectories ?? throw new ArgumentNullException(nameof(sourceDirectories));
        }

        /// <summary>
        /// Gets the assemblies.
        /// </summary>
        /// <value>
        /// The assemblies.
        /// </value>
        public IReadOnlyCollection<Assembly> Assemblies { get; }

        /// <summary>
        /// Gets the used parser.
        /// </summary>
        /// <value>
        /// The used parser.
        /// </value>
        public string UsedParser { get; }

        /// <summary>
        /// Gets a value indicating whether the used parser supports branch coverage.
        /// </summary>
        /// <value>
        /// <c>true</c> if used parser supports branch coverage; otherwise, <c>false</c>.
        /// </value>
        public bool SupportsBranchCoverage { get; }

        /// <summary>
        /// Gets the source directories.
        /// </summary>
        /// <value>
        /// The source directories.
        /// </value>
        public IReadOnlyCollection<string> SourceDirectories { get; }

        /// <summary>
        /// Gets the number of covered lines.
        /// </summary>
        /// <value>The covered lines.</value>
        public int CoveredLines => this.Assemblies.Sum(a => a.CoveredLines);

        /// <summary>
        /// Gets the number of coverable lines.
        /// </summary>
        /// <value>The coverable lines.</value>
        public int CoverableLines => this.Assemblies.Sum(a => a.CoverableLines);

        /// <summary>
        /// Gets the number of total lines.
        /// </summary>
        /// <value>The total lines.</value>
        public int? TotalLines
        {
            get
            {
                var processedFiles = new HashSet<string>();
                int? result = null;

                foreach (var assembly in this.Assemblies)
                {
                    foreach (var clazz in assembly.Classes)
                    {
                        foreach (var file in clazz.Files)
                        {
                            if (!processedFiles.Contains(file.Path) && file.TotalLines.HasValue)
                            {
                                processedFiles.Add(file.Path);
                                result = result.HasValue ? result + file.TotalLines : file.TotalLines;
                            }
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the coverage quota.
        /// </summary>
        /// <value>The coverage quota.</value>
        public decimal? CoverageQuota => (this.CoverableLines == 0) ? (decimal?)null : (decimal)Math.Truncate(1000 * (double)this.CoveredLines / (double)this.CoverableLines) / 10;

        /// <summary>
        /// Gets the number of covered branches.
        /// </summary>
        /// <value>
        /// The number of covered branches.
        /// </value>
        public int? CoveredBranches => this.Assemblies.Sum(f => f.CoveredBranches);

        /// <summary>
        /// Gets the number of total branches.
        /// </summary>
        /// <value>
        /// The number of total branches.
        /// </value>
        public int? TotalBranches => this.Assemblies.Sum(f => f.TotalBranches);

        /// <summary>
        /// Gets the branch coverage quota.
        /// </summary>
        /// <value>The branch coverage quota.</value>
        public decimal? BranchCoverageQuota => (this.TotalBranches == 0) ? (decimal?)null : (decimal)Math.Truncate(1000 * (double)this.CoveredBranches / (double)this.TotalBranches) / 10;
    }
}
