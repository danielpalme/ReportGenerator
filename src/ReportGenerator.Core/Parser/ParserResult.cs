using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Core.Parser
{
    /// <summary>
    /// The result on a <see cref="ParserBase"/>.
    /// </summary>
    internal class ParserResult
    {
        /// <summary>
        /// The name of the parser or the merged parsers.
        /// </summary>
        private readonly List<string> parserNames;

        /// <summary>
        /// The covered assemblies.
        /// </summary>
        private readonly List<Assembly> assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserResult"/> class.
        /// </summary>
        public ParserResult()
        {
            this.assemblies = new List<Assembly>();
            this.SupportsBranchCoverage = true;
            this.parserNames = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserResult"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="supportsBranchCoverage">Indicates whether the used parser supports branch coverage.</param>
        /// <param name="parserName">The name of the parser.</param>
        public ParserResult(List<Assembly> assemblies, bool supportsBranchCoverage, string parserName)
        {
            if (parserName == null)
            {
                throw new ArgumentNullException(nameof(parserName));
            }

            this.assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
            this.SupportsBranchCoverage = supportsBranchCoverage;
            this.parserNames = new List<string>()
            {
                parserName
            };
        }

        /// <summary>
        /// Gets the assemblies that have been found in the report.
        /// </summary>
        /// <value>The assemblies.</value>
        public IReadOnlyCollection<Assembly> Assemblies => this.assemblies;

        /// <summary>
        /// Gets a value indicating whether the used parser supports branch coverage.
        /// </summary>
        /// <value>
        /// <c>true</c> if used parser supports branch coverage; otherwise, <c>false</c>.
        /// </value>
        public bool SupportsBranchCoverage { get; private set; }

        /// <summary>
        /// Gets the names of the parsers.
        /// </summary>
        public string ParserName
        {
            get
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
                    StringBuilder sb = new StringBuilder("MultiReportParser (");

                    var groupedParsers = this.parserNames.GroupBy(p => p).OrderBy(pg => pg.Key);

                    sb.Append(string.Join(
                        ", ",
                        groupedParsers.Select(pg => string.Format(CultureInfo.InvariantCulture, "{0}x {1}", pg.Count(), pg.Key))));

                    sb.Append(")");
                    return sb.ToString();
                }
            }
        }

        /// <summary>
        /// Merges the given parser result with the current instance.
        /// </summary>
        /// <param name="parserResult">The parser result to merge.</param>
        internal void Merge(ParserResult parserResult)
        {
            foreach (var assembly in parserResult.Assemblies)
            {
                var existingAssembly = this.assemblies.FirstOrDefault(a => a.Name == assembly.Name);

                if (existingAssembly != null)
                {
                    existingAssembly.Merge(assembly);
                }
                else
                {
                    this.assemblies.Add(assembly);
                }
            }

            this.assemblies.Sort((x, y) => x.Name.CompareTo(y.Name));

            this.SupportsBranchCoverage |= parserResult.SupportsBranchCoverage;
            this.parserNames.AddRange(parserResult.parserNames);
        }
    }
}