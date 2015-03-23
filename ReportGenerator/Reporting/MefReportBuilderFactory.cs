using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Palmmedia.ReportGenerator.Properties;

namespace Palmmedia.ReportGenerator.Reporting
{
    /// <summary>
    /// Implementation of <see cref="IReportBuilderFactory"/> based on MEF.
    /// </summary>
    internal class MefReportBuilderFactory : IReportBuilderFactory
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MefReportBuilderFactory));

        /// <summary>
        /// Gets the available report types.
        /// </summary>
        /// <returns>
        /// The available report types.
        /// </returns>
        public IEnumerable<string> GetAvailableReportTypes()
        {
            var reportBuilders = LoadReportBuilders();

            return reportBuilders
                .Select(r => r.ReportType)
                .Distinct()
                .OrderBy(r => r)
                .ToArray();
        }

        /// <summary>
        /// Gets the report builders that correspond to the given <paramref name="reportTypes" />.
        /// </summary>
        /// <param name="targetDirectory">The target directory where reports are stored.</param>
        /// <param name="reportTypes">The report types.</param>
        /// <returns>
        /// The report builders.
        /// </returns>
        public IEnumerable<IReportBuilder> GetReportBuilders(string targetDirectory, IEnumerable<string> reportTypes)
        {
            Logger.InfoFormat(Resources.InitializingReportBuilders, string.Join(", ", reportTypes));

            var reportBuilders = LoadReportBuilders()
                .Where(r => reportTypes.Contains(r.ReportType, StringComparer.OrdinalIgnoreCase))
                .OrderBy(r => r.ReportType)
                .ToArray();

            var result = new List<IReportBuilder>();

            foreach (var reportBuilderGroup in reportBuilders.GroupBy(r => r.ReportType))
            {
                if (reportBuilderGroup.Count() == 1)
                {
                    result.Add(reportBuilderGroup.First());
                }
                else
                {
                    var nonDefaultParsers = reportBuilderGroup
                        .Where(r => r.GetType().Assembly.GetName().Name != "ReportGenerator.Reporting")
                        .ToArray();

                    foreach (var reportBuilder in nonDefaultParsers)
                    {
                        result.Add(reportBuilder);
                    }

                    if (nonDefaultParsers.Length > 1)
                    {
                        Logger.WarnFormat(" " + Resources.SeveralCustomReportBuildersWithSameReportType, reportBuilderGroup.Key);
                    }

                    if (nonDefaultParsers.Length < reportBuilderGroup.Count())
                    {
                        Logger.WarnFormat(" " + Resources.DefaultReportBuilderReplaced, reportBuilderGroup.Key);
                    }
                }
            }

            foreach (var reportBuilder in result)
            {
                reportBuilder.TargetDirectory = targetDirectory;
            }

            return result;
        }

        /// <summary>
        /// Loads the report builders.
        /// </summary>
        /// <returns>The report builders.</returns>
        private static IEnumerable<IReportBuilder> LoadReportBuilders()
        {
            AggregateCatalog aggregateCatalog = new AggregateCatalog();

            foreach (var file in new FileInfo(typeof(MefReportBuilderFactory).Assembly.Location).Directory.EnumerateFiles("*.dll"))
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
                var reportBuilders = container.GetExportedValues<IReportBuilder>();
                return reportBuilders;
            }
        }
    }
}
