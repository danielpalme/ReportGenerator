using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Palmmedia.ReportGenerator.Logging;
using Palmmedia.ReportGenerator.Properties;
using Palmmedia.ReportGenerator.Reporting;

namespace Palmmedia.ReportGenerator
{
    /// <summary>
    /// Builder for <see cref="ReportConfiguration"/>.
    /// Creates instances of <see cref="ReportConfiguration"/> based on command line parameters.
    /// </summary>
    internal class ReportConfigurationBuilder
    {
        /// <summary>
        /// The report builder factory.
        /// </summary>
        private readonly IReportBuilderFactory reportBuilderFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportConfigurationBuilder"/> class.
        /// </summary>
        /// <param name="reportBuilderFactory">The report builder factory.</param>
        internal ReportConfigurationBuilder(
            IReportBuilderFactory reportBuilderFactory)
        {
            if (reportBuilderFactory == null)
            {
                throw new ArgumentNullException(nameof(reportBuilderFactory));
            }

            this.reportBuilderFactory = reportBuilderFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportConfiguration"/> class.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The report configuration.</returns>
        internal ReportConfiguration Create(string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var namedArguments = new Dictionary<string, string>();

            foreach (var arg in args)
            {
                var match = Regex.Match(arg, "-(?<key>\\w{2,}):(?<value>.+)");

                if (match.Success)
                {
                    namedArguments[match.Groups["key"].Value.ToUpperInvariant()] = match.Groups["value"].Value;
                }
            }

            var reportFilePatterns = new string[] { };
            string targetDirectory = string.Empty;
            string historyDirectory = null;
            var reportTypes = new string[] { };
            var sourceDirectories = new string[] { };
            var assemblyFilters = new string[] { };
            var classFilters = new string[] { };
            var fileFilters = new string[] { };
            string verbosityLevel = null;
            string tag = null;

            string value = null;

            if (namedArguments.TryGetValue("REPORTS", out value))
            {
                reportFilePatterns = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("TARGETDIR", out value))
            {
                targetDirectory = value;
            }

            if (namedArguments.TryGetValue("HISTORYDIR", out value))
            {
                historyDirectory = value;
            }

            if (namedArguments.TryGetValue("REPORTTYPES", out value))
            {
                reportTypes = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (namedArguments.TryGetValue("REPORTTYPE", out value))
            {
                reportTypes = new[] { value };
            }

            if (namedArguments.TryGetValue("SOURCEDIRS", out value))
            {
                sourceDirectories = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("ASSEMBLYFILTERS", out value))
            {
                assemblyFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (namedArguments.TryGetValue("FILTERS", out value))
            {
                assemblyFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("CLASSFILTERS", out value))
            {
                classFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("FILEFILTERS", out value))
            {
                fileFilters = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }

            if (namedArguments.TryGetValue("VERBOSITY", out value))
            {
                verbosityLevel = value;
            }

            if (namedArguments.TryGetValue("TAG", out value))
            {
                tag = value;
            }

            return new ReportConfiguration(
                this.reportBuilderFactory,
                reportFilePatterns,
                targetDirectory,
                historyDirectory,
                reportTypes,
                sourceDirectories,
                assemblyFilters,
                classFilters,
                fileFilters,
                verbosityLevel,
                tag);
        }

        /// <summary>
        /// Shows the help of the program.
        /// </summary>
        internal void ShowHelp()
        {
            var availableReportTypes = this.reportBuilderFactory.GetAvailableReportTypes();

            Console.WriteLine();
            Console.WriteLine(typeof(ReportConfigurationBuilder).Assembly.GetName().Name + " "
                + typeof(ReportConfigurationBuilder).Assembly.GetName().Version);

            AssemblyCopyrightAttribute assemblyCopyrightAttribute = typeof(ReportConfigurationBuilder).Assembly
                .GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)
                .Cast<AssemblyCopyrightAttribute>()
                .FirstOrDefault();

            if (assemblyCopyrightAttribute != null)
            {
                Console.WriteLine(assemblyCopyrightAttribute.Copyright);
            }

            Console.WriteLine();
            Console.WriteLine(Help.Parameters);
            Console.WriteLine("    " + Help.Parameters1);
            Console.WriteLine("    " + Help.Parameters2);
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    " + Help.Parameters3, string.Join("|", availableReportTypes.Take(3).Union(new[] { "..." }))));
            Console.WriteLine("    " + Help.Parameters4);
            Console.WriteLine("    " + Help.Parameters5);
            Console.WriteLine("    " + Help.Parameters6);
            Console.WriteLine("    " + Help.Parameters7);
            Console.WriteLine("    " + Help.Parameters8);
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    " + Help.Parameters9, string.Join("|", Enum.GetNames(typeof(VerbosityLevel)))));
            Console.WriteLine("    " + Help.Parameters10);

            Console.WriteLine();
            Console.WriteLine(Help.Explanations);
            Console.WriteLine("    " + Help.Explanations1);
            Console.WriteLine("    " + Help.Explanations2);
            Console.WriteLine("    " + Help.Explanations3);
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    " + Help.ReportTypeValues, string.Join(", ", availableReportTypes)));
            Console.WriteLine("    " + Help.Explanations4);
            Console.WriteLine("    " + Help.Explanations5);
            Console.WriteLine("    " + Help.Explanations6);
            Console.WriteLine("    " + Help.Explanations7);
            Console.WriteLine("    " + Help.Explanations8);
            Console.WriteLine("    " + Help.Explanations9);
            Console.WriteLine("    " + Help.Explanations10);
            Console.WriteLine("    " + Help.Explanations11);
            Console.WriteLine("    " + Help.Explanations12);
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "    " + Help.VerbosityValues, string.Join(", ", Enum.GetNames(typeof(VerbosityLevel)))));
            Console.WriteLine("    " + Help.Explanations13);

            Console.WriteLine();
            Console.WriteLine(Help.DefaultValues);
            Console.WriteLine("   -reporttypes:Html");
            Console.WriteLine("   -assemblyfilters:+*");
            Console.WriteLine("   -classfilters:+*");
            Console.WriteLine("   -filefilters:+*");
            Console.WriteLine("   -verbosity:" + VerbosityLevel.Verbose);

            Console.WriteLine();
            Console.WriteLine(Help.Examples);
            Console.WriteLine("   \"-reports:coverage.xml\" \"-targetdir:C:\\report\"");
            Console.WriteLine("   \"-reports:target\\*\\*.xml\" \"-targetdir:C:\\report\" -reporttypes:Latex;HtmlSummary -tag:v1.4.5");
            Console.WriteLine("   \"-reports:coverage1.xml;coverage2.xml\" \"-targetdir:report\"");
            Console.WriteLine("   \"-reports:coverage.xml\" \"-targetdir:C:\\report\" -reporttypes:Latex \"-sourcedirs:C:\\MyProject\"");
            Console.WriteLine("   \"-reports:coverage.xml\" \"-targetdir:C:\\report\" \"-sourcedirs:C:\\MyProject1;C:\\MyProject2\" \"-assemblyfilters:+Included;-Excluded.*\"");
        }
    }
}