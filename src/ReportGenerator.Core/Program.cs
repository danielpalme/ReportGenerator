using System;
using System.Linq;
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
            var reportConfigurationBuilder = new ReportConfigurationBuilder(new ReportBuilderFactory(new ReflectionPluginLoader()));
            if (args.Length < 2)
            {
                reportConfigurationBuilder.ShowHelp();
                return 1;
            }

            args = args.Select(a => a.EndsWith("\"", StringComparison.OrdinalIgnoreCase) ? a.TrimEnd('\"') + "\\" : a).ToArray();

            ReportConfiguration configuration = reportConfigurationBuilder.Create(args);
            return new Generator().GenerateReport(configuration) ? 0 : 1;
        }
    }
}
