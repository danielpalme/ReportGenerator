using System;
using System.Xml.Linq;

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
        /// Parses the given XML report.
        /// </summary>
        /// <param name="report">The XML report</param>
        /// <returns>The parser result.</returns>
        public abstract ParserResult Parse(XContainer report);

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString() => this.GetType().Name;
    }
}
