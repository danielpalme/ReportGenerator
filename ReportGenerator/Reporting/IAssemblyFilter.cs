namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Interface to filter assemblies based on their name during report generation.
    /// This can be used to include only a subset of all assemblies in the report.
    /// </summary>
    internal interface IAssemblyFilter
    {
        /// <summary>
        /// Determines whether the given assembly should be included in the report.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>
        ///   <c>true</c> if assembly should be included in the report; otherwise, <c>false</c>.
        /// </returns>
        bool IsAssemblyIncludedInReport(string assemblyName);
    }
}
