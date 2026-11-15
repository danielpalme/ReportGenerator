using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.CommandLine;
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
            ITestCoverageResult coverage)
        {
            Console.WriteLine("ReportGenerator.MTP - ReportGeneratorOutOfProcessHandler constructor");
            this.extension = extension;
            this.loggerFactory = loggerFactory;
            this.outputDevice = outputDevice;
            this.commandLineOptions = commandLineOptions;
            this.isEnabled = commandLineOptions.IsOptionSet("reportgenerator");
            this.coverage = coverage;
        }

        public Task BeforeTestHostProcessStartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("ReportGenerator.MTP - BeforeTestHostProcessStartAsync ");
            return Task.CompletedTask;
        }

        public Task OnTestHostProcessStartedAsync(ITestHostProcessInformation testHostProcessInformation, CancellationToken cancellationToken)
        {
            Console.WriteLine("ReportGenerator.MTP - OnTestHostProcessStartedAsync ");
            return Task.CompletedTask;
        }

        public Task OnTestHostProcessExitedAsync(ITestHostProcessInformation testHostProcessInformation, CancellationToken cancellationToken)
        {
            Console.WriteLine("ReportGenerator.MTP - OnTestHostProcessExitedAsync ");

            return Task.CompletedTask;
        }

        public Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
        {
            Console.WriteLine($"ReportGenerator.MTP - Received data of type {value.GetType().Name} from {dataProducer.Uid}");
            return Task.CompletedTask;
        }
    }
}