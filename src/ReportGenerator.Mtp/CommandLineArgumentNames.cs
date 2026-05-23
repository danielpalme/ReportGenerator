using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Palmmedia.ReportGenerator.Mtp
{
    /// <summary>
    /// Name of the command line arguments.
    /// </summary>
    internal static class CommandLineArgumentNames
    {
        /// <summary>
        /// The reports.
        /// </summary>
        public const string Reports = "reportgenerator-reports";

        /// <summary>
        /// The target directory.
        /// </summary>
        public const string TargetDirectory = "reportgenerator-targetdir";

        /// <summary>
        /// The source directories.
        /// </summary>
        public const string SourceDirectories = "reportgenerator-sourcedirs";

        /// <summary>
        /// The history directory.
        /// </summary>
        public const string HistoryDirectory = "reportgenerator-historydir";

        /// <summary>
        /// The report types.
        /// </summary>
        public const string ReportTypes = "reportgenerator-reporttypes";

        /// <summary>
        /// Single report type (deprecated).
        /// </summary>
        public const string ReportType = "reportgenerator-reporttype";

        /// <summary>
        /// The plugins.
        /// </summary>
        public const string Plugins = "reportgenerator-plugins";

        /// <summary>
        /// The assembly filters.
        /// </summary>
        public const string AssemblyFilters = "reportgenerator-assemblyfilters";

        /// <summary>
        /// The assembly filters (deprecated).
        /// </summary>
        public const string Filters = "reportgenerator-filters";

        /// <summary>
        /// Single class filter.
        /// </summary>
        public const string ClassFilters = "reportgenerator-classfilters";

        /// <summary>
        /// The file filters.
        /// </summary>
        public const string FileFilters = "reportgenerator-filefilters";

        /// <summary>
        /// The assembly filters for risk hotspots.
        /// </summary>
        public const string RiskHotspotAssemblyFilters = "reportgenerator-riskhotspotassemblyfilters";

        /// <summary>
        /// The class filters for risk hotspots.
        /// </summary>
        public const string RiskHotspotClassFilters = "reportgenerator-riskhotspotclassfilters";

        /// <summary>
        /// The verbosity.
        /// </summary>
        public const string Verbosity = "reportgenerator-verbosity";

        /// <summary>
        /// The tag.
        /// </summary>
        public const string Tag = "reportgenerator-tag";

        /// <summary>
        /// The title.
        /// </summary>
        public const string Title = "reportgenerator-title";

        /// <summary>
        /// The license.
        /// </summary>
        public const string License = "reportgenerator-license";

        /// <summary>
        /// The custom settings.
        /// </summary>
        public const string CustomSettings = "reportgenerator-customsettings";

        /// <summary>
        /// All valid command line parameter names.
        /// </summary>
        public static readonly IReadOnlyList<string> ValidNames = new List<string>(
            typeof(CommandLineArgumentNames)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(x => (string)x.GetRawConstantValue())
                .ToList());
    }
}
