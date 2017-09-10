using System.Linq;
using Palmmedia.ReportGenerator.Logging;
using Palmmedia.ReportGenerator.Properties;

namespace Palmmedia.ReportGenerator.Reporting.History
{
    /// <summary>
    /// Factory of <see cref="IHistoryStorage"/> based on MEF.
    /// </summary>
    internal class MefHistoryStorageFactory
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(MefHistoryStorageFactory));

        /// <summary>
        /// Gets the history storage.
        /// </summary>
        /// <param name="reportConfiguration">The report configuration.</param>
        /// <returns>
        /// The history storage or <code>null</code> if no storage exists.
        /// </returns>
        public IHistoryStorage GetHistoryStorage(ReportConfiguration reportConfiguration)
        {
            var storages = MefHelper.LoadInstancesOfType<IHistoryStorage>().ToArray();

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
