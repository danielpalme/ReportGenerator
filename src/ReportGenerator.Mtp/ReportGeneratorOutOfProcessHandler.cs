using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.TestHostControllers;
using Microsoft.Testing.Platform.Logging;
using Microsoft.Testing.Platform.OutputDevice;
using Palmmedia.ReportGenerator.Core;
using Palmmedia.ReportGenerator.Mtp.Logging;

namespace Palmmedia.ReportGenerator.Mtp
{
    internal sealed class ReportGeneratorOutOfProcessHandler : ITestHostProcessLifetimeHandler
    {
        private readonly IExtension extension;

        private readonly ILoggerFactory loggerFactory;

        private readonly IOutputDevice outputDevice;

        private readonly ICommandLineOptions commandLineOptions;

        private readonly bool isEnabled;

        public string Uid => "ReportGenerator.Mtp.OutOfProcess";

        public string Version => typeof(ReportGeneratorExtension).Assembly.GetName().Version?.ToString() ?? "1.0.0";

        public string DisplayName => "ReportGenerator Out-Of-Process Handler";

        public string Description => "Apply ReportGenerator settings when test session ends";

        public Task<bool> IsEnabledAsync() => Task.FromResult(this.isEnabled);

        public ReportGeneratorOutOfProcessHandler(
            IExtension extension,
            ILoggerFactory loggerFactory,
            IOutputDevice outputDevice,
            ICommandLineOptions commandLineOptions)
        {
            this.extension = extension;
            this.loggerFactory = loggerFactory;
            this.outputDevice = outputDevice;
            this.commandLineOptions = commandLineOptions;
            this.isEnabled = commandLineOptions.IsOptionSet("reportgenerator");
        }

        public Task BeforeTestHostProcessStartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task OnTestHostProcessStartedAsync(ITestHostProcessInformation testHostProcessInformation, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task OnTestHostProcessExitedAsync(ITestHostProcessInformation testHostProcessInformation, CancellationToken cancellationToken)
        {
            var logger = this.loggerFactory.CreateLogger("ReportGenerator");
            var loggerFactoryAdapter = new ReportGeneratorLoggerAdapterFactory(this.extension, logger, this.outputDevice);
            Core.Logging.LoggerFactory.Configure(loggerFactoryAdapter);

            var reportConfiguration = new ReportConfigurationBuilder()
               .Create(this.GetCommandLineOptions());
            new Generator().GenerateReport(reportConfiguration);

            return Task.CompletedTask;
        }

        private Dictionary<string, string> GetCommandLineOptions()
        {
            var result = new Dictionary<string, string>();

            foreach (var commandLineArgumentName in CommandLineArgumentNames.ValidNames)
            {
                if (this.commandLineOptions.TryGetOptionArgumentList(commandLineArgumentName, out var argumentList))
                {
                    if (argumentList.Length > 0)
                    {
                        result.Add(commandLineArgumentName.Replace("reportgenerator-", string.Empty), argumentList[0]);
                    }
                }
            }

            if (!result.ContainsKey("reports"))
            {
                result.Add("reports", "**/TestResults/coverage.cobertura*.xml");
            }

            if (!result.ContainsKey("targetdir"))
            {
                result.Add("targetdir", "coveragereport");
            }

            // TODO: Handle custom settings

            return result;
        }
    }
}