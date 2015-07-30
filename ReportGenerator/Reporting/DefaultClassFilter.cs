using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Default implementation of <see cref="IClassFilter"/>.
    /// An class is included if at least one include filter matches their name.
    /// The class is excluded if at least one exclude filter matches its name.
    /// Exclusion filters take precedence over inclusion filters. Wildcards are allowed in filters.
    /// </summary>
    class DefaultClassFilter : IClassFilter
    {
        /// <summary>
        /// The include filters.
        /// </summary>
        private readonly IEnumerable<string> includeFilters;

        /// <summary>
        /// The exclude filters.
        /// </summary>
        private readonly IEnumerable<string> excludeFilters;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultClassFilter"/> class.
        /// </summary>
        /// <param name="filters">The filters.</param>
        internal DefaultClassFilter(IEnumerable<string> filters)
        {
            if (filters == null)
            {
                throw new ArgumentNullException("filters");
            }

            this.excludeFilters = filters
                .Where(f => f.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                .Select(f => CreateFilterRegex(f));

            this.includeFilters = filters
                .Where(f => f.StartsWith("+", StringComparison.OrdinalIgnoreCase))
                .Select(f => CreateFilterRegex(f));

            if (!this.includeFilters.Any())
            {
                this.includeFilters = Enumerable.Repeat(CreateFilterRegex("+*"), 1);
            }
        }

        /// <summary>
        /// Determines whether the given class should be included in the report.
        /// </summary>
        /// <param name="assemblyName">Name of the class.</param>
        /// <returns>
        ///   <c>true</c> if class should be included in the report; otherwise, <c>false</c>.
        /// </returns>
        public bool IsClassIncludedInReport(string className)
        {
            if (this.excludeFilters.Any(f => Regex.IsMatch(className, f)))
            {
                return false;
            }
            else
            {
                return this.includeFilters.Any(f => Regex.IsMatch(className, f));
            }
        }

        /// <summary>
        /// Converts the given filter to a corresponding regular expression.
        /// Special characters are escaped. Wildcards '*' are converted to '.*'.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>The regular expression.</returns>
        private static string CreateFilterRegex(string filter)
        {
            filter = filter.Substring(1);
            filter = filter.Replace("*", "$$$*");
            filter = Regex.Escape(filter);
            filter = filter.Replace(@"\$\$\$\*", ".*");

            return string.Format(CultureInfo.InvariantCulture, "^{0}$", filter);
        }
    }
}
