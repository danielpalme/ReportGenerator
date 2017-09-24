using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Parser.Analysis;
using Palmmedia.ReportGenerator.Properties;

namespace Palmmedia.ReportGenerator.Reporting.Rendering.RiskHotspots
{
    internal static class RiskHotspotsAnalysis
    {
        private const int DefaultMaxHotspots = 20;

        public static IEnumerable<RiskHotspot> DetectHotspots(IEnumerable<Assembly> assemblies, int maxHotspotsCount = DefaultMaxHotspots)
        {
            var hotspotsCloud = new List<RiskHotspot>();

            foreach (var assembly in assemblies)
            {
                foreach (var @class in assembly.Classes)
                {
                    var hotspot = new RiskHotspot {AssemblyShortName = assembly.ShortName, ClassName = @class.Name};
                    hotspotsCloud.Add(hotspot);

                    foreach (var method in @class.MethodMetrics)
                    {
                        var methodNameShort = method.ShortName;

                        var complexity = hotspot.Complexity;
                        var coverage = hotspot.Coverage;
                        var branchCoverage = hotspot.BranchCoverage;
                        var crapScore = hotspot.CrapScore;

                        foreach (var metrics in method.Metrics)
                        {
                            if (metrics.Name == ReportResources.CyclomaticComplexity)
                                complexity = metrics.Value;
                            else if (metrics.Name == ReportResources.SequenceCoverage)
                                coverage = metrics.Value;
                            else if (metrics.Name == ReportResources.BranchCoverage)
                                branchCoverage = metrics.Value;
                            else if (metrics.Name == ReportResources.CrapScore)
                                crapScore = metrics.Value;
                        }

                        if (crapScore > hotspot.CrapScore)
                        {
                            hotspot.MethodNameShort = methodNameShort;
                            hotspot.Complexity = complexity;
                            hotspot.Coverage = coverage;
                            hotspot.BranchCoverage = branchCoverage;
                            hotspot.CrapScore = crapScore;
                        }
                    }
                }
            }

            var sortedHotspotsCloud = hotspotsCloud.OrderByDescending(hotspot => hotspot.CrapScore).Take(maxHotspotsCount);
            return sortedHotspotsCloud;
        }
    }
}