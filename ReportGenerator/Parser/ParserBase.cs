using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Parser
{
    /// <summary>
    /// Base class for the <see cref="IParser"/> implementations.
    /// </summary>
    internal abstract class ParserBase : IParser
    {
        /// <summary>
        /// The cyclomatic complexity URI.
        /// </summary>
        protected static readonly Uri CyclomaticComplexityUri = new Uri("https://en.wikipedia.org/wiki/Cyclomatic_complexity");

        /// <summary>
        /// The code coverage URI.
        /// </summary>
        protected static readonly Uri CodeCoverageUri = new Uri("https://en.wikipedia.org/wiki/Code_coverage");

        /// <summary>
        /// The n path complexity URI.
        /// </summary>
        protected static readonly Uri NPathComplexityUri = new Uri("https://modess.io/npath-complexity-cyclomatic-complexity-explained");

        /// <summary>
        /// The crap score URI.
        /// </summary>
        protected static readonly Uri CrapScoreUri = new Uri("https://googletesting.blogspot.de/2011/02/this-code-is-crap.html");

        /// <summary>
        /// The assemblies found in the report.
        /// </summary>
        private ConcurrentBag<Assembly> assemblies = new ConcurrentBag<Assembly>();

        /// <summary>
        /// Gets the assemblies that have been found in the report.
        /// </summary>
        /// <value>The assemblies.</value>
        public IEnumerable<Assembly> Assemblies => this.assemblies.OrderBy(a => a.Name);

        /// <summary>
        /// Gets a value indicating whether the used parser supports branch coverage.
        /// </summary>
        /// <value>
        /// <c>true</c> if used parser supports branch coverage; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SupportsBranchCoverage => false;

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString() => this.GetType().Name;

        /// <summary>
        /// Adds the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        protected internal void AddAssembly(Assembly assembly)
        {
            this.assemblies.Add(assembly);
        }
    }
}
