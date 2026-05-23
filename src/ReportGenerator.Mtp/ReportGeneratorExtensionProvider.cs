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

            builder.TestHostControllers.AddProcessLifetimeHandler(serviceProvider
                => new ReportGeneratorOutOfProcessHandler(
                    extension,
                    serviceProvider.GetService<ILoggerFactory>(),
                    serviceProvider.GetService<IOutputDevice>(),
                    serviceProvider.GetCommandLineOptions()));

            builder.CommandLine.AddProvider(() => new ReportGeneratorCommandLineOptions(extension));
        }
    }
}
