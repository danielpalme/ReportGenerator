using System;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Global settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Gets or sets the number reports that are parsed and processed in parallel.
        /// </summary>
        public int NumberOfReportsParsedInParallel { get; set; } = 1;

        /// <summary>
        /// Gets or sets the number reports that are merged in parallel.
        /// </summary>
        public int NumberOfReportsMergedInParallel { get; set; } = 1;

        /// <summary>
        /// Gets or sets the maximum number of historic coverage files that get parsed.
        /// </summary>
        public int MaximumNumberOfHistoricCoverageFiles { get; set; } = 100;

        /// <summary>
        /// Gets or sets the caching duration of code files that are downloaded from remote servers in minutes.
        /// </summary>
        public int CachingDurationOfRemoteFilesInMinutes { get; set; } = 7 * 24 * 60;

        /// <summary>
        /// Gets or sets the caching duration of code files that are downloaded from remote servers in minutes.
        /// </summary>
        [Obsolete("Replaced by 'CachingDurationOfRemoteFilesInMinutes'.")]
        public int CachingDuringOfRemoteFilesInMinutes
        {
            get
            {
                return this.CachingDurationOfRemoteFilesInMinutes;
            }

            set
            {
                this.CachingDurationOfRemoteFilesInMinutes = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether risk hotspots should be disabled or not.
        /// </summary>
        public bool DisableRiskHotspots { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether test projects should be included or not (only for Clover files).
        /// </summary>
        public bool ExcludeTestProjects { get; set; } = false;

        /// <summary>
        ///  Gets or sets a value indicating whether a subdirectory should be created in the target directory for each report type.
        /// </summary>
        public bool CreateSubdirectoryForAllReportTypes { get; set; } = false;

        /// <summary>
        ///  Gets or sets custom headers (e.g. authentication headers) for remote requests.
        ///  Format: key1=value1;key2=value2
        ///  Example: Authorization=Bearer ~JWT~
        /// </summary>
        public string CustomHeadersForRemoteFiles { get; set; }

        /// <summary>
        ///  Gets or sets the default assembly name for gcov and lcov.
        /// </summary>
        public string DefaultAssemblyName { get; set; } = "Default";

        /// <summary>
        /// Gets or sets the maximum decimal places for coverage quotas / percentages.
        /// </summary>
        public int MaximumDecimalPlacesForCoverageQuotas { get; set; } = 1;

        /// <summary>
        /// Gets or sets the prefix for history files.
        /// </summary>
        public string HistoryFileNamePrefix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether class names are interpreted (false) or not (true).
        /// Interpreted means that the coverage data of nested or compiler generated classes is included in the parent class.
        /// In raw mode the coverage data is reported for each class separately.
        /// </summary>
        public bool RawMode { get; set; }
    }
}
