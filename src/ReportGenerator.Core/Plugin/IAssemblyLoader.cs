using System.Reflection;

namespace Palmmedia.ReportGenerator.Core.Plugin
{
    /// <summary>
    /// Interface for assembly loaders.
    /// </summary>
    public interface IAssemblyLoader
    {
        /// <summary>
        /// Loads the assembly with the given name.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        /// <returns>The assembly.</returns>
        Assembly Load(string name);
    }
}
