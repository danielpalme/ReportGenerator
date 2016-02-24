using Palmmedia.ReportGenerator.Reporting;
using System;
using System.Linq;

namespace Palmmedia.ReportGenerator
{
    /// <summary>
    /// Command line access to the ReportBuilder.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The main method.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Return code indicating success/failure.</returns>
        internal static int Main(string[] args)
        {
            var reportConfigurationBuilder = new ReportConfigurationBuilder(new MefReportBuilderFactory());
            if (args.Length < 2)
            {
                reportConfigurationBuilder.ShowHelp();
                return 1;
            }

            args = args.Select(a => a.EndsWith("\"", StringComparison.OrdinalIgnoreCase) ? a.TrimEnd('\"') + "\\" : a).ToArray();

            ReportConfiguration configuration = reportConfigurationBuilder.Create(args);
            var generator = new Generator();
            return generator.GenerateReport(configuration) ? 0 : 1;
        }
    }
}
