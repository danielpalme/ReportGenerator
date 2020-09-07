using System;
using System.Collections.Generic;

namespace Palmmedia.ReportGenerator.Core.CodeAnalysis
{
    /// <summary>
    /// The result of the risk hotspot analysis.
    /// </summary>
    public class RiskHotspotAnalysisResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RiskHotspotAnalysisResult"/> class.
        /// </summary>
        /// <param name="riskHotspots">The risk hotspots.</param>
        /// <param name="codeCodeQualityMetricsAvailable">Indicates whether any code quality metrics exists.</param>
        public RiskHotspotAnalysisResult(IReadOnlyCollection<RiskHotspot> riskHotspots, bool codeCodeQualityMetricsAvailable)
        {
            this.RiskHotspots = riskHotspots ?? throw new ArgumentNullException(nameof(riskHotspots));
            this.CodeCodeQualityMetricsAvailable = codeCodeQualityMetricsAvailable;
        }

        /// <summary>
        /// Gets the risk hotspots.
        /// </summary>
        public IReadOnlyCollection<RiskHotspot> RiskHotspots { get; }

        /// <summary>
        /// Gets a value indicating whether any code quality metrics exists.
        /// </summary>
        public bool CodeCodeQualityMetricsAvailable { get; }
    }
}
