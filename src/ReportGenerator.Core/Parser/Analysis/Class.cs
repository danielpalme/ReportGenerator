using System;
using System.Collections.Generic;
using System.Linq;

namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Represents a class.
    /// </summary>
    public class Class
    {
        /// <summary>
        /// List of files that define this class.
        /// </summary>
        private readonly List<CodeFile> files = new List<CodeFile>();

        /// <summary>
        /// List of historic coverage information.
        /// </summary>
        private readonly List<HistoricCoverage> historicCoverages = new List<HistoricCoverage>();

        /// <summary>
        /// The coverage quota.
        /// </summary>
        private decimal? coverageQuota;

        /// <summary>
        /// Initializes a new instance of the <see cref="Class"/> class.
        /// </summary>
        /// <param name="name">The name of the class.</param>
        /// <param name="assembly">The assembly.</param>
        internal Class(string name, Assembly assembly)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        /// <summary>
        /// Gets the name of the class.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the assembly.
        /// </summary>
        /// <value>The assembly.</value>
        public Assembly Assembly { get; internal set; }

        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <value>The files.</value>
        public IEnumerable<CodeFile> Files => this.files.OrderBy(f => f.Path);

        /// <summary>
        /// Gets the historic coverage information.
        /// </summary>
        /// <value>The historic coverage information.</value>
        public IEnumerable<HistoricCoverage> HistoricCoverages => this.historicCoverages;

        /// <summary>
        /// Gets the coverage type.
        /// </summary>
        /// <value>The coverage type.</value>
        public CoverageType CoverageType => this.files.Count == 0 ? CoverageType.MethodCoverage : CoverageType.LineCoverage;

        /// <summary>
        /// Gets the number of covered lines.
        /// </summary>
        /// <value>The covered lines.</value>
        public int CoveredLines => this.files.Sum(f => f.CoveredLines);

        /// <summary>
        /// Gets the number of coverable lines.
        /// </summary>
        /// <value>The coverable lines.</value>
        public int CoverableLines => this.files.Sum(f => f.CoverableLines);

        /// <summary>
        /// Gets the number of total lines.
        /// </summary>
        /// <value>The total lines.</value>
        public int? TotalLines => this.files.Sum(f => f.TotalLines);

        /// <summary>
        /// Gets or sets the coverage quota of the class.
        /// </summary>
        /// <value>The coverage quota.</value>
        public decimal? CoverageQuota
        {
            get
            {
                if (this.files.Count == 0)
                {
                    return this.coverageQuota;
                }
                else
                {
                    return (this.CoverableLines == 0) ? (decimal?)null : (decimal)Math.Truncate(1000 * (double)this.CoveredLines / (double)this.CoverableLines) / 10;
                }
            }

            set
            {
                this.coverageQuota = value;
            }
        }

        /// <summary>
        /// Gets the number of covered branches.
        /// </summary>
        /// <value>
        /// The number of covered branches.
        /// </value>
        public int? CoveredBranches => this.files.Sum(f => f.CoveredBranches);

        /// <summary>
        /// Gets the number of total branches.
        /// </summary>
        /// <value>
        /// The number of total branches.
        /// </value>
        public int? TotalBranches => this.files.Sum(f => f.TotalBranches);

        /// <summary>
        /// Gets the branch coverage quota of the class.
        /// </summary>
        /// <value>The branch coverage quota.</value>
        public decimal? BranchCoverageQuota => (this.TotalBranches == 0) ? (decimal?)null : (decimal)Math.Truncate(1000 * (double)this.CoveredBranches / (double)this.TotalBranches) / 10;

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !obj.GetType().Equals(typeof(Class)))
            {
                return false;
            }
            else
            {
                var @class = (Class)obj;
                return @class.Name.Equals(this.Name) && @class.Assembly.Equals(this.Assembly);
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => this.Name.GetHashCode();

        /// <summary>
        /// Adds the given file.
        /// </summary>
        /// <param name="codeFile">The code file.</param>
        internal void AddFile(CodeFile codeFile)
        {
            this.files.Add(codeFile);
        }

        /// <summary>
        /// Adds the given historic coverage.
        /// </summary>
        /// <param name="historicCoverage">The historic coverage.</param>
        internal void AddHistoricCoverage(HistoricCoverage historicCoverage)
        {
            this.historicCoverages.Add(historicCoverage);
        }

        /// <summary>
        /// Merges the given class with the current instance.
        /// </summary>
        /// <param name="class">The class to merge.</param>
        internal void Merge(Class @class)
        {
            if (@class == null)
            {
                throw new ArgumentNullException(nameof(@class));
            }

            if (this.coverageQuota.HasValue && @class.coverageQuota.HasValue)
            {
                this.CoverageQuota = Math.Max(this.coverageQuota.Value, @class.coverageQuota.Value);
            }
            else if (@class.coverageQuota.HasValue)
            {
                this.CoverageQuota = @class.coverageQuota.Value;
            }

            foreach (var file in @class.files)
            {
                var existingFile = this.files.FirstOrDefault(f => f.Equals(file));
                if (existingFile != null)
                {
                    existingFile.Merge(file);
                }
                else
                {
                    this.AddFile(file);
                }
            }
        }
    }
}
