using System;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Validates the risk hotspots thresholds.
    /// </summary>
    public class MaxiumRiskhotspotsThresholdsValidator
    {
        /// <summary>
        /// The minimum coverage thresholds.
        /// </summary>
        private readonly RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxiumRiskhotspotsThresholdsValidator" /> class.
        /// </summary>
        /// <param name="riskHotspotsAnalysisThresholds">The maximum risk hotspots thresholds.</param>
        public MaxiumRiskhotspotsThresholdsValidator(RiskHotspotsAnalysisThresholds riskHotspotsAnalysisThresholds)
        {
            if (riskHotspotsAnalysisThresholds == null)
            {
                throw new ArgumentNullException(nameof(riskHotspotsAnalysisThresholds));
            }

            this.riskHotspotsAnalysisThresholds = riskHotspotsAnalysisThresholds;
        }

        /// <summary>
        /// Validates the risk hotspots thresholds.
        /// </summary>
        /// <param name="riskHotspotAnalysisResult">The risk hotspots analysis.</param>
        public void Validate(RiskHotspotAnalysisResult riskHotspotAnalysisResult)
        {
            var thresholdsByMetricName = new Dictionary<string, decimal>();

            if (this.riskHotspotsAnalysisThresholds.MaximumThresholdForCyclomaticComplexity.HasValue)
            {
                thresholdsByMetricName.Add(ReportResources.CyclomaticComplexity, this.riskHotspotsAnalysisThresholds.MaximumThresholdForCyclomaticComplexity.Value);
            }

            if (this.riskHotspotsAnalysisThresholds.MaximumThresholdForCrapScore.HasValue)
            {
                thresholdsByMetricName.Add(ReportResources.CrapScore, this.riskHotspotsAnalysisThresholds.MaximumThresholdForCrapScore.Value);
            }

            if (this.riskHotspotsAnalysisThresholds.MaximumThresholdForNPathComplexity.HasValue)
            {
                thresholdsByMetricName.Add(ReportResources.NPathComplexity, this.riskHotspotsAnalysisThresholds.MaximumThresholdForNPathComplexity.Value);
            }

            if (thresholdsByMetricName.Count == 0)
            {
                return;
            }

            var errors = new List<string>();

            foreach (var riskHotspot in riskHotspotAnalysisResult.RiskHotspots)
            {
                foreach (var statusMetric in riskHotspot.StatusMetrics)
                {
                    if (!thresholdsByMetricName.TryGetValue(statusMetric.Metric.Name, out var threshold))
                    {
                        continue;
                    }

                    if (statusMetric.Metric.Value > threshold)
                    {
                        errors.Add(string.Format(
                            Resources.ErrorRiskHotspot,
                            statusMetric.Metric.Value,
                            riskHotspot.Assembly.Name,
                            riskHotspot.Class.Name,
                            riskHotspot.MethodMetric.FullName,
                            statusMetric.Metric.Name,
                            threshold));
                    }
                }
            }

            if (errors.Count > 0)
            {
                throw new RiskhotspotThresholdException(string.Join("\r\n", errors));
            }
        }
    }
}
