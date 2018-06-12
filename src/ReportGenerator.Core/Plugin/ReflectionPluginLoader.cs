using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        /// Determines all available plugins from a certain type.
        /// </summary>
        /// <typeparam name="T">The plugin type.</typeparam>
        /// <returns>All available plugins.</returns>
        public IReadOnlyCollection<T> LoadInstancesOfType<T>()
        {
            var directory = new FileInfo(typeof(ReflectionPluginLoader).Assembly.Location).Directory.FullName;

            var dlls = Directory
                .GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly)
                .ToList();

            var result = new List<T>();

            foreach (var file in dlls)
            {
                try
                {
                    // Unblock files, this prevents FileLoadException (e.g. if file was extracted from a ZIP archive)
                    FileUnblocker.Unblock(file);

                    var assembly = Assembly.LoadFrom(file);

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
                    if (!file.Contains("ReportGenerator.Core.Test.dll") && !file.Contains("xunit.runner"))
                    {
                        Logger.Error(string.Format(Resources.FailedToLoadPlugins, file));
                    }
                }
            }

            return result;
        }
    }
}
