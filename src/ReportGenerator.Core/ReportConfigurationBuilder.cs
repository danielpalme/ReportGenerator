using System;
using System.Collections.Generic;
using System.Linq;
using DotNetConfig;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Builder for <see cref="ReportConfiguration"/>.
    /// Creates instances of <see cref="ReportConfiguration"/> based on command line parameters.
    /// </summary>
    public class ReportConfigurationBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(ReportConfigurationBuilder));

        /// <summary>
        /// The argument separators.
        /// </summary>
        private static readonly char[] ArgumentSeparators = new[] { ';', ',' };

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportConfiguration"/> class.
        /// </summary>
        /// <param name="cliArguments">The command line arguments stored as key value pairs.</param>
        /// <returns>The report configuration.</returns>
        public ReportConfiguration Create(Dictionary<string, string> cliArguments)
        {
            var namedArguments = new Dictionary<string, string>(cliArguments, StringComparer.OrdinalIgnoreCase);
            var config = Config.Build().GetSection(DotNetConfigSettingNames.SectionName);

            var reportFilePatterns = Array.Empty<string>();
            var targetDirectory = string.Empty;
            var sourceDirectories = Array.Empty<string>();
            string historyDirectory = null;
            var reportTypes = Array.Empty<string>();
            var plugins = Array.Empty<string>();
            var assemblyFilters = Array.Empty<string>();
            var classFilters = Array.Empty<string>();
            var fileFilters = Array.Empty<string>();
            var riskHotspotAssemblyFilters = Array.Empty<string>();
            var riskHotspotClassFilters = Array.Empty<string>();
            string verbosityLevel = null;
            string tag = null;
            string title = null;
            string license = null;

            string value = null;

            if (namedArguments.TryGetValue(CommandLineArgumentNames.Reports, out value))
            {
                reportFilePatterns = value.SplitThatEnsuresGlobsAreSafe(ArgumentSeparators);
            }
            else if (config.TryGetString(DotNetConfigSettingNames.Reports, out value))
            {
                reportFilePatterns = value.SplitThatEnsuresGlobsAreSafe(ArgumentSeparators);
            }
            else
            {
                reportFilePatterns = config
                    .GetAll(DotNetConfigSettingNames.Report)
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (namedArguments.TryGetValue(CommandLineArgumentNames.TargetDirectory, out value))
            {
                targetDirectory = value;
            }
            else if (config.TryGetString(DotNetConfigSettingNames.TargetDirectory, out value))
            {
                targetDirectory = value;
            }

            if (namedArguments.TryGetValue(CommandLineArgumentNames.SourceDirectories, out value))
            {
                sourceDirectories = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (config.TryGetString(DotNetConfigSettingNames.SourceDirectories, out value))
            {
                sourceDirectories = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                sourceDirectories = config
                    .GetAll(DotNetConfigSettingNames.SourceDirectory)
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (namedArguments.TryGetValue(CommandLineArgumentNames.HistoryDirectory, out value))
            {
                historyDirectory = value;
            }
            else if (config.TryGetString(DotNetConfigSettingNames.HistoryDirectory, out value))
            {
                historyDirectory = value;
            }

            if (namedArguments.TryGetValue(CommandLineArgumentNames.ReportTypes, out value))
            {
                reportTypes = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (namedArguments.TryGetValue(CommandLineArgumentNames.ReportType, out value))
            {
                reportTypes = new[] { value };
            }
            else if (config.TryGetString(DotNetConfigSettingNames.ReportTypes, out value))
            {
                reportTypes = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                reportTypes = config
                    .GetAll(DotNetConfigSettingNames.ReportType)
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (namedArguments.TryGetValue(CommandLineArgumentNames.Plugins, out value))
            {
                plugins = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (config.TryGetString(DotNetConfigSettingNames.Plugins, out value))
            {
                plugins = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                plugins = config
                    .GetAll(DotNetConfigSettingNames.Plugin)
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (namedArguments.TryGetValue(CommandLineArgumentNames.AssemblyFilters, out value))
            {
                assemblyFilters = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (namedArguments.TryGetValue(CommandLineArgumentNames.Filters, out value))
            {
                assemblyFilters = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (config.TryGetString(DotNetConfigSettingNames.AssemblyFilters, out value))
            {
                assemblyFilters = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                assemblyFilters = config
                    .GetAll(DotNetConfigSettingNames.AssemblyFilter)
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (namedArguments.TryGetValue(CommandLineArgumentNames.ClassFilters, out value))
            {
                classFilters = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (config.TryGetString(DotNetConfigSettingNames.ClassFilters, out value))
            {
                classFilters = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                classFilters = config
                    .GetAll(DotNetConfigSettingNames.ClassFilter)
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (namedArguments.TryGetValue(CommandLineArgumentNames.FileFilters, out value))
            {
                fileFilters = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (config.TryGetString(DotNetConfigSettingNames.FileFilters, out value))
            {
                fileFilters = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                fileFilters = config
                    .GetAll(DotNetConfigSettingNames.FileFilter)
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (namedArguments.TryGetValue(CommandLineArgumentNames.RiskHotspotAssemblyFilters, out value))
            {
                riskHotspotAssemblyFilters = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (config.TryGetString(DotNetConfigSettingNames.RiskHotspotAssemblyFilters, out value))
            {
                riskHotspotAssemblyFilters = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                riskHotspotAssemblyFilters = config
                    .GetAll(DotNetConfigSettingNames.RiskHotspotAssemblyFilter)
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (namedArguments.TryGetValue(CommandLineArgumentNames.RiskHotspotClassFilters, out value))
            {
                riskHotspotClassFilters = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (config.TryGetString(DotNetConfigSettingNames.RiskHotspotClassFilters, out value))
            {
                riskHotspotClassFilters = value.Split(ArgumentSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                riskHotspotClassFilters = config
                    .GetAll(DotNetConfigSettingNames.RiskHotspotClassFilter)
                    .Select(x => x.RawValue)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToArray();
            }

            if (namedArguments.TryGetValue(CommandLineArgumentNames.Verbosity, out value))
            {
                verbosityLevel = value;
            }
            else if (config.TryGetString(DotNetConfigSettingNames.Verbosity, out value))
            {
                verbosityLevel = value;
            }

            if (namedArguments.TryGetValue(CommandLineArgumentNames.Tag, out value))
            {
                tag = value;
            }
            else if (config.TryGetString(DotNetConfigSettingNames.Tag, out value))
            {
                tag = value;
            }

            if (namedArguments.TryGetValue(CommandLineArgumentNames.Title, out value))
            {
                title = value;
            }
            else if (config.TryGetString(DotNetConfigSettingNames.Title, out value))
            {
                title = value;
            }

            string licenseFromEnviroment = Environment.GetEnvironmentVariable("REPORTGENERATOR_LICENSE");

            if (namedArguments.TryGetValue(CommandLineArgumentNames.License, out value))
            {
                license = value;
            }
            else if (config.TryGetString(DotNetConfigSettingNames.License, out value))
            {
                license = value;
            }
            else if (licenseFromEnviroment != null)
            {
                license = licenseFromEnviroment;
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
                riskHotspotAssemblyFilters,
                riskHotspotClassFilters,
                verbosityLevel,
                tag,
                title,
                license);
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
                var match = CommandLineArgumentNames.CommandLineParameterRegex.Match(arg);

                if (match.Success)
                {
                    if (namedArguments.ContainsKey(match.Groups["key"].Value))
                    {
                        Logger.WarnFormat(Resources.DuplicateCommandLineParameter, match.Groups["key"].Value, namedArguments[match.Groups["key"].Value]);
                    }
                    else
                    {
                        if (CommandLineArgumentNames.IsValid(match.Groups["key"].Value))
                        {
                            namedArguments[match.Groups["key"].Value] = match.Groups["value"].Value;
                        }
                        else
                        {
                            Logger.WarnFormat(Resources.UnknownCommandLineParameter, match.Groups["key"].Value);
                        }
                    }
                }
            }

            return this.Create(namedArguments);
        }
    }
}