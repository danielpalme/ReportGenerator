using System;
using System.Collections.Generic;
using System.Linq;
using DotNetConfig;
using Microsoft.Extensions.Configuration;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Provides configuration values from dotnet-config sections and exposes them through the
    /// Microsoft.Extensions.Configuration provider pipeline.
    /// </summary>
    /// <remarks>Loads writable, non-obsolete properties from the riskHotspotsAnalysisThresholds,
    /// MinimumCoverageThresholds, and settings subsections, maps property names to lower-camel-case keys, and ignores
    /// entries with the literal value "null".</remarks>
    public sealed class DotNetConfigConfigurationProvider : ConfigurationProvider
    {
        private const string NullString = "null";

        /// <inheritdoc />
        public override void Load()
        {
            var riskHotspotsAnalysisThresholdsPropertyNames = typeof(RiskHotspotsAnalysisThresholds).GetProperties()
                .Where(p => p.CanWrite && p.GetSetMethod() != null && !p.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any())
                .Select(p => p.Name)
                .ToList();

            this.ApplySettingsFromSubsection("RiskHotspotsAnalysisThresholds", riskHotspotsAnalysisThresholdsPropertyNames);

            var minimumCoverageThresholdsPropertyNames = typeof(MinimumCoverageThresholds).GetProperties()
                .Where(p => p.CanWrite && p.GetSetMethod() != null && !p.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any())
                .Select(p => p.Name)
                .ToList();

            this.ApplySettingsFromSubsection("MinimumCoverageThresholds", minimumCoverageThresholdsPropertyNames);

            var settingsPropertyNames = typeof(Settings).GetProperties()
                .Where(p => p.CanWrite && p.GetSetMethod() != null && !p.GetCustomAttributes(typeof(ObsoleteAttribute), false).Any())
                .Select(p => p.Name)
                .ToList();

            this.ApplySettingsFromSubsection("Settings", settingsPropertyNames);
        }

        private static string FirstLetterToLower(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        private void ApplySettingsFromSubsection(string section, List<string> properties)
        {
            var config = Config.Build().GetSection(DotNetConfigSettingNames.SectionName, section);

            this.ApplySettings(config, section, properties);
        }

        private void ApplySettings(ConfigSection config, string section, List<string> properties)
        {
            foreach (string property in properties)
            {
                string key = FirstLetterToLower(property);
                if (config.TryGetString(key, out string value))
                {
                    if (value.Equals(NullString, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    this.Data[$"{section}:{key}"] = value;
                }
            }
        }
    }
}