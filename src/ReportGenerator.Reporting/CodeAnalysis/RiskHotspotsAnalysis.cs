using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Reporting.CodeAnalysis
{
    /// <summary>
    /// Analyses the method metrics for risk hotspots based on exceeded thresholds.
    /// </summary>
    internal static class RiskHotspotsAnalysis
    {
        /// <summary>
        /// Threshold for cylomatic complexity.
        /// </summary>
        private const decimal MetricThresholdForCyclomaticComplexity = 15;

        /// <summary>
        /// Threshold for crap score.
        /// </summary>
        private const decimal MetricThresholdForCrapScore = 30;

        /// <summary>
        /// Threshold for NPath complexity.
        /// </summary>
        private const decimal MetricThresholdForNPathComplexity = 200;

        /// <summary>
        /// The thresholds of the various metrics.
        /// </summary>
        private static readonly Dictionary<string, decimal> ThresholdsByMetricName = new Dictionary<string, decimal>()
        {
            { ReportResources.CyclomaticComplexity, MetricThresholdForCyclomaticComplexity },
            { ReportResources.NPathComplexity, MetricThresholdForNPathComplexity },
            { ReportResources.CrapScore, MetricThresholdForCrapScore }
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

            return riskHotspots
                .OrderByDescending(r => r.StatusMetrics.Where(m => m.Exceeded).Max(m => m.Metric.Value))
                .ToList();
        }
    }
}