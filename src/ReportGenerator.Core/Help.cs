using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Reporting;

namespace Palmmedia.ReportGenerator.Core
{
    /// <summary>
    /// Prints help on the command line.
    /// </summary>
    internal class Help
    {
        /// <summary>
        /// The report builder factory.
        /// </summary>
        private readonly IReportBuilderFactory reportBuilderFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Help"/> class.
        /// </summary>
        /// <param name="reportBuilderFactory">The report builder factory.</param>
        internal Help(IReportBuilderFactory reportBuilderFactory)
        {
            this.reportBuilderFactory = reportBuilderFactory ?? throw new ArgumentNullException(nameof(reportBuilderFactory));
        }

        /// <summary>
        /// Shows the help of the program.
        /// </summary>
        internal void ShowHelp()
        {
            var availableReportTypes = this.reportBuilderFactory.GetAvailableReportTypes();

            Console.WriteLine();
            Console.WriteLine("ReportGenerator " + typeof(ReportConfigurationBuilder).Assembly.GetName().Version);

            AssemblyCopyrightAttribute assemblyCopyrightAttribute = typeof(ReportConfigurationBuilder).Assembly
                .GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)
                .Cast<AssemblyCopyrightAttribute>()
                .FirstOrDefault();

            if (assemblyCopyrightAttribute != null)
            {
                Console.WriteLine(assemblyCopyrightAttribute.Copyright);
            }

            Console.WriteLine();
            Console.WriteLine(Properties.Help.Parameters);
            Console.WriteLine("    " + Properties.Help.Parameters1);
            Console.WriteLine("    " + Properties.Help.Parameters2);
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    " + Properties.Help.Parameters3, string.Join("|", availableReportTypes.Take(3).Union(new[] { "..." }))));
            Console.WriteLine("    " + Properties.Help.Parameters12);
            Console.WriteLine("    " + Properties.Help.Parameters5);
            Console.WriteLine("    " + Properties.Help.Parameters11);
            Console.WriteLine("    " + Properties.Help.Parameters6);
            Console.WriteLine("    " + Properties.Help.Parameters7);
            Console.WriteLine("    " + Properties.Help.Parameters8);
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    " + Properties.Help.Parameters9, string.Join("|", Enum.GetNames(typeof(VerbosityLevel)))));
            Console.WriteLine("    " + Properties.Help.Parameters10);

            Console.WriteLine();
            Console.WriteLine(Properties.Help.Explanations);
            Console.WriteLine("    " + Properties.Help.Explanations1);
            Console.WriteLine("    " + Properties.Help.Explanations2);
            Console.WriteLine("    " + Properties.Help.Explanations3);
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    " + Properties.Help.ReportTypeValues, string.Join(", ", availableReportTypes)));
            Console.WriteLine("    " + Properties.Help.Explanations15);
            Console.WriteLine("    " + Properties.Help.Explanations5);
            Console.WriteLine("    " + Properties.Help.Explanations6);
            Console.WriteLine("    " + Properties.Help.Explanations7);
            Console.WriteLine("    " + Properties.Help.Explanations14);
            Console.WriteLine("    " + Properties.Help.Explanations8);
            Console.WriteLine("    " + Properties.Help.Explanations9);
            Console.WriteLine("    " + Properties.Help.Explanations10);
            Console.WriteLine("    " + Properties.Help.Explanations11);
            Console.WriteLine("    " + Properties.Help.Explanations12);
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    " + Properties.Help.VerbosityValues, string.Join(", ", Enum.GetNames(typeof(VerbosityLevel)))));
            Console.WriteLine("    " + Properties.Help.Explanations13);

            Console.WriteLine();
            Console.WriteLine(Properties.Help.DefaultValues);
            Console.WriteLine("   -reporttypes:Html");
            Console.WriteLine("   -assemblyfilters:+*");
            Console.WriteLine("   -classfilters:+*");
            Console.WriteLine("   -filefilters:+*");
            Console.WriteLine("   -verbosity:" + VerbosityLevel.Verbose);

            Console.WriteLine();
            Console.WriteLine(Properties.Help.Examples);
            Console.WriteLine("   \"-reports:coverage.xml\" \"-targetdir:C:\\report\"");
            Console.WriteLine("   \"-reports:target\\*\\*.xml\" \"-targetdir:C:\\report\" -reporttypes:Latex;HtmlSummary -tag:v1.4.5");
            Console.WriteLine("   \"-reports:coverage1.xml;coverage2.xml\" \"-targetdir:report\" \"-sourcedirs:C:\\MyProject\" -plugins:CustomReports.dll");
            Console.WriteLine("   \"-reports:coverage.xml\" \"-targetdir:C:\\report\" \"-assemblyfilters:+Included;-Excluded.*\"");
        }
    }
}