using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Plugin
{
    /// <summary>
    /// Load plugins at runtime by scanning all DLLs via reflection.
    /// </summary>
    internal class ReflectionPluginLoader : IPluginLoader
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(ReflectionPluginLoader));

        /// <summary>
        /// The plugins.
        /// </summary>
        private readonly IReadOnlyCollection<string> plugins;

        /// <summary>
        /// The <see cref="IAssemblyLoader"/>.
        /// </summary>
        private readonly IAssemblyLoader assemblyLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionPluginLoader" /> class.
        /// </summary>
        /// <param name="plugins">The plugins.</param>
        public ReflectionPluginLoader(IReadOnlyCollection<string> plugins)
        {
            this.plugins = plugins ?? throw new ArgumentNullException(nameof(plugins));
            this.assemblyLoader = this.CreateAssemblyLoader();
        }

        /// <summary>
        /// Determines all available plugins from a certain type.
        /// </summary>
        /// <typeparam name="T">The plugin type.</typeparam>
        /// <returns>All available plugins.</returns>
        public IReadOnlyCollection<T> LoadInstancesOfType<T>()
        {
            var result = new List<T>();

            var internalPluginTypes = typeof(ReflectionPluginLoader).Assembly.GetExportedTypes()
                        .Where(t => typeof(T).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

            foreach (var pluginType in internalPluginTypes)
            {
                try
                {
                    result.Add((T)Activator.CreateInstance(pluginType));
                }
                catch (Exception)
                {
                    Logger.Error(string.Format(Resources.FailedToInstantiatePlugin, pluginType.Name));
                }
            }

            if (this.plugins.Count == 0)
            {
                return result;
            }

            foreach (var plugin in this.plugins)
            {
                try
                {
                    // Unblock files, this prevents FileLoadException (e.g. if file was extracted from a ZIP archive)
                    FileUnblocker.Unblock(plugin);

                    var assembly = this.assemblyLoader.Load(plugin);

                    var pluginTypes = assembly.GetExportedTypes()
                        .Where(t => typeof(T).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

                    foreach (var pluginType in pluginTypes)
                    {
                        try
                        {
                            result.Add((T)Activator.CreateInstance(pluginType));
                        }
                        catch (Exception)
                        {
                            Logger.Error(string.Format(Resources.FailedToInstantiatePlugin, pluginType.Name));
                        }
                    }
                }
                catch (Exception)
                {
                    Logger.Error(string.Format(Resources.FailedToLoadPlugins, plugin));
                }
            }

            return result;
        }

        /// <summary>
        /// Initializes the <see cref="IAssemblyLoader"/> based on the runtime (.NET full framework vs. .NET Core).
        /// </summary>
        /// <returns>The <see cref="IAssemblyLoader"/>.</returns>
        private IAssemblyLoader CreateAssemblyLoader()
        {
            string framework = Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;

            if (framework != null && framework.StartsWith(".NETCoreApp"))
            {
                var directory = new FileInfo(typeof(ReflectionPluginLoader).Assembly.Location).Directory.FullName;
                string path = Path.Combine(directory, "ReportGenerator.DotnetCorePluginLoader.dll");

                var dotnetCorePluginLoaderAssembly = Assembly.LoadFrom(path);
                var assemblyLoaderType = dotnetCorePluginLoaderAssembly.GetExportedTypes()
                        .Where(t => typeof(IAssemblyLoader).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                        .Single();
                return (IAssemblyLoader)Activator.CreateInstance(assemblyLoaderType);
            }

            return new DefaultAssemblyLoader();
        }
    }
}
