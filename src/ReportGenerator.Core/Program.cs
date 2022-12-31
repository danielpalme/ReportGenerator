using System.Linq;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Plugin;
using Palmmedia.ReportGenerator.Core.Properties;
using Palmmedia.ReportGenerator.Core.Reporting;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Command line access to the ReportBuilder.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(Program));

        /// <summary>
        /// The arguments which will show the help.
        /// </summary>
        private static readonly string[] HelpArguments = new[] { "-h", "--h", "help", "-help", "/?", "?" };

        /// <summary>
        /// The main method.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>Return code indicating success/failure.</returns>
        public static int Main(string[] args)
        {
            args = NormalizeArgs(args);

            if (args.Any(a => "-verbosity:verbose".Equals(a, System.StringComparison.OrdinalIgnoreCase))
                || !args.Any(a => a.StartsWith("-verbosity:")))
            {
                Logger.Debug(Resources.Arguments);

                foreach (var arg in args)
                {
                    if (arg.StartsWith("-license:", System.StringComparison.OrdinalIgnoreCase) && arg.Length > 15)
                    {
                        Logger.Debug(" " + arg.Substring(0, 15) + "...");
                    }
                    else
                    {
                        Logger.Debug(" " + arg);
                    }
                }
            }

            var reportConfigurationBuilder = new ReportConfigurationBuilder();
            ReportConfiguration configuration = reportConfigurationBuilder.Create(args);

            if (args.Length == 1 && HelpArguments.Contains(args[0]))
            {
                var help = new Help(new ReportBuilderFactory(new ReflectionPluginLoader(configuration.Plugins)));
                help.ShowHelp();

                return 0;
            }

            return new Generator().GenerateReport(configuration) ? 0 : 1;
        }

        /// <summary>
        /// Normalizes the command line arguments.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The normalized command line arguments.</returns>
        public static string[] NormalizeArgs(string[] args)
        {
            return args.Select(a => a.Replace(@"""", string.Empty).Trim()).ToArray();
        }
    }
}
