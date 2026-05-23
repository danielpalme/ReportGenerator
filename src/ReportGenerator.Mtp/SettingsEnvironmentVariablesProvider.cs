using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.TestHostControllers;

namespace Palmmedia.ReportGenerator.Mtp
{
    // TODO: Remove class 
    internal class SettingsEnvironmentVariablesProvider : ITestHostEnvironmentVariableProvider
    {
        private static Regex CommandLineParameterRegex { get; } = new Regex("^(?:--|/|)(?<section>(riskhotspotsanalysisthresholds|minimumcoveragethresholds|settings))(?::|__)(?<setting>.+)=(?<value>.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly ICommandLineOptions commandLineOptions;

        private readonly bool isEnabled;

        public SettingsEnvironmentVariablesProvider(ICommandLineOptions commandLineOptions)
        {
            this.commandLineOptions = commandLineOptions;
            this.isEnabled = commandLineOptions.IsOptionSet("reportgenerator");
        }
        public string Uid => "ReportGenerator.Mtp.SettingsEnvironmentVariables";

        public string Version => typeof(ReportGeneratorExtension).Assembly.GetName().Version?.ToString() ?? "1.0.0";

        public string DisplayName => "ReportGenerator set environment variables";

        public string Description => "Apply ReportGenerator settings via environment variables";

        public Task<bool> IsEnabledAsync() => Task.FromResult(this.isEnabled);

        public Task UpdateAsync(IEnvironmentVariables environmentVariables)
        {

            if (this.commandLineOptions.TryGetOptionArgumentList(CommandLineArgumentNames.CustomSettings, out var argumentList))
            {
                foreach (var argumentGroup in argumentList)
                {
                    foreach (var argument in argumentGroup.Split(';', System.StringSplitOptions.RemoveEmptyEntries))
                    {
                        var match = CommandLineParameterRegex.Match(argument);
                        if (match.Success)
                        {
                            var section = match.Groups["section"].Value;
                            var setting = match.Groups["setting"].Value;
                            var value = match.Groups["value"].Value;
                            environmentVariables.SetVariable(new EnvironmentVariable($"{section}:{setting}", value, false, false));


                            // Environment.SetEnvironmentVariable($"{section}:{setting}", value);

                            // TODO: Remove
                            Console.WriteLine($"Set environment variable: {section}__{setting}={value}");
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task<ValidationResult> ValidateTestHostEnvironmentVariablesAsync(IReadOnlyEnvironmentVariables environmentVariables)
        {
            return ValidationResult.ValidTask;
        }
    }
}