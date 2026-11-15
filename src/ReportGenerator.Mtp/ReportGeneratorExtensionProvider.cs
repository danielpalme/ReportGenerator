using System;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Logging;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.Services;

namespace Palmmedia.ReportGenerator.Mtp
{
    public static class ReportGeneratorExtensionProvider
    {
        public static void AddReportGeneratorExtensionProvider(this ITestApplicationBuilder builder, bool ignoreIfNotSupported = false)
        {
            // TODO: Remove
            //builder.TestHostControllers.AddEnvironmentVariableProvider(
            //    serviceProvider => new SettingsEnvironmentVariablesProvider(serviceProvider.GetCommandLineOptions()));

            var extension = new ReportGeneratorExtension();

            builder.CommandLine.AddProvider(() => new ReportGeneratorCommandLineOptions(extension));

            // create a single handler instance so the same object handles all lifetimes/data
            ReportGeneratorOutOfProcessHandler handlerFactory(IServiceProvider sp) =>
                new ReportGeneratorOutOfProcessHandler(
                    extension,
                    sp.GetService<ILoggerFactory>(),
                    sp.GetService<IOutputDevice>(),
                    sp.GetCommandLineOptions(),
                    sp.GetService<ITestCoverageResult>());

            //builder.TestHost.AddTestHostApplicationLifetime(handlerFactory);
            builder.TestHost.AddDataConsumer(handlerFactory);
            builder.TestHostControllers.AddProcessLifetimeHandler(handlerFactory);
        }
    }
}
