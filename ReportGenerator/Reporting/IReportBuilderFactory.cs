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
        /// <param name="targetDirectory">The target directory where reports are stored.</param>
        /// <param name="reportTypes">The report types.</param>
        /// <returns>The report builders.</returns>
        IEnumerable<IReportBuilder> GetReportBuilders(string targetDirectory, IEnumerable<string> reportTypes);
    }
}
