using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Builder for <see cref="ReportConfiguration"/>.
    /// Creates instances of <see cref="ReportConfiguration"/> based on command line parameters.
    /// </summary>
    public class ReportConfigurationBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportConfiguration"/> class.
        /// </summary>
        /// <param name="cliArguments">The command line arguments stored as key value pairs.</param>
        /// <returns>The report configuration.</returns>
        public ReportConfiguration Create(Dictionary<string, string> cliArguments)
        {
            var namedArguments = new Dictionary<string, string>(cliArguments, StringComparer.OrdinalIgnoreCase);

            var reportFilePatterns = new string[] { };
            string targetDirectory = string.Empty;
            var sourceDirectories = new string[] { };
            string historyDirectory = null;
            var reportTypes = new string[] { };
            var plugins = new string[] { };
            var assemblyFilters = new string[] { };
            var namespaceFilters = new string[] { };
            var classFilters = new string[] { };
            var fileFilters = new string[] { };
            string verbosityLevel = null;
            string title = null;
            string tag = null;

            string value = null;

            char[] separator = new[] { ';' };

            if (namedArguments.TryGetValue("REPORTS", out value))
            {
                reportFilePatterns = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("TARGETDIR", out value))
            {
                targetDirectory = value;
            }

            if (namedArguments.TryGetValue("SOURCEDIRS", out value))
            {
                sourceDirectories = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("HISTORYDIR", out value))
            {
                historyDirectory = value;
            }

            if (namedArguments.TryGetValue("REPORTTYPES", out value))
            {
                reportTypes = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (namedArguments.TryGetValue("REPORTTYPE", out value))
            {
                reportTypes = new[] { value };
            }

            if (namedArguments.TryGetValue("PLUGINS", out value))
            {
                plugins = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("ASSEMBLYFILTERS", out value))
            {
                assemblyFilters = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (namedArguments.TryGetValue("FILTERS", out value))
            {
                assemblyFilters = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("NAMESPACEFILTERS", out value))
            {
                namespaceFilters = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("CLASSFILTERS", out value))
            {
                classFilters = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("FILEFILTERS", out value))
            {
                fileFilters = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("VERBOSITY", out value))
            {
                verbosityLevel = value;
            }

            if (namedArguments.TryGetValue("TITLE", out value))
            {
                title = value;
            }

            if (namedArguments.TryGetValue("TAG", out value))
            {
                tag = value;
            }

            return new ReportConfiguration(
                reportFilePatterns,
                targetDirectory,
                sourceDirectories,
                historyDirectory,
                reportTypes,
                plugins,
                assemblyFilters,
                namespaceFilters,
                classFilters,
                fileFilters,
                verbosityLevel,
                tag,
                title);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportConfiguration"/> class.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The report configuration.</returns>
        internal ReportConfiguration Create(string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var namedArguments = new Dictionary<string, string>();

            foreach (var arg in args)
            {
                var match = Regex.Match(arg, "-(?<key>\\w{2,}):(?<value>.+)");

                if (match.Success)
                {
                    namedArguments[match.Groups["key"].Value] = match.Groups["value"].Value;
                }
            }

            return this.Create(namedArguments);
        }
    }
}