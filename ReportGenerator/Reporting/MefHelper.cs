using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using Palmmedia.ReportGenerator.Logging;
using Palmmedia.ReportGenerator.Properties;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Helper methods for MEF.
    /// </summary>
    internal static class MefHelper
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(MefHelper));

        /// <summary>
        /// Creates instances of all matching types found in all DLLs.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>The instances.</returns>
        public static IEnumerable<T> LoadInstancesOfType<T>()
        {
            AggregateCatalog aggregateCatalog = new AggregateCatalog();

            foreach (var file in new FileInfo(typeof(MefHelper).Assembly.Location).Directory.EnumerateFiles("*.dll"))
            {
                try
                {
                    // Unblock files, this prevents FileLoadException (e.g. if file was extracted from a ZIP archive)
                    FileUnblocker.Unblock(file.FullName);

                    var assemblyCatalog = new AssemblyCatalog(Assembly.LoadFrom(file.FullName));
                    assemblyCatalog.Parts.ToArray(); // This may throw ReflectionTypeLoadException 
                    aggregateCatalog.Catalogs.Add(assemblyCatalog);
                }
                catch (FileLoadException)
                {
                    Logger.ErrorFormat(Resources.FileLoadError, file.FullName);
                    throw;
                }
                catch (ReflectionTypeLoadException ex)
                {
                    if (!file.Name.Equals("ICSharpCode.NRefactory.Cecil.dll", StringComparison.OrdinalIgnoreCase))
                    {
                        string errors = string.Join(Environment.NewLine, ex.LoaderExceptions.Select(e => "-" + e.Message));
                        Logger.ErrorFormat(Resources.FileReflectionLoadError, file.FullName, errors);
                    }

                    // Ignore assemblies that throw this exception
                }
            }

            using (var container = new CompositionContainer(aggregateCatalog))
            {
                return container.GetExportedValues<T>();
            }
        }
    }
}
