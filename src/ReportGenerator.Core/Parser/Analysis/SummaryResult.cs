using System;
using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Common;

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
        /// <param name="parserResult">The parser result.</param>
        public SummaryResult(ParserResult parserResult)
            : this(parserResult.Assemblies, parserResult.ParserName, parserResult.SupportsBranchCoverage, parserResult.SourceDirectories)
        {
            this.MinimumTimeStamp = parserResult.MinimumTimeStamp;
            this.MaximumTimeStamp = parserResult.MaximumTimeStamp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryResult" /> class.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="usedParser">The used parser.</param>
        /// <param name="supportsBranchCoverage">if set to <c>true</c> the used parser supports branch coverage.</param>
        /// <param name="sourceDirectories">The source directories.</param>
        public SummaryResult(IReadOnlyCollection<Assembly> assemblies, string usedParser, bool supportsBranchCoverage, IReadOnlyCollection<string> sourceDirectories)
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
        /// Gets the timestamp on which the coverage report was generated.
        /// </summary>
        public DateTime? MinimumTimeStamp { get; }

        /// <summary>
        /// Gets the timestamp on which the coverage report was generated.
        /// </summary>
        public DateTime? MaximumTimeStamp { get; }

        /// <summary>
        /// Gets the number of covered lines.
        /// </summary>
        /// <value>The covered lines.</value>
        public int CoveredLines => this.Assemblies.SafeSum(a => a.CoveredLines);

        /// <summary>
        /// Gets the number of coverable lines.
        /// </summary>
        /// <value>The coverable lines.</value>
        public int CoverableLines => this.Assemblies.SafeSum(a => a.CoverableLines);

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
        public decimal? CoverageQuota => (this.CoverableLines == 0) ? (decimal?)null : MathExtensions.CalculatePercentage(this.CoveredLines, this.CoverableLines);

        /// <summary>
        /// Gets the number of covered branches.
        /// </summary>
        /// <value>
        /// The number of covered branches.
        /// </value>
        public int? CoveredBranches => this.Assemblies.SafeSum(f => f.CoveredBranches);

        /// <summary>
        /// Gets the number of total branches.
        /// </summary>
        /// <value>
        /// The number of total branches.
        /// </value>
        public int? TotalBranches => this.Assemblies.SafeSum(f => f.TotalBranches);

        /// <summary>
        /// Gets the branch coverage quota.
        /// </summary>
        /// <value>The branch coverage quota.</value>
        public decimal? BranchCoverageQuota => (this.TotalBranches == 0) ? (decimal?)null : MathExtensions.CalculatePercentage(this.CoveredBranches.GetValueOrDefault(), this.TotalBranches.GetValueOrDefault());

        /// <summary>
        /// Gets the number of covered code elements.
        /// </summary>
        /// <value>
        /// The number of covered code elements.
        /// </value>
        public int CoveredCodeElements => this.Assemblies.SafeSum(f => f.CoveredCodeElements);

        /// <summary>
        /// Gets the number of fully covered code elements.
        /// </summary>
        /// <value>
        /// The number of fully covered code elements.
        /// </value>
        public int FullCoveredCodeElements => this.Assemblies.SafeSum(f => f.FullCoveredCodeElements);

        /// <summary>
        /// Gets the number of total code elements.
        /// </summary>
        /// <value>
        /// The number of total code elements.
        /// </value>
        public int TotalCodeElements => this.Assemblies.SafeSum(f => f.TotalCodeElements);

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
        /// Gets all sumable metrics.
        /// </summary>
        /// <value>The sumable metrics.</value>
        public IReadOnlyCollection<Metric> SumableMetrics =>
            this.Assemblies
                .SelectMany(a => a.Classes)
                .SelectMany(c => c.Files)
                .SelectMany(f => f.MethodMetrics)
                .SelectMany(m => m.Metrics)
                .Where(m => m.MetricType == MetricType.CoverageAbsolute)
                .GroupBy(m => m.Name)
                .Select(g => new Metric(g.Key, g.First().Abbreviation, g.First().ExplanationUrl, MetricType.CoverageAbsolute, g.SafeSum(m => m.Value)))
                .ToList();

        /// <summary>
        /// Get the coverage date(s) based on the minimum and maximum timestamp.
        /// </summary>
        /// <returns> The coverage date(s).</returns>
        public string CoverageDate()
        {
            string value = null;

            if (this.MinimumTimeStamp.HasValue)
            {
                value = $"{this.MinimumTimeStamp.Value.ToShortDateString()} - {this.MinimumTimeStamp.Value.ToLongTimeString()}";

                if (this.MaximumTimeStamp.HasValue
                    && !this.MinimumTimeStamp.Value.Equals(this.MaximumTimeStamp.Value))
                {
                    value += $" - {this.MaximumTimeStamp.Value.ToShortDateString()} - {this.MaximumTimeStamp.Value.ToLongTimeString()}";
                }
            }
            else if (this.MaximumTimeStamp.HasValue)
            {
                value = $"{this.MaximumTimeStamp.Value.ToShortDateString()} - {this.MaximumTimeStamp.Value.ToLongTimeString()}";
            }

            return value;
        }
    }
}
