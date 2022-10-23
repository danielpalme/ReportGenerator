using System.Reflection;
using McMaster.NETCore.Plugins;
using Palmmedia.ReportGenerator.Core.Plugin;
using Palmmedia.ReportGenerator.Core.Reporting;
using Palmmedia.ReportGenerator.Core.Reporting.History;

namespace ReportGenerator.DotnetCorePluginLoader
{
    /// <summary>
    /// Aassembly loader for .NET Core.
    /// </summary>
    public class DotNetCoreAssemblyLoader : IAssemblyLoader
    {
        /// <summary>
        /// Loads the assembly with the given name.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        /// <returns>The assembly.</returns>
        public Assembly Load(string name)
        {
            PluginLoader loader = PluginLoader.CreateFromAssemblyFile(
                name,
                sharedTypes: new[] { typeof(IReportBuilder), typeof(IHistoryStorage) });

            Assembly assembly = loader.LoadDefaultAssembly();

            return assembly;
        }
    }
}
