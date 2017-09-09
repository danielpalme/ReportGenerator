using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Palmmedia.ReportGenerator.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Parser
{
    /// <summary>
    /// Parser that aggregates several parsers.
    /// </summary>
    internal class MultiReportParser : ParserBase
    {
        /// <summary>
        /// The names of the aggregated parsers.
        /// </summary>
        private readonly List<string> parserNames = new List<string>();

        /// <summary>
        /// Indicates whether the used parsers supports branch coverage.
        /// </summary>
        private bool supportsBranchCoverage = false;

        /// <summary>
        /// Gets a value indicating whether the used parser supports branch coverage.
        /// </summary>
        /// <value>
        /// <c>true</c> if used parser supports branch coverage; otherwise, <c>false</c>.
        /// </value>
        public override bool SupportsBranchCoverage => this.supportsBranchCoverage;

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (this.parserNames.Count == 0)
            {
                return string.Empty;
            }
            else if (this.parserNames.Count == 1)
            {
                return this.parserNames[0];
            }
            else
            {
                StringBuilder sb = new StringBuilder(this.GetType().Name);
                sb.Append(" (");

                var groupedParsers = this.parserNames.GroupBy(p => p).OrderBy(pg => pg.Key);

                sb.Append(string.Join(
                    ", ",
                    groupedParsers.Select(pg => string.Format(CultureInfo.InvariantCulture, "{0}x {1}", pg.Count(), pg.Key))));

                sb.Append(")");
                return sb.ToString();
            }
        }

        /// <summary>
        /// Adds the parser.
        /// </summary>
        /// <param name="parser">The parser to add.</param>
        internal void AddParser(IParser parser)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            this.parserNames.Add(parser.ToString());
            this.supportsBranchCoverage |= parser.SupportsBranchCoverage;

            this.MergeAssemblies(parser.Assemblies);
        }

        /// <summary>
        /// Merges the given assemblies with the existing assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies to merge.</param>
        private void MergeAssemblies(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var existingAssembly = this.Assemblies.FirstOrDefault(a => a.Name == assembly.Name);

                if (existingAssembly != null)
                {
                    existingAssembly.Merge(assembly);
                }
                else
                {
                    this.AddAssembly(assembly);
                }
            }
        }
    }
}
