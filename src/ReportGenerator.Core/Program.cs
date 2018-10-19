using System;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Plugin;
using Palmmedia.ReportGenerator.Core.Reporting;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Command line access to the ReportBuilder.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main method.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Return code indicating success/failure.</returns>
        public static int Main(string[] args)
        {
            args = args.Select(a => a.EndsWith("\"", StringComparison.OrdinalIgnoreCase) ? a.TrimEnd('\"') + "\\" : a).ToArray();

            var reportConfigurationBuilder = new ReportConfigurationBuilder();
            ReportConfiguration configuration = reportConfigurationBuilder.Create(args);

            if (args.Length < 2)
            {
                var help = new Help(new ReportBuilderFactory(new ReflectionPluginLoader(configuration.Plugins)));
                help.ShowHelp();

                return 1;
            }

            return new Generator().GenerateReport(configuration) ? 0 : 1;
        }
    }
}
