using System.Collections.Generic;

namespace Palmmedia.ReportGenerator.Core.Plugin
{
    /// <summary>
    /// Interface for loading plugins at runtime.
    /// </summary>
    internal interface IPluginLoader
    {
        /// <summary>
        /// Determines all available plugins from a certain type.
        /// </summary>
        /// <typeparam name="T">The plugin type.</typeparam>
        /// <returns>All available plugins.</returns>
        IReadOnlyCollection<T> LoadInstancesOfType<T>();
    }
}
