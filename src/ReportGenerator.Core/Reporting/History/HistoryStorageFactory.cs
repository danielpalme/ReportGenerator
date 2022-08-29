using System;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Plugin;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.History
{
    /// <summary>
    /// Factory of <see cref="IHistoryStorage"/>.
    /// </summary>
    internal class HistoryStorageFactory
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(HistoryStorageFactory));

        /// <summary>
        /// The plugin loader.
        /// </summary>
        private readonly IPluginLoader pluginLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryStorageFactory" /> class.
        /// </summary>
        /// <param name="pluginLoader">The plugin loader.</param>
        public HistoryStorageFactory(IPluginLoader pluginLoader)
        {
            this.pluginLoader = pluginLoader ?? throw new ArgumentNullException(nameof(pluginLoader));
        }

        /// <summary>
        /// Gets the history storage.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <returns>
        /// The history storage or <see langword="null"/> if no storage exists.
        /// </returns>
        public IHistoryStorage GetHistoryStorage(IReportConfiguration reportConfiguration)
        {
            var storages = this.pluginLoader.LoadInstancesOfType<IHistoryStorage>().ToArray();

            if (storages.Length == 1)
            {
                return storages[0];
            }
            else if (storages.Length > 1)
            {
                Logger.WarnFormat(Resources.SeveralCustomHistoryStorages);
            }
            else if (reportConfiguration.HistoryDirectory != null)
            {
                return new FileHistoryStorage(reportConfiguration.HistoryDirectory);
            }

            return null;
        }
    }
}
