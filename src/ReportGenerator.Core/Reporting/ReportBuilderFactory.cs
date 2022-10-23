using System;
using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Plugin;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting
{
    /// <summary>
    /// Implementation of <see cref="IReportBuilderFactory"/>.
    /// </summary>
    internal class ReportBuilderFactory : IReportBuilderFactory
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(ReportBuilderFactory));

        /// <summary>
        /// The plugin loader.
        /// </summary>
        private readonly IPluginLoader pluginLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportBuilderFactory" /> class.
        /// </summary>
        /// <param name="pluginLoader">The plugin loader.</param>
        public ReportBuilderFactory(IPluginLoader pluginLoader)
        {
            this.pluginLoader = pluginLoader ?? throw new ArgumentNullException(nameof(pluginLoader));
        }

        /// <summary>
        /// Gets the available report types.
        /// </summary>
        /// <returns>
        /// The available report types.
        /// </returns>
        public IEnumerable<string> GetAvailableReportTypes()
        {
            var reportBuilders = this.pluginLoader.LoadInstancesOfType<IReportBuilder>();

            return reportBuilders
                .Select(r => r.ReportType)
                .Distinct()
                .OrderBy(r => r)
                .ToArray();
        }

        /// <summary>
        /// Gets the report builders that correspond to the given <paramref name="reportContext" />.
        /// </summary>
        /// <param name="reportContext">The report context.</param>
        /// <returns>
        /// The report builders.
        /// </returns>
        public IEnumerable<IReportBuilder> GetReportBuilders(IReportContext reportContext)
        {
            Logger.DebugFormat(Resources.InitializingReportBuilders, string.Join(", ", reportContext.ReportConfiguration.ReportTypes));

            var reportBuilders = this.pluginLoader.LoadInstancesOfType<IReportBuilder>()
                .Where(r => reportContext.ReportConfiguration.ReportTypes.Contains(r.ReportType, StringComparer.OrdinalIgnoreCase))
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
                        .Where(r => r.GetType().Assembly.GetName().Name != "ReportGenerator.Core")
                        .ToArray();

                    foreach (var reportBuilder in nonDefaultParsers)
                    {
                        result.Add(reportBuilder);
                    }

                    if (nonDefaultParsers.Length > 1)
                    {
                        Logger.WarnFormat(Resources.SeveralCustomReportBuildersWithSameReportType, reportBuilderGroup.Key);
                    }

                    if (nonDefaultParsers.Length < reportBuilderGroup.Count())
                    {
                        Logger.WarnFormat(Resources.DefaultReportBuilderReplaced, reportBuilderGroup.Key);
                    }
                }
            }

            foreach (var reportBuilder in result)
            {
                reportBuilder.ReportContext = reportContext;
            }

            return result;
        }
    }
}
