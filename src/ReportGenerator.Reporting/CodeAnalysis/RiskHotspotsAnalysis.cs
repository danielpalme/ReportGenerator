using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Parser.Analysis;
using Palmmedia.ReportGenerator.Properties;
using Palmmedia.ReportGenerator.Reporting.Properties;

namespace Palmmedia.ReportGenerator.Reporting.CodeAnalysis
{
    /// <summary>
    /// Analyses the method metrics for risk hotspots based on exceeded thresholds.
    /// </summary>
    internal static class RiskHotspotsAnalysis
    {
        /// <summary>
        /// The thresholds of the various metrics.
        /// </summary>
        private static readonly Dictionary<string, decimal> ThresholdsByMetricName = new Dictionary<string, decimal>()
        {
            { ReportResources.CyclomaticComplexity, Settings.Default.MetricThresholdForCyclomaticComplexity },
            { ReportResources.NPathComplexity, Settings.Default.MetricThresholdForNPathComplexity },
            { ReportResources.CrapScore, Settings.Default.MetricThresholdForCrapScore }
        };

        /// <summary>
        /// Determines the risk hotspots in the code base.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns>The risk hotspots.</returns>
        public static IEnumerable<RiskHotspot> DetectHotspotsByMetricName(IEnumerable<Assembly> assemblies)
        {
            var riskHotspots = new List<RiskHotspot>();
            decimal threshold = -1;

            foreach (var assembly in assemblies)
            {
                foreach (var clazz in assembly.Classes)
                {
                    foreach (var methodMetric in clazz.MethodMetrics)
                    {
                        var statusMetrics = methodMetric.Metrics
                            .Where(m => m.MetricType == MetricType.CodeQuality)
                            .Select(m => new MetricStatus(m, ThresholdsByMetricName.TryGetValue(m.Name, out threshold) && m.Value > threshold))
                            .ToArray();

                        if (statusMetrics.Any(m => m.Exceeded))
                        {
                            riskHotspots.Add(new RiskHotspot(assembly, clazz, methodMetric, statusMetrics));
                        }
                    }
                }
            }

            return riskHotspots;
        }
    }
}