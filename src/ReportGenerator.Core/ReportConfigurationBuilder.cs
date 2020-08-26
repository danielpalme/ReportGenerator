using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetConfig;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Builder for <see cref="ReportConfiguration"/>.
    /// Creates instances of <see cref="ReportConfiguration"/> based on command line parameters.
    /// </summary>
    public class ReportConfigurationBuilder
    {
        /// <summary>
        /// Name of the configuration section in a .netconfig file.
        /// </summary>
        public const string SectionName = "ReportGenerator";

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportConfiguration"/> class.
        /// </summary>
        /// <param name="cliArguments">The command line arguments stored as key value pairs.</param>
        /// <returns>The report configuration.</returns>
        public ReportConfiguration Create(Dictionary<string, string> cliArguments)
        {
            var namedArguments = new Dictionary<string, string>(cliArguments, StringComparer.OrdinalIgnoreCase);
            var config = Config.Build().GetSection(SectionName);

            var reportFilePatterns = Array.Empty<string>();
            var targetDirectory = string.Empty;
            var sourceDirectories = Array.Empty<string>();
            string historyDirectory = null;
            var reportTypes = Array.Empty<string>();
            var plugins = Array.Empty<string>();
            var assemblyFilters = Array.Empty<string>();
            var classFilters = Array.Empty<string>();
            var fileFilters = Array.Empty<string>();
            string verbosityLevel = null;
            string title = null;
            string tag = null;

            string value = null;

            if (namedArguments.TryGetValue("REPORTS", out value))
            {
                reportFilePatterns = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (config.TryGetString("reports", out value))
            {
                reportFilePatterns = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                reportFilePatterns = config
                    .GetAll("report")
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (namedArguments.TryGetValue("TARGETDIR", out value))
            {
                targetDirectory = value;
            }
            else if (config.TryGetString("targetdir", out value))
            {
                targetDirectory = value;
            }

            if (namedArguments.TryGetValue("SOURCEDIRS", out value))
            {
                sourceDirectories = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (config.TryGetString("sourcedirs", out value))
            {
                sourceDirectories = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                sourceDirectories = config
                    .GetAll("sourcedir")
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (namedArguments.TryGetValue("HISTORYDIR", out value))
            {
                historyDirectory = value;
            }
            else if (config.TryGetString("historydir", out value))
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
            else if (config.TryGetString("reporttypes", out value))
            {
                reportTypes = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                reportTypes = config
                    .GetAll("reporttype")
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (namedArguments.TryGetValue("PLUGINS", out value))
            {
                plugins = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (config.TryGetString("plugins", out value))
            {
                plugins = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                plugins = config
                    .GetAll("plugin")
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (namedArguments.TryGetValue("ASSEMBLYFILTERS", out value))
            {
                assemblyFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (namedArguments.TryGetValue("FILTERS", out value))
            {
                assemblyFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (config.TryGetString("assemblyfilters", out value))
            {
                assemblyFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                assemblyFilters = config
                    .GetAll("assemblyfilter")
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (namedArguments.TryGetValue("CLASSFILTERS", out value))
            {
                classFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (config.TryGetString("classfilters", out value))
            {
                classFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                classFilters = config
                    .GetAll("classfilter")
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (namedArguments.TryGetValue("FILEFILTERS", out value))
            {
                fileFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (config.TryGetString("filefilters", out value))
            {
                fileFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                fileFilters = config
                    .GetAll("filefilter")
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (namedArguments.TryGetValue("VERBOSITY", out value))
            {
                verbosityLevel = value;
            }
            else if (config.TryGetString("verbosity", out value))
            {
                verbosityLevel = value;
            }

            if (namedArguments.TryGetValue("TITLE", out value))
            {
                title = value;
            }
            else if (config.TryGetString("title", out value))
            {
                title = value;
            }

            if (namedArguments.TryGetValue("TAG", out value))
            {
                tag = value;
            }
            else if (config.TryGetString("tag", out value))
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