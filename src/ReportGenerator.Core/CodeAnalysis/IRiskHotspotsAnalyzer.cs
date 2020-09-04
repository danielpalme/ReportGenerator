using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

namespace Palmmedia.ReportGenerator.Core.CodeAnalysis
{
    /// <summary>
    /// Interface for risk hotspot analyzer.
    /// </summary>
    public interface IRiskHotspotsAnalyzer
    {
        /// <summary>
        /// Performs a risk hotspot analysis on the given assemblies.
        /// </summary>
        /// <param name="assemblies">The assemlies to analyze.</param>
        /// <returns>The risk hotspot analysis result.</returns>
        RiskHotspotAnalysisResult PerformRiskHotspotAnalysis(IEnumerable<Assembly> assemblies);
    }
}
