using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Builder for <see cref="ReportConfiguration"/>.
    /// Creates instances of <see cref="ReportConfiguration"/> based on command line parameters.
    /// </summary>
    internal class ReportConfigurationBuilder
    {
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
                    namedArguments[match.Groups["key"].Value.ToUpperInvariant()] = match.Groups["value"].Value;
                }
            }

            var reportFilePatterns = new string[] { };
            string targetDirectory = string.Empty;
            var sourceDirectories = new string[] { };
            string historyDirectory = null;
            var reportTypes = new string[] { };
            var plugins = new string[] { };
            var assemblyFilters = new string[] { };
            var classFilters = new string[] { };
            var fileFilters = new string[] { };
            string verbosityLevel = null;
            string tag = null;

            string value = null;

            if (namedArguments.TryGetValue("REPORTS", out value))
            {
                reportFilePatterns = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("TARGETDIR", out value))
            {
                targetDirectory = value;
            }

            if (namedArguments.TryGetValue("SOURCEDIRS", out value))
            {
                sourceDirectories = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("HISTORYDIR", out value))
            {
                historyDirectory = value;
            }

            if (namedArguments.TryGetValue("REPORTTYPES", out value))
            {
                reportTypes = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (namedArguments.TryGetValue("REPORTTYPE", out value))
            {
                reportTypes = new[] { value };
            }

            if (namedArguments.TryGetValue("PLUGINS", out value))
            {
                plugins = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("ASSEMBLYFILTERS", out value))
            {
                assemblyFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (namedArguments.TryGetValue("FILTERS", out value))
            {
                assemblyFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("CLASSFILTERS", out value))
            {
                classFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("FILEFILTERS", out value))
            {
                fileFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("VERBOSITY", out value))
            {
                verbosityLevel = value;
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
                classFilters,
                fileFilters,
                verbosityLevel,
                tag);
        }
    }
}