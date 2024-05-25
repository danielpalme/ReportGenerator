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
            Console.WriteLine("    " + Properties.Help.Parameters_Reports);
            Console.WriteLine("    " + Properties.Help.Parameters_TargetDirectory);
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    " + Properties.Help.Parameters_ReportTypes, string.Join("|", availableReportTypes.Take(3).Union(new[] { "..." }))));
            Console.WriteLine("    " + Properties.Help.Parameters_SourceDirectories);
            Console.WriteLine("    " + Properties.Help.Parameters_HistoryDirectory);
            Console.WriteLine("    " + Properties.Help.Parameters_Plugins);
            Console.WriteLine("    " + Properties.Help.Parameters_AssemblyFilters);
            Console.WriteLine("    " + Properties.Help.Parameters_ClassFilters);
            Console.WriteLine("    " + Properties.Help.Parameters_FileFilters);
            Console.WriteLine("    " + Properties.Help.Parameters_FileFilters);
            Console.WriteLine("    " + Properties.Help.Parameters_RiskHotspotAssemblyFilters);
            Console.WriteLine("    " + Properties.Help.Parameters_RiskHotspotClassFilters);
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    " + Properties.Help.Parameters_Verbosity, string.Join("|", Enum.GetNames(typeof(VerbosityLevel)))));
            Console.WriteLine("    " + Properties.Help.Parameters_Title);
            Console.WriteLine("    " + Properties.Help.Parameters_Tag);
            Console.WriteLine("    " + Properties.Help.Parameters_License);

            Console.WriteLine();
            Console.WriteLine(Properties.Help.Explanations);
            Console.WriteLine("    " + Properties.Help.Explanations_Reports);
            Console.WriteLine("    " + Properties.Help.Explanations_TargetDirectory);
            Console.WriteLine("    " + Properties.Help.Explanations_ReportTypes);
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    " + Properties.Help.ReportTypeValues, string.Join(", ", availableReportTypes)));
            Console.WriteLine("    " + Properties.Help.Explanations_SourceDirectories);
            Console.WriteLine("    " + Properties.Help.Explanations_SourceDirectories2);
            Console.WriteLine("    " + Properties.Help.Explanations_HistoryDirectory);
            Console.WriteLine("    " + Properties.Help.Explanations7);
            Console.WriteLine("    " + Properties.Help.Explanations_Plugins);
            Console.WriteLine("    " + Properties.Help.Explanations_AssemblyFilters);
            Console.WriteLine("    " + Properties.Help.Explanations_ClassFilters);
            Console.WriteLine("    " + Properties.Help.Explanations_FileFilters);
            Console.WriteLine("    " + Properties.Help.Explanations_RiskHotspotAssemblyFilters);
            Console.WriteLine("    " + Properties.Help.Explanations_RiskHotspotClassFilters);
            Console.WriteLine("    " + Properties.Help.Explanations_FiltersCommon);
            Console.WriteLine("    " + Properties.Help.Explanations_Verbosity);
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    " + Properties.Help.VerbosityValues, string.Join(", ", Enum.GetNames(typeof(VerbosityLevel)))));
            Console.WriteLine("    " + Properties.Help.Explanations_Title);
            Console.WriteLine("    " + Properties.Help.Explanations_Tag);
            Console.WriteLine("    " + Properties.Help.Explanations_License);

            Console.WriteLine();
            Console.WriteLine(Properties.Help.DefaultValues);
            Console.WriteLine("   -reporttypes:Html");
            Console.WriteLine("   -assemblyfilters:+*");
            Console.WriteLine("   -classfilters:+*");
            Console.WriteLine("   -filefilters:+*");
            Console.WriteLine("   -riskhotspotassemblyfilters:+*");
            Console.WriteLine("   -riskhotspotclassfilters:+*");
            Console.WriteLine("   -verbosity:" + VerbosityLevel.Info);

            Console.WriteLine();
            Console.WriteLine(Properties.Help.Examples);
            Console.WriteLine("   \"-reports:coverage.xml\" \"-targetdir:C:\\report\"");
            Console.WriteLine("   \"-reports:target\\*\\*.xml\" \"-targetdir:C:\\report\" -reporttypes:Latex;HtmlSummary -title:IntegrationTest -tag:v1.4.5");
            Console.WriteLine("   \"-reports:coverage1.xml;coverage2.xml\" \"-targetdir:report\" \"-sourcedirs:C:\\MyProject\" -plugins:CustomReports.dll");
            Console.WriteLine("   \"-reports:coverage.xml\" \"-targetdir:C:\\report\" \"-assemblyfilters:+Included;-Excluded.*\"");
        }
    }
}