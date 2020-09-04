using System.Reflection;

namespace Palmmedia.ReportGenerator.Core.Plugin
{
    /// <summary>
    /// The default assembly loader.
    /// </summary>
    internal class DefaultAssemblyLoader : IAssemblyLoader
    {
        /// <summary>
        /// Loads the assembly with the given name.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        /// <returns>The assembly.</returns>
        public Assembly Load(string name)
        {
            return Assembly.LoadFrom(name);
        }
    }
}
