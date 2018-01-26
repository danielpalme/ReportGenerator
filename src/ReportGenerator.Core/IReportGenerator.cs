using Palmmedia.ReportGenerator.Core.Reporting;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Provides report generation capabilities.
    /// </summary>
    public interface IReportGenerator
    {
        /// <summary>
        /// Generates a report using given configuration.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <returns>True if the report generation succeeded, otherwise false.</returns>
        bool GenerateReport(IReportConfiguration reportConfiguration);
    }
}
