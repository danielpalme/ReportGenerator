namespace Palmmedia.ReportGenerator.Core.Parser.Filtering
{
    /// <summary>
    /// Interface to filter assemblies and classes based on their name during report generation.
    /// This can be used to include only a subset of all assemblies/classes in the report.
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Gets a value indicating whether custom filter are applied.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if custom filter are applied; otherwise, <c>false</c>.
        /// </returns>
        bool HasCustomFilters { get; }

        /// <summary>
        /// Determines whether the given element should be included in the report.
        /// </summary>
        /// <param name="name">Name of the element.</param>
        /// <returns>
        ///   <c>true</c> if element should be included in the report; otherwise, <c>false</c>.
        /// </returns>
        bool IsElementIncludedInReport(string name);
    }
}
