using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Common;
using Palmmedia.ReportGenerator.Logging;
using Palmmedia.ReportGenerator.Properties;
using Palmmedia.ReportGenerator.Reporting;

namespace Palmmedia.ReportGenerator
{
    /// <summary>
    /// Provides all parameters that are required for report generation.
    /// </summary>
    public class ReportConfiguration
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(ReportConfiguration));

        /// <summary>
        /// The report files.
        /// </summary>
        private List<string> reportFiles = new List<string>();

        /// <summary>
        /// The report file pattern that could not be parsed.
        /// </summary>
        private List<string> failedReportFilePatterns = new List<string>();

        /// <summary>
        /// Determines whether the verbosity level was successfully parsed during initialization.
        /// </summary>
        private bool verbosityLevelValid = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportConfiguration" /> class.
        /// </summary>
        /// <param name="reportBuilderFactory">The report builder factory.</param>
        /// <param name="reportFilePatterns">The report file patterns.</param>
        /// <param name="targetDirectory">The target directory.</param>
        /// <param name="historyDirectory">The history directory.</param>
        /// <param name="reportTypes">The report types.</param>
        /// <param name="sourceDirectories">The source directories.</param>
        /// <param name="assemblyFilters">The assembly filters.</param>
        /// <param name="classFilters">The class filters.</param>
        /// <param name="fileFilters">The file filters.</param>
        /// <param name="verbosityLevel">The verbosity level.</param>
        public ReportConfiguration(
            IReportBuilderFactory reportBuilderFactory,
            IEnumerable<string> reportFilePatterns,
            string targetDirectory,
            string historyDirectory,
            IEnumerable<string> reportTypes,
            IEnumerable<string> sourceDirectories,
            IEnumerable<string> assemblyFilters,
            IEnumerable<string> classFilters,
            IEnumerable<string> fileFilters,
            string verbosityLevel)
        {
            if (reportBuilderFactory == null)
            {
                throw new ArgumentNullException(nameof(reportBuilderFactory));
            }

            if (reportFilePatterns == null)
            {
                throw new ArgumentNullException(nameof(reportFilePatterns));
            }

            if (targetDirectory == null)
            {
                throw new ArgumentNullException(nameof(targetDirectory));
            }

            if (reportTypes == null)
            {
                throw new ArgumentNullException(nameof(reportTypes));
            }

            if (sourceDirectories == null)
            {
                throw new ArgumentNullException(nameof(sourceDirectories));
            }

            if (assemblyFilters == null)
            {
                throw new ArgumentNullException(nameof(assemblyFilters));
            }

            if (classFilters == null)
            {
                throw new ArgumentNullException(nameof(classFilters));
            }

            if (fileFilters == null)
            {
                throw new ArgumentNullException(nameof(fileFilters));
            }

            this.ReportBuilderFactory = reportBuilderFactory;
            foreach (var reportFilePattern in reportFilePatterns)
            {
                try
                {
                    this.reportFiles.AddRange(FileSearch.GetFiles(reportFilePattern));
                }
                catch (Exception)
                {
                    this.failedReportFilePatterns.Add(reportFilePattern);
                }
            }

            this.TargetDirectory = targetDirectory;
            this.HistoryDirectory = historyDirectory;

            if (reportTypes.Any())
            {
                this.ReportTypes = reportTypes;
            }
            else
            {
                this.ReportTypes = new[] { "Html" };
            }

            this.SourceDirectories = sourceDirectories;
            this.AssemblyFilters = assemblyFilters;
            this.ClassFilters = classFilters;
            this.FileFilters = fileFilters;

            if (verbosityLevel != null)
            {
                VerbosityLevel parsedVerbosityLevel = VerbosityLevel.Verbose;
                this.verbosityLevelValid = Enum.TryParse<VerbosityLevel>(verbosityLevel, true, out parsedVerbosityLevel);
                this.VerbosityLevel = parsedVerbosityLevel;
            }
        }

        /// <summary>
        /// Gets the report builder factory.
        /// </summary>
        public IReportBuilderFactory ReportBuilderFactory { get; }

        /// <summary>
        /// Gets the report files.
        /// </summary>
        public IEnumerable<string> ReportFiles => this.reportFiles;

        /// <summary>
        /// Gets the target directory.
        /// </summary>
        public string TargetDirectory { get; }

        /// <summary>
        /// Gets the history directory.
        /// </summary>
        public string HistoryDirectory { get; }

        /// <summary>
        /// Gets the type of the report.
        /// </summary>
        public IEnumerable<string> ReportTypes { get; }

        /// <summary>
        /// Gets the source directories.
        /// </summary>
        public IEnumerable<string> SourceDirectories { get; }

        /// <summary>
        /// Gets the assembly filters.
        /// </summary>
        public IEnumerable<string> AssemblyFilters { get; }

        /// <summary>
        /// Gets the class filters.
        /// </summary>
        public IEnumerable<string> ClassFilters { get; }

        /// <summary>
        /// Gets the file filters.
        /// </summary>
        public IEnumerable<string> FileFilters { get; }

        /// <summary>
        /// Gets the verbosity level.
        /// </summary>
        public VerbosityLevel VerbosityLevel { get; }

        /// <summary>
        /// Validates all parameters.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if all parameters are in a valid state; otherwise <c>false</c>.
        /// </returns>
        internal bool Validate()
        {
            bool result = true;

            if (this.failedReportFilePatterns.Count > 0)
            {
                foreach (var failedReportFilePattern in this.failedReportFilePatterns)
                {
                    Logger.ErrorFormat(Resources.FailedReportFilePattern, failedReportFilePattern);
                }

                result &= false;
            }

            if (!this.ReportFiles.Any())
            {
                Logger.Error(Resources.NoReportFiles);
                result &= false;
            }
            else
            {
                foreach (var file in this.ReportFiles)
                {
                    if (!File.Exists(file))
                    {
                        Logger.ErrorFormat(Resources.NotExistingReportFile, file);
                        result &= false;
                    }
                }
            }

            if (string.IsNullOrEmpty(this.TargetDirectory))
            {
                Logger.Error(Resources.NoTargetDirectory);
                result &= false;
            }
            else if (!Directory.Exists(this.TargetDirectory))
            {
                try
                {
                    Directory.CreateDirectory(this.TargetDirectory);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat(Resources.TargetDirectoryCouldNotBeCreated, this.TargetDirectory, ex.Message);
                    result &= false;
                }
            }

            if (!string.IsNullOrEmpty(this.HistoryDirectory) && !Directory.Exists(this.HistoryDirectory))
            {
                try
                {
                    Directory.CreateDirectory(this.HistoryDirectory);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat(Resources.HistoryDirectoryCouldNotBeCreated, this.HistoryDirectory, ex.Message);
                    result &= false;
                }
            }

            var availableReportTypes = this.ReportBuilderFactory.GetAvailableReportTypes();

            foreach (var reportType in this.ReportTypes)
            {
                if (!availableReportTypes.Contains(reportType, StringComparer.OrdinalIgnoreCase))
                {
                    Logger.ErrorFormat(Resources.UnknownReportType, reportType);
                    result &= false;
                }
            }

            foreach (var directory in this.SourceDirectories)
            {
                if (!Directory.Exists(directory))
                {
                    Logger.ErrorFormat(Resources.SourceDirectoryDoesNotExist, directory);
                    result &= false;
                }
            }

            foreach (var filter in this.AssemblyFilters)
            {
                if (string.IsNullOrEmpty(filter)
                    || (!filter.StartsWith("+", StringComparison.OrdinalIgnoreCase)
                        && !filter.StartsWith("-", StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.ErrorFormat(Resources.InvalidFilter, filter);
                    result &= false;
                }
            }

            if (!this.verbosityLevelValid)
            {
                Logger.Error(Resources.UnknownVerbosityLevel);
                result &= false;
            }

            return result;
        }
    }
}
