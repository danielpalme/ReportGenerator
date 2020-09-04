using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Core.Reporting
{
    /// <summary>
    /// The context containing configuration and runtime information of the current execution.
    /// </summary>
    public interface IReportContext
    {
        /// <summary>
        /// Gets the configuration options.
        /// </summary>
        IReportConfiguration ReportConfiguration { get; }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        Settings Settings { get; }

        /// <summary>
        /// Gets the risk hotspot analysis result.
        /// </summary>
        RiskHotspotAnalysisResult RiskHotspotAnalysisResult { get; }

        /// <summary>
        /// Gets the historic coverage elements.
        /// </summary>
        IReadOnlyCollection<HistoricCoverage> OverallHistoricCoverages { get; }
    }
}
