using System;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;

namespace Palmmedia.ReportGenerator.Core.Parser
{
    /// <summary>
    /// Base class for the parser implementations.
    /// </summary>
    internal abstract class ParserBase
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
        /// Initializes a new instance of the <see cref="ParserBase" /> class.
        /// </summary>
        /// <param name="assemblyFilter">The assembly filter.</param>
        /// <param name="namespaceFilter">The namespace filter.</param>
        /// <param name="classFilter">The class filter.</param>
        /// <param name="fileFilter">The file filter.</param>
        protected ParserBase(IFilter assemblyFilter, IFilter namespaceFilter, IFilter classFilter, IFilter fileFilter)
        {
            this.AssemblyFilter = assemblyFilter ?? throw new ArgumentNullException(nameof(assemblyFilter));
            this.NamespaceFilter = namespaceFilter ?? throw new ArgumentNullException(nameof(namespaceFilter));
            this.ClassFilter = classFilter ?? throw new ArgumentNullException(nameof(classFilter));
            this.FileFilter = fileFilter ?? throw new ArgumentNullException(nameof(fileFilter));
        }

        /// <summary>
        /// Gets the assembly filter.
        /// </summary>
        protected IFilter AssemblyFilter { get; }

        /// <summary>
        /// Gets the namespace filter.
        /// </summary>
        protected IFilter NamespaceFilter { get; }

        /// <summary>
        /// Gets the class filter.
        /// </summary>
        protected IFilter ClassFilter { get; }

        /// <summary>
        /// Gets the file filter.
        /// </summary>
        protected IFilter FileFilter { get; }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString() => this.GetType().Name;
    }
}
