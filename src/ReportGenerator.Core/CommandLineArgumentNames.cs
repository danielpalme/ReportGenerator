using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Name of the command line arguments.
    /// </summary>
    internal static class CommandLineArgumentNames
    {
        /// <summary>
        /// The reports.
        /// </summary>
        public const string Reports = "REPORTS";

        /// <summary>
        /// The target directory.
        /// </summary>
        public const string TargetDirectory = "TARGETDIR";

        /// <summary>
        /// The source directories.
        /// </summary>
        public const string SourceDirectories = "SOURCEDIRS";

        /// <summary>
        /// The history directory.
        /// </summary>
        public const string HistoryDirectory = "HISTORYDIR";

        /// <summary>
        /// The report types.
        /// </summary>
        public const string ReportTypes = "REPORTTYPES";

        /// <summary>
        /// Single report type (deprecated).
        /// </summary>
        public const string ReportType = "REPORTTYPE";

        /// <summary>
        /// The plugins.
        /// </summary>
        public const string Plugins = "PLUGINS";

        /// <summary>
        /// The assembly filters.
        /// </summary>
        public const string AssemblyFilters = "ASSEMBLYFILTERS";

        /// <summary>
        /// The assembly filters (deprecated).
        /// </summary>
        public const string Filters = "FILTERS";

        /// <summary>
        /// Single class filter.
        /// </summary>
        public const string ClassFilters = "CLASSFILTERS";

        /// <summary>
        /// The file filters.
        /// </summary>
        public const string FileFilters = "FILEFILTERS";

        /// <summary>
        /// The assembly filters for risk hotspots.
        /// </summary>
        public const string RiskHotspotAssemblyFilters = "RISKHOTSPOTASSEMBLYFILTERS";

        /// <summary>
        /// The class filters for risk hotspots.
        /// </summary>
        public const string RiskHotspotClassFilters = "RISKHOTSPOTCLASSFILTERS";

        /// <summary>
        /// The verbosity.
        /// </summary>
        public const string Verbosity = "VERBOSITY";

        /// <summary>
        /// The tag.
        /// </summary>
        public const string Tag = "TAG";

        /// <summary>
        /// The title.
        /// </summary>
        public const string Title = "TITLE";

        /// <summary>
        /// The license.
        /// </summary>
        public const string License = "LICENSE";

        /// <summary>
        /// All valid command line parameter names.
        /// </summary>
        private static readonly HashSet<string> ValidNames = new HashSet<string>(
            typeof(CommandLineArgumentNames)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(x => (string)x.GetRawConstantValue())
                .ToList());

        /// <summary>
        /// Gets the regex to parse command line parameters.
        /// </summary>
        internal static Regex CommandLineParameterRegex { get; } = new Regex("^-(?<key>[a-zA-Z]{2,}):(?<value>.+)$", RegexOptions.Compiled);

        /// <summary>
        /// Gets a value indicating whether a command line parameter name is valid.
        /// </summary>
        /// <param name="name">The command line parameter name.</param>
        /// <returns><c>true</c> if command line parameter is valid; otherwise <c>false</c>.</returns>
        public static bool IsValid(string name)
        {
            if (name == null)
            {
                return false;
            }

            return ValidNames.Contains(name.ToUpperInvariant());
        }
    }
}
