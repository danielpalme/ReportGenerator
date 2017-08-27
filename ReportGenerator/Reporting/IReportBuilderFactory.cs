using System.Collections.Generic;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Interface for factories that create instances of <see cref="IReportBuilder"/>.
    /// </summary>
    public interface IReportBuilderFactory
    {
        /// <summary>
        /// Gets the available report types.
        /// </summary>
        /// <returns>The available report types.</returns>
        IEnumerable<string> GetAvailableReportTypes();

        /// <summary>
        /// Gets the report builders that correspond to the given <paramref name="reportTypes"/>.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <returns>
        /// The report builders.
        /// </returns>
        IEnumerable<IReportBuilder> GetReportBuilders(IReportConfiguration reportConfiguration);
    }
}
