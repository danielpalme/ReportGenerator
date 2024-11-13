using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Palmmedia.ReportGenerator.Core.Parser.Filtering
{
    /// <summary>
    /// Default implementation of <see cref="IFilter"/>.
    /// An element is included if at least one include filter matches their name.
    /// The assembly is excluded if at least one exclude filter matches its name.
    /// Exclusion filters take precedence over inclusion filters. Wildcards are allowed in filters.
    /// </summary>
    public class DefaultFilter : IFilter
    {
        /// <summary>
        /// The include filters.
        /// </summary>
        private readonly Regex[] includeFilters;

        /// <summary>
        /// The exclude filters.
        /// </summary>
        private readonly Regex[] excludeFilters;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFilter"/> class.
        /// </summary>
        /// <param name="filters">The filters.</param>
        public DefaultFilter(IEnumerable<string> filters)
            : this(filters, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultFilter"/> class.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="osIndependantPathSeparator">Indicates whether filter thould be treated as paths and the operating system format should be ignored.</param>
        public DefaultFilter(IEnumerable<string> filters, bool osIndependantPathSeparator)
        {
            if (filters == null)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            this.excludeFilters = filters
                .Where(f => f.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                .Select(f => CreateFilterRegex(f, osIndependantPathSeparator))
                .ToArray();

            this.includeFilters = filters
                .Where(f => f.StartsWith("+", StringComparison.OrdinalIgnoreCase))
                .Select(f => CreateFilterRegex(f, osIndependantPathSeparator))
                .ToArray();

            this.HasCustomFilters = this.excludeFilters.Length > 0 || this.includeFilters.Length > 0;

            if (this.includeFilters.Length == 0)
            {
                this.includeFilters = new[]
                {
                    CreateFilterRegex("+*", false)
                };
            }
        }

        /// <summary>
        /// Gets a value indicating whether custom filter are applied.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if custom filter are applied; otherwise, <c>false</c>.
        /// </returns>
        public bool HasCustomFilters { get; }

        /// <summary>
        /// Determines whether the given element should be included in the report.
        /// </summary>
        /// <param name="name">Name of the element.</param>
        /// <returns>
        ///   <c>true</c> if element should be included in the report; otherwise, <c>false</c>.
        /// </returns>
        public bool IsElementIncludedInReport(string name)
        {
            if (this.excludeFilters.Any(f => f.IsMatch(name)))
            {
                return false;
            }
            else
            {
                return this.includeFilters.Any(f => f.IsMatch(name));
            }
        }

        /// <summary>
        /// Converts the given filter to a corresponding regular expression.
        /// Special characters are escaped. Wildcards '*' are converted to '.*'.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="osIndependantPathSeparator">Indicates whether filter thould be treated as paths and the operating system format should be ignored.</param>
        /// <returns>The regular expression.</returns>
        private static Regex CreateFilterRegex(string filter, bool osIndependantPathSeparator)
        {
            filter = filter.Substring(1);
            filter = filter.Replace("*", "$$$*");

            if (osIndependantPathSeparator)
            {
                filter = filter
                    .Replace("/", "$$$pathseparator$$$")
                    .Replace("\\", "$$$pathseparator$$$");
            }

            filter = Regex.Escape(filter);
            filter = filter.Replace(@"\$\$\$\*", ".*");

            if (osIndependantPathSeparator)
            {
                filter = filter.Replace(@"\$\$\$pathseparator\$\$\$", @"[/\\]");
            }

            return new Regex($"^{filter}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}