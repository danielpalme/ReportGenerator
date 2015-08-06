namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Interface to filter assemblies and classes based on their name during report generation.
    /// This can be used to include only a subset of all assemblies/classes in the report.
    /// </summary>
    internal interface IFilter
    {
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
