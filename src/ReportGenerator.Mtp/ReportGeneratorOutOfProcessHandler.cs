using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Configurations;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestHostControllers;
using Microsoft.Testing.Platform.Logging;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.Services;

namespace Palmmedia.ReportGenerator.Mtp
{
    internal sealed class ReportGeneratorOutOfProcessHandler : IDataConsumer, ITestHostProcessLifetimeHandler
    {
        private readonly IExtension extension;

        private readonly ILoggerFactory loggerFactory;

        private readonly IOutputDevice outputDevice;

        private readonly ICommandLineOptions commandLineOptions;

        private readonly IConfiguration configuration;

        private readonly bool isEnabled;

        public string Uid => "ReportGenerator.Mtp.OutOfProcess";

        public string Version => typeof(ReportGeneratorExtension).Assembly.GetName().Version?.ToString() ?? "1.0.0";

        public string DisplayName => "ReportGenerator Out-Of-Process Handler";

        public string Description => "Apply ReportGenerator settings when test session ends";

        public Task<bool> IsEnabledAsync() => Task.FromResult(this.isEnabled);

        public Type[] DataTypesConsumed => [typeof(TestCoverageReportMessage)];

        private readonly ITestCoverageResult coverage;

        public ReportGeneratorOutOfProcessHandler(
            IExtension extension,
            ILoggerFactory loggerFactory,
            IOutputDevice outputDevice,
            ICommandLineOptions commandLineOptions,
            IConfiguration configuration,
            ITestCoverageResult coverage)
        {
            this.extension = extension;
            this.loggerFactory = loggerFactory;
            this.outputDevice = outputDevice;
            this.commandLineOptions = commandLineOptions;
            this.configuration = configuration;
            this.isEnabled = commandLineOptions.IsOptionSet("reportgenerator");
            this.coverage = coverage;
            Trace($"ReportGeneratorOutOfProcessHandler constructor (instance {this.GetHashCode()})");
        }

        public Task BeforeTestHostProcessStartAsync(CancellationToken cancellationToken)
        {
            Trace($"BeforeTestHostProcessStartAsync (instance {this.GetHashCode()})");
            return Task.CompletedTask;
        }

        public Task OnTestHostProcessStartedAsync(ITestHostProcessInformation testHostProcessInformation, CancellationToken cancellationToken)
        {
            Trace($"OnTestHostProcessStartedAsync (instance {this.GetHashCode()}, test host pid {testHostProcessInformation.PID})");
            return Task.CompletedTask;
        }

        public Task OnTestHostProcessExitedAsync(ITestHostProcessInformation testHostProcessInformation, CancellationToken cancellationToken)
        {
            Trace($"OnTestHostProcessExitedAsync (instance {this.GetHashCode()}, test host pid {testHostProcessInformation.PID})");
            this.TraceCoberturaFiles();
            return Task.CompletedTask;
        }

        public Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
        {
            if (value is TestCoverageReportMessage report)
            {
                Trace($"ConsumeAsync (instance {this.GetHashCode()}): {nameof(TestCoverageReportMessage)} from '{dataProducer.Uid}', format {report.Format}, path '{report.ReportPath}', exists {File.Exists(report.ReportPath)}");
            }
            else
            {
                Trace($"ConsumeAsync (instance {this.GetHashCode()}): {value.GetType().Name} from '{dataProducer.Uid}'");
            }

            return Task.CompletedTask;
        }

        // Diagnostics for https://github.com/microsoft/testfx/issues/8904: the PID tells which process
        // (test host controller or test host) runs each callback; the file listing shows whether
        // coverage reports exist when the callback runs.
        private static void Trace(string message)
            => Console.WriteLine($"ReportGenerator.MTP [{DateTime.Now:HH:mm:ss.fff} pid {Environment.ProcessId}] - {message}");

        private void TraceCoberturaFiles()
        {
            string resultsDirectory = this.configuration.GetTestResultDirectory();
            string[] files = Directory.Exists(resultsDirectory)
                ? Directory.GetFiles(resultsDirectory, "coverage.cobertura*.xml")
                : [];
            Trace($"  Cobertura files in '{resultsDirectory}': {(files.Length == 0 ? "<none>" : string.Join(", ", files.Select(Path.GetFileName)))}");
        }
    }
}