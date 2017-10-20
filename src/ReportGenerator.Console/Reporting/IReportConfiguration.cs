using System.Collections.Generic;
using Palmmedia.ReportGenerator.Logging;
using Palmmedia.ReportGenerator.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Provides all parameters that are required for report generation.
    /// </summary>
    public interface IReportConfiguration
    {
        /// <summary>
        /// Gets the target directory.
        /// </summary>
        string TargetDirectory { get; }

        /// <summary>
        /// Gets the history directory.
        /// </summary>
        string HistoryDirectory { get; }

        /// <summary>
        /// Gets the type of the report.
        /// </summary>
        IEnumerable<string> ReportTypes { get; }

        /// <summary>
        /// Gets the source directories.
        /// </summary>
        IEnumerable<string> SourceDirectories { get; }

        /// <summary>
        /// Gets the assembly filters.
        /// </summary>
        IEnumerable<string> AssemblyFilters { get; }

        /// <summary>
        /// Gets the class filters.
        /// </summary>
        IEnumerable<string> ClassFilters { get; }

        /// <summary>
        /// Gets the file filters.
        /// </summary>
        IEnumerable<string> FileFilters { get; }

        /// <summary>
        /// Gets the verbosity level.
        /// </summary>
        VerbosityLevel VerbosityLevel { get; }

        /// <summary>
        /// Gets the custom tag (e.g. build number).
        /// </summary>
        string Tag { get; }

        /// <summary>
        /// Gets all historic coverage elements.
        /// </summary>
        IEnumerable<HistoricCoverage> OverallHistoricCoverages { get; }
    }
}
