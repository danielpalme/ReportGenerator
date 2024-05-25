using System;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Licensing;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Properties;
using Palmmedia.ReportGenerator.Core.Reporting;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Validates an <see cref="IReportConfiguration"/> to verify all user input is applicable and correct.
    /// </summary>
    internal class ReportConfigurationValidator
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(ReportConfiguration));

        /// <summary>
        /// The report builder factory (required to determine if all requested formats are supported).
        /// </summary>
        private readonly IReportBuilderFactory reportBuilderFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportConfigurationValidator"/> class.
        /// </summary>
        /// <param name="reportBuilderFactory">The report builder factory.</param>
        internal ReportConfigurationValidator(IReportBuilderFactory reportBuilderFactory)
        {
            this.reportBuilderFactory = reportBuilderFactory;
        }

        /// <summary>
        /// Validates all parameters.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <returns>
        ///   <c>true</c> if all parameters are in a valid state; otherwise <c>false</c>.
        /// </returns>
        internal bool Validate(IReportConfiguration reportConfiguration)
        {
            if (reportConfiguration.License != null
                && !reportConfiguration.License.IsValid())
            {
                Logger.WarnFormat(Resources.InvalidLicense);
            }

            if (reportConfiguration.InvalidReportFilePatterns.Count > 0)
            {
                foreach (var failedReportFilePattern in reportConfiguration.InvalidReportFilePatterns)
                {
                    if (failedReportFilePattern.Contains("*"))
                    {
                        Logger.WarnFormat(Resources.FailedReportFilePattern, failedReportFilePattern);
                    }
                    else
                    {
                        Logger.WarnFormat(Resources.FailedReportFile, failedReportFilePattern, new FileInfo(failedReportFilePattern).FullName);
                    }
                }
            }

            bool result = true;

            foreach (var file in reportConfiguration.Plugins)
            {
                if (!File.Exists(file))
                {
                    Logger.ErrorFormat(Resources.NotExistingPlugin, file);
                    result &= false;
                }
            }

            if (!reportConfiguration.ReportFiles.Any())
            {
                Logger.Error(Resources.NoReportFiles);
                result &= false;
            }
            else
            {
                foreach (var file in reportConfiguration.ReportFiles)
                {
                    if (!File.Exists(file))
                    {
                        Logger.ErrorFormat(Resources.NotExistingReportFile, file);
                        result &= false;
                    }
                }
            }

            if (string.IsNullOrEmpty(reportConfiguration.TargetDirectory))
            {
                Logger.Error(Resources.NoTargetDirectory);
                result &= false;
            }
            else if (!Directory.Exists(reportConfiguration.TargetDirectory))
            {
                try
                {
                    Directory.CreateDirectory(reportConfiguration.TargetDirectory);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat(Resources.TargetDirectoryCouldNotBeCreated, reportConfiguration.TargetDirectory, ex.GetExceptionMessageForDisplay());
                    result &= false;
                }
            }

            if (!string.IsNullOrEmpty(reportConfiguration.HistoryDirectory) && !Directory.Exists(reportConfiguration.HistoryDirectory))
            {
                try
                {
                    Directory.CreateDirectory(reportConfiguration.HistoryDirectory);
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat(Resources.HistoryDirectoryCouldNotBeCreated, reportConfiguration.HistoryDirectory, ex.GetExceptionMessageForDisplay());
                    result &= false;
                }
            }

            foreach (var sourceDirectory in reportConfiguration.SourceDirectories)
            {
                if (!Directory.Exists(sourceDirectory))
                {
                    Logger.ErrorFormat(Resources.SourceDirectoryDoesNotExist, sourceDirectory);
                    result &= false;
                }
            }

            var availableReportTypes = this.reportBuilderFactory.GetAvailableReportTypes();

            foreach (var reportType in reportConfiguration.ReportTypes)
            {
                if (!availableReportTypes.Contains(reportType, StringComparer.OrdinalIgnoreCase))
                {
                    Logger.ErrorFormat(Resources.UnknownReportType, reportType);
                    result &= false;
                }
            }

            foreach (var filter in reportConfiguration.AssemblyFilters)
            {
                if (string.IsNullOrEmpty(filter)
                    || (!filter.StartsWith("+", StringComparison.OrdinalIgnoreCase)
                        && !filter.StartsWith("-", StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.ErrorFormat(Resources.InvalidFilter, filter);
                    result &= false;
                }
            }

            foreach (var filter in reportConfiguration.ClassFilters)
            {
                if (string.IsNullOrEmpty(filter)
                    || (!filter.StartsWith("+", StringComparison.OrdinalIgnoreCase)
                        && !filter.StartsWith("-", StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.ErrorFormat(Resources.InvalidFilter, filter);
                    result &= false;
                }
            }

            foreach (var filter in reportConfiguration.FileFilters)
            {
                if (string.IsNullOrEmpty(filter)
                    || (!filter.StartsWith("+", StringComparison.OrdinalIgnoreCase)
                        && !filter.StartsWith("-", StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.ErrorFormat(Resources.InvalidFilter, filter);
                    result &= false;
                }
            }

            foreach (var filter in reportConfiguration.RiskHotspotAssemblyFilters)
            {
                if (string.IsNullOrEmpty(filter)
                    || (!filter.StartsWith("+", StringComparison.OrdinalIgnoreCase)
                        && !filter.StartsWith("-", StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.ErrorFormat(Resources.InvalidFilter, filter);
                    result &= false;
                }
            }

            foreach (var filter in reportConfiguration.RiskHotspotClassFilters)
            {
                if (string.IsNullOrEmpty(filter)
                    || (!filter.StartsWith("+", StringComparison.OrdinalIgnoreCase)
                        && !filter.StartsWith("-", StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.ErrorFormat(Resources.InvalidFilter, filter);
                    result &= false;
                }
            }

            if (!reportConfiguration.VerbosityLevelValid)
            {
                Logger.Error(Resources.UnknownVerbosityLevel);
                result &= false;
            }

            return result;
        }
    }
}
