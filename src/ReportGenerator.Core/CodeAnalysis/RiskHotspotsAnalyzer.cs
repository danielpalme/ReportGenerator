using System;
using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.CodeAnalysis
{
    /// <summary>
    /// Analyses the method metrics for risk hotspots based on exceeded thresholds.
    /// </summary>
    internal class RiskHotspotsAnalyzer : IRiskHotspotsAnalyzer
    {
        /// <summary>
        /// Indicates whether risk hotspots should be disabled or not.
        /// </summary>
        private readonly bool disabled;

        /// <summary>
        /// The thresholds of the various metrics.
        /// </summary>
        private readonly Dictionary<string, decimal> thresholdsByMetricName;

        /// <summary>
        /// The assembly filters for risk hotspots.
        /// </summary>
        private readonly IFilter riskHotSpotAssemblyFilter;

        /// <summary>
        /// The class filters for risk hotspots.
        /// </summary>
        private readonly IFilter riskHotSpotClassFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="RiskHotspotsAnalyzer"/> class.
        /// </summary>
        /// <param name="riskHotspotsAnalysisThresholds">The metric thresholds.</param>
        /// <param name="disableRiskHotspots">Indicates whether risk hotspots should be disabled or not.</param>
        public RiskHotspotsAnalyzer(
            RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds,
            bool disableRiskHotspots)
            : this(
                  riskHotspotsAnalysisThresholds,
                  disableRiskHotspots,
                  new DefaultFilter(Array.Empty<string>()),
                  new DefaultFilter(Array.Empty<string>()))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RiskHotspotsAnalyzer"/> class.
        /// </summary>
        /// <param name="riskHotspotsAnalysisThresholds">The metric thresholds.</param>
        /// <param name="disableRiskHotspots">Indicates whether risk hotspots should be disabled or not.</param>
        /// <param name="riskHotSpotAssemblyFilter">The class filter.</param>
        /// <param name="riskHotSpotClassFilter">The file filter.</param>
        public RiskHotspotsAnalyzer(
            RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds,
            bool disableRiskHotspots,
            IFilter riskHotSpotAssemblyFilter,
            IFilter riskHotSpotClassFilter)
        {
            this.disabled = disableRiskHotspots;

            this.thresholdsByMetricName = new Dictionary<string, decimal>()
            {
                { ReportResources.CyclomaticComplexity, riskHotspotsAnalysisThresholds.MetricThresholdForCyclomaticComplexity },
                { ReportResources.NPathComplexity, riskHotspotsAnalysisThresholds.MetricThresholdForNPathComplexity },
                { ReportResources.CrapScore, riskHotspotsAnalysisThresholds.MetricThresholdForCrapScore }
            };

            this.riskHotSpotAssemblyFilter = riskHotSpotAssemblyFilter;
            this.riskHotSpotClassFilter = riskHotSpotClassFilter;
        }

        /// <summary>
        /// Performs a risk hotspot analysis on the given assemblies.
        /// </summary>
        /// <param name="assemblies">The assemlies to analyze.</param>
        /// <returns>The risk hotspot analysis result.</returns>
        public RiskHotspotAnalysisResult PerformRiskHotspotAnalysis(IEnumerable<Assembly> assemblies)
        {
            var riskHotspots = new List<RiskHotspot>();

            if (this.disabled)
            {
                return new RiskHotspotAnalysisResult(riskHotspots, false);
            }

            decimal threshold = -1;

            bool codeCodeQualityMetricsAvailable = false;

            foreach (var assembly in assemblies.Where(c => this.riskHotSpotAssemblyFilter.IsElementIncludedInReport(c.Name)))
            {
                foreach (var clazz in assembly.Classes.Where(c => this.riskHotSpotClassFilter.IsElementIncludedInReport(c.Name)))
                {
                    int fileIndex = 0;

                    foreach (var file in clazz.Files)
                    {
                        foreach (var methodMetric in file.MethodMetrics)
                        {
                            var codeCodeQualityMetrics = methodMetric.Metrics
                                .Where(m => m.MetricType == MetricType.CodeQuality);

                            codeCodeQualityMetricsAvailable |= codeCodeQualityMetrics.Any();

                            var statusMetrics = codeCodeQualityMetrics
                                .Select(m => new MetricStatus(m, this.thresholdsByMetricName.TryGetValue(m.Name, out threshold) && m.Value > threshold))
                                .ToArray();

                            if (statusMetrics.Any(m => m.Exceeded))
                            {
                                riskHotspots.Add(new RiskHotspot(assembly, clazz, methodMetric, statusMetrics, fileIndex));
                            }
                        }

                        fileIndex++;
                    }
                }
            }

            var result = new RiskHotspotAnalysisResult(
                riskHotspots
                .OrderByDescending(r => r.StatusMetrics.Where(m => m.Exceeded).Max(m => m.Metric.Value))
                .ToList(),
                codeCodeQualityMetricsAvailable);

            return result;
        }
    }
}