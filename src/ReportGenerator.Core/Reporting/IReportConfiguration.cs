using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Logging;

namespace Palmmedia.ReportGenerator.Core.Reporting
{
    /// <summary>
    /// Provides all parameters that are required for report generation.
    /// </summary>
    public interface IReportConfiguration
    {
        /// <summary>
        /// Gets the files containing coverage information.
        /// </summary>
        IReadOnlyCollection<string> ReportFiles { get; }

        /// <summary>
        /// Gets the target directory.
        /// </summary>
        string TargetDirectory { get; }

        /// <summary>
        /// Gets the source directories.
        /// </summary>
        IReadOnlyCollection<string> SourceDirectories { get; }

        /// <summary>
        /// Gets the history directory.
        /// </summary>
        string HistoryDirectory { get; }

        /// <summary>
        /// Gets the type of the report.
        /// </summary>
        IReadOnlyCollection<string> ReportTypes { get; }

        /// <summary>
        /// Gets the plugins.
        /// </summary>
        IReadOnlyCollection<string> Plugins { get; }

        /// <summary>
        /// Gets the assembly filters.
        /// </summary>
        IReadOnlyCollection<string> AssemblyFilters { get; }

        /// <summary>
        /// Gets the class filters.
        /// </summary>
        IReadOnlyCollection<string> ClassFilters { get; }

        /// <summary>
        /// Gets the file filters.
        /// </summary>
        IReadOnlyCollection<string> FileFilters { get; }

        /// <summary>
        /// Gets the assembly filters for risk hotspots.
        /// </summary>
        IReadOnlyCollection<string> RiskHotspotAssemblyFilters { get; }

        /// <summary>
        /// Gets the class filters for risk hotspots.
        /// </summary>
        IReadOnlyCollection<string> RiskHotspotClassFilters { get; }

        /// <summary>
        /// Gets the verbosity level.
        /// </summary>
        VerbosityLevel VerbosityLevel { get; }

        /// <summary>
        /// Gets the custom tag (e.g. build number).
        /// </summary>
        string Tag { get; }

        /// <summary>
        /// Gets the custom title.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the license in Base64 format.
        /// </summary>
        string License { get; }

        /// <summary>
        /// Gets the invalid file patters supplied by the user.
        /// </summary>
        IReadOnlyCollection<string> InvalidReportFilePatterns { get; }

        /// <summary>
        /// Gets a value indicating whether the verbosity level was successfully parsed during initialization.
        /// </summary>
        bool VerbosityLevelValid { get; }
    }
}
