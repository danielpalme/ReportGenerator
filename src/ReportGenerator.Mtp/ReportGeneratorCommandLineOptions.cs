using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.CommandLine;

namespace Palmmedia.ReportGenerator.Mtp
{
    internal class ReportGeneratorCommandLineOptions : ICommandLineOptionsProvider
    {
        private readonly IExtension extension;

        public ReportGeneratorCommandLineOptions(IExtension extension)
        {
            this.extension = extension;
        }

        public string Uid => this.extension.Uid;

        public string Version => this.extension.Version;

        public string DisplayName => this.extension.DisplayName;

        public string Description => this.extension.Description;

        public Task<bool> IsEnabledAsync() => Task.FromResult(true);

        public IReadOnlyCollection<CommandLineOption> GetCommandLineOptions() => new[]
        {
            new CommandLineOption(
                "reportgenerator",
                "Enable code coverage report generation.",
                ArgumentArity.Zero,
                isHidden:
                false),
            new CommandLineOption(
                CommandLineArgumentNames.Reports,
                "The coverage reports that should be parsed (separated by semicolon). Globbing is supported. Default: '**/TestResults/coverage.cobertura*.xml'",
                ArgumentArity.ZeroOrOne,
                false),
            new CommandLineOption(
                CommandLineArgumentNames.TargetDirectory,
                "The directory where the generated report should be saved. Default: 'coveragereport'",
                ArgumentArity.ZeroOrOne,
                false),
            new CommandLineOption(
                CommandLineArgumentNames.ReportTypes,
                "The output formats and scope (separated by semicolon) Values: Badges, Clover, Cobertura, CsvSummary, Html, HtmlChart, HtmlInline, HtmlInline_AzurePipelines, HtmlInline_AzurePipelines_Dark, HtmlSummary, Html_BlueRed_Summary, JsonSummary, Latex, LatexSummary, lcov, MHtml, SvgChart, SonarQube, TeamCitySummary, TextSummary, Xml, XmlSummary. Default: 'Html'",
                ArgumentArity.ZeroOrOne,
                false),
            new CommandLineOption(
                CommandLineArgumentNames.SourceDirectories,
                "Optional directories which contain the corresponding source code (separated by semicolon). The source directories are used if coverage report contains classes without path information. Default: ''",
                ArgumentArity.ZeroOrOne,
                false),
            new CommandLineOption(
                CommandLineArgumentNames.HistoryDirectory,
                "Optional directory for storing persistent coverage information. Can be used in future reports to show coverage evolution. Default: ''",
                ArgumentArity.ZeroOrOne,
                false),
            new CommandLineOption(
                CommandLineArgumentNames.Plugins,
                "Optional plugin files for custom reports or custom history storage (separated by semicolon). Default: ''",
                ArgumentArity.ZeroOrOne,
                false),
            new CommandLineOption(
                CommandLineArgumentNames.AssemblyFilters,
                "Optional list of assemblies that should be included or excluded in the report. Exclusion filters take precedence over inclusion filters. Wildcards are allowed. Default: '+*'",
                ArgumentArity.ZeroOrOne,
                false),
            new CommandLineOption(
                CommandLineArgumentNames.ClassFilters,
                "Optional list of classes that should be included or excluded in the report. Exclusion filters take precedence over inclusion filters. Wildcards are allowed. Default: '+*'",
                ArgumentArity.ZeroOrOne,
                false),
            new CommandLineOption(
                CommandLineArgumentNames.FileFilters,
                "Optional list of files that should be included or excluded in the report. Exclusion filters take precedence over inclusion filters. Wildcards are allowed. Default: '+*'",
                ArgumentArity.ZeroOrOne,
                false),
            new CommandLineOption(
                CommandLineArgumentNames.RiskHotspotAssemblyFilters,
                "Optional list of assemblies that should be included or excluded in the risk hotspots. Exclusion filters take precedence over inclusion filters. Wildcards are allowed. Default: '+*'",
                ArgumentArity.ZeroOrOne,
                false),
            new CommandLineOption(
                CommandLineArgumentNames.RiskHotspotClassFilters,
                "Optional list of classes that should be included or excluded in the risk hotspots. Exclusion filters take precedence over inclusion filters. Wildcards are allowed. Default: '+*'",
                ArgumentArity.ZeroOrOne,
                false),
            new CommandLineOption(
                CommandLineArgumentNames.Verbosity,
                "The verbosity level of the log messages. Values: Verbose, Info, Warning, Error, Off. Default: 'Info'",
                ArgumentArity.ZeroOrOne,
                false),
            new CommandLineOption(
                CommandLineArgumentNames.Title,
                "Optional title. Default: ''",
                ArgumentArity.ZeroOrOne,
                false),
            new CommandLineOption(
                CommandLineArgumentNames.Tag,
                "Optional tag or build version. Default: ''",
                ArgumentArity.ZeroOrOne,
                false),
            new CommandLineOption(
                CommandLineArgumentNames.License,
                "Optional license for PRO version. Get your license here: https://reportgenerator.io/pro. Default: ''",
                ArgumentArity.ZeroOrOne,
                false),
            // TODO: Add support for custom settings. Currently not working
            //new CommandLineOption(
            //    CommandLineArgumentNames.CustomSettings,
            //    "Optional custom settings (separated by semicolon). See: https://github.com/danielpalme/ReportGenerator/wiki/Settings. Default: ''",
            //    ArgumentArity.ZeroOrMore,
            //    false),
        };

        public Task<ValidationResult> ValidateCommandLineOptionsAsync(ICommandLineOptions commandLineOptions)
        {
            //if (commandLineOptions..Name == DopOption)
            //{
            //    if (!int.TryParse(arguments[0], out int dopValue) || dopValue <= 0)
            //    {
            //        return ValidationResult.InvalidTask("Dop must be a positive integer");
            //    }
            //}

            return ValidationResult.ValidTask;
        }

        public Task<ValidationResult> ValidateOptionArgumentsAsync(CommandLineOption commandOption, string[] arguments)
        {
            //bool generateReportEnabled = commandLineOptions.IsOptionSet(GenerateReportOption);
            //bool reportFileName = commandLineOptions.TryGetOptionArgumentList(ReportFilenameOption, out string[]? _);

            //return (generateReportEnabled || reportFileName) && !(generateReportEnabled && reportFileName)
            //    ? ValidationResult.InvalidTask("--generatereport and --reportfilename must be specified together")
            //    : ValidationResult.ValidTask;

            return ValidationResult.ValidTask;
        }
    }
}