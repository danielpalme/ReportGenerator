using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Palmmedia.ReportGenerator.Parser.Analysis
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
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            this.Name = name;
        }

        /// <summary>
        /// Gets the list of classes in assembly.
        /// </summary>
        public IEnumerable<Class> Classes
        {
            get
            {
                return this.classes.OrderBy(c => c.Name);
            }
        }

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        public string Name { get; private set; }

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
        public int CoveredLines
        {
            get
            {
                return this.classes.Sum(c => c.CoveredLines);
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
                return this.classes.Sum(c => c.CoverableLines);
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
                return this.classes.Sum(c => c.TotalLines);
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
                return this.classes.Sum(f => f.CoveredBranches);
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
                return this.classes.Sum(f => f.TotalBranches);
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

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
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
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

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
                throw new ArgumentNullException("assembly");
            }

            foreach (var @class in assembly.classes)
            {
                var existingClass = this.classes.FirstOrDefault(c => c.Name == @class.Name);

                if (existingClass != null)
                {
                    existingClass.Merge(@class);
                }
                else
                {
                    this.AddClass(@class);
                }
            }
        }
    }
}
