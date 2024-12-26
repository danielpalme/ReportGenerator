using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Common;

namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Represents one assembly.
    /// </summary>
    public class Assembly
    {
        /// <summary>
        /// List of classes in assembly.
        /// </summary>
        private readonly ConcurrentBag<Class> classes = new ConcurrentBag<Class>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Assembly"/> class.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        internal Assembly(string name)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Gets the list of classes in assembly.
        /// </summary>
        public IEnumerable<Class> Classes => this.classes.OrderBy(c => c.Name);

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the short name of the assembly.
        /// </summary>
        /// <value>The short name of the assembly.</value>
        public string ShortName
        {
            get
            {
                string shortName = this.Name.Replace("/", "\\");
                return shortName.Substring(shortName.LastIndexOf('\\') + 1);
            }
        }

        /// <summary>
        /// Gets the number of covered lines.
        /// </summary>
        /// <value>The covered lines.</value>
        public int CoveredLines => this.classes.SafeSum(c => c.CoveredLines);

        /// <summary>
        /// Gets the number of coverable lines.
        /// </summary>
        /// <value>The coverable lines.</value>
        public int CoverableLines => this.classes.SafeSum(c => c.CoverableLines);

        /// <summary>
        /// Gets the number of total lines.
        /// </summary>
        /// <value>The total lines.</value>
        public int? TotalLines => this.classes.SafeSum(c => c.TotalLines);

        /// <summary>
        /// Gets the coverage quota of the class.
        /// </summary>
        /// <value>The coverage quota.</value>
        public decimal? CoverageQuota => (this.CoverableLines == 0) ? (decimal?)null : MathExtensions.CalculatePercentage(this.CoveredLines, this.CoverableLines);

        /// <summary>
        /// Gets the number of covered branches.
        /// </summary>
        /// <value>
        /// The number of covered branches.
        /// </value>
        public int? CoveredBranches => this.classes.SafeSum(f => f.CoveredBranches);

        /// <summary>
        /// Gets the number of total branches.
        /// </summary>
        /// <value>
        /// The number of total branches.
        /// </value>
        public int? TotalBranches => this.classes.SafeSum(f => f.TotalBranches);

        /// <summary>
        /// Gets the branch coverage quota of the class.
        /// </summary>
        /// <value>The branch coverage quota.</value>
        public decimal? BranchCoverageQuota => (this.TotalBranches == 0) ? (decimal?)null : MathExtensions.CalculatePercentage(this.CoveredBranches.GetValueOrDefault(), this.TotalBranches.GetValueOrDefault());

        /// <summary>
        /// Gets the number of covered code elements.
        /// </summary>
        /// <value>
        /// The number of covered code elements.
        /// </value>
        public int CoveredCodeElements => this.classes.SafeSum(f => f.CoveredCodeElements);

        /// <summary>
        /// Gets the number of fully covered code elements.
        /// </summary>
        /// <value>
        /// The number of fully covered code elements.
        /// </value>
        public int FullCoveredCodeElements => this.classes.SafeSum(f => f.FullCoveredCodeElements);

        /// <summary>
        /// Gets the number of total code elements.
        /// </summary>
        /// <value>
        /// The number of total code elements.
        /// </value>
        public int TotalCodeElements => this.classes.SafeSum(f => f.TotalCodeElements);

        /// <summary>
        /// Gets the code elements coverage quota.
        /// </summary>
        /// <value>The code elements coverage quota.</value>
        public decimal? CodeElementCoverageQuota => (this.TotalCodeElements == 0) ? (decimal?)null : MathExtensions.CalculatePercentage(this.CoveredCodeElements, this.TotalCodeElements);

        /// <summary>
        /// Gets the full code elements coverage quota.
        /// </summary>
        /// <value>The full code elements coverage quota.</value>
        public decimal? FullCodeElementCoverageQuota => (this.TotalCodeElements == 0) ? (decimal?)null : MathExtensions.CalculatePercentage(this.FullCoveredCodeElements, this.TotalCodeElements);

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !obj.GetType().Equals(typeof(Assembly)))
            {
                return false;
            }
            else
            {
                var assembly = (Assembly)obj;
                return assembly.Name.Equals(this.Name);
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
        /// Adds the given class to the assembly.
        /// </summary>
        /// <param name="class">The class to add.</param>
        internal void AddClass(Class @class)
        {
            this.classes.Add(@class);
        }

        /// <summary>
        /// Merges the given assembly with the current instance.
        /// </summary>
        /// <param name="assembly">The assembly to merge.</param>
        internal void Merge(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            foreach (var @class in assembly.classes)
            {
                var existingClass = this.classes.FirstOrDefault(c => c.Equals(@class));

                if (existingClass != null)
                {
                    existingClass.Merge(@class);
                }
                else
                {
                    this.AddClass(@class);
                    @class.Assembly = this;
                }
            }
        }
    }
}
