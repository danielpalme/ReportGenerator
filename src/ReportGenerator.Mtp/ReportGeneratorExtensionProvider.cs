using System;
using System.Linq;
using System.Reflection;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Logging;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.Services;
using Microsoft.Testing.Platform.TestHostControllers;

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

            // A CompositeExtensionFactory (not a plain Func) makes MTP reuse one handler instance for
            // every registration made with it in the same process. A plain Func creates a new object
            // per registration.
            var handlerFactory = new CompositeExtensionFactory<ReportGeneratorOutOfProcessHandler>(sp =>
                new ReportGeneratorOutOfProcessHandler(
                    extension,
                    sp.GetService<ILoggerFactory>(),
                    sp.GetService<IOutputDevice>(),
                    sp.GetCommandLineOptions(),
                    sp.GetConfiguration(),
                    sp.GetService<ITestCoverageResult>()));

            // Runs in the test host controller process.
            builder.TestHostControllers.AddProcessLifetimeHandler(handlerFactory);

            // Coverlet (coverlet.MTP >= 10.1.0) publishes TestCoverageReportMessage from its own
            // OnTestHostProcessExitedAsync, i.e. on the message bus of the test host controller
            // process, after the test host process has exited. A consumer registered with
            // builder.TestHost.AddDataConsumer lives in the test host process and never sees it.
            RegisterTestHostControllerDataConsumer(builder.TestHostControllers, handlerFactory);
        }

        // VALIDATION ONLY for https://github.com/microsoft/testfx/issues/8904 - do not ship.
        // Microsoft.Testing.Platform has no public API to register an IDataConsumer in the test host
        // controller process: ITestHostControllersManager does not expose AddDataConsumer; only the
        // internal TestHostControllersManager implementation has it. Reflection is used here only to
        // prove the behavior until a supported API exists.
        private static void RegisterTestHostControllerDataConsumer(
            ITestHostControllersManager testHostControllers,
            CompositeExtensionFactory<ReportGeneratorOutOfProcessHandler> handlerFactory)
        {
            MethodInfo addDataConsumer = testHostControllers.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .SingleOrDefault(m => m.Name == "AddDataConsumer"
                    && m.IsGenericMethodDefinition
                    && m.GetParameters() is [{ ParameterType: { IsGenericType: true } parameterType }]
                    && parameterType.GetGenericTypeDefinition() == typeof(CompositeExtensionFactory<>));

            if (addDataConsumer is null)
            {
                Console.WriteLine("ReportGenerator.MTP - test host controller AddDataConsumer is not available; TestCoverageReportMessage will not be consumed.");
                return;
            }

            addDataConsumer
                .MakeGenericMethod(typeof(ReportGeneratorOutOfProcessHandler))
                .Invoke(testHostControllers, [handlerFactory]);
        }
    }
}
