using Microsoft.Testing.Platform.Builder;

namespace Palmmedia.ReportGenerator.Mtp
{
    public static class TestingPlatformBuilderHook
    {
        /// <summary>
        /// Adds ReportGenerator extension support to the Testing Platform Builder.
        /// </summary>
        /// <param name="testApplicationBuilder">The test application builder.</param>
        /// <param name="args">The command line arguments.</param>
        public static void AddExtensions(ITestApplicationBuilder testApplicationBuilder, string[] args)
        {
            testApplicationBuilder.AddReportGeneratorExtensionProvider();
        }
    }
}
