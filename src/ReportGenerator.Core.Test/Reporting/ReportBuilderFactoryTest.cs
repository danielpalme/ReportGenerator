using System.Collections.Generic;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Plugin;
using Palmmedia.ReportGenerator.Core.Reporting;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Reporting
{
    public class ReportBuilderFactoryTest
    {
        [Fact]
        public void GetAvailableReportTypes_AllReportTypesReturned()
        {
            var plugins = new List<string>()
            {
                typeof(ReportBuilderFactoryTest).Assembly.Location
            };

            var factory = new ReportBuilderFactory(new ReflectionPluginLoader(plugins));
            Assert.True(factory.GetAvailableReportTypes().Count() > 12, "Not all default report builders available.");
        }

        [Fact]
        public void GetReportBuilders_DefaultReportBuilderReturned()
        {
            var plugins = new List<string>()
            {
                typeof(ReportBuilderFactoryTest).Assembly.Location
            };

            var factory = new ReportBuilderFactory(new ReflectionPluginLoader(plugins));

            var reportContext = new ReportContext(new ReportConfiguration() { TargetDirectory = "C:\\temp", ReportTypes = new[] { "Html" } }, new Settings());
            var reportBuilders = factory.GetReportBuilders(reportContext);
            Assert.Single(reportBuilders);

            reportContext = new ReportContext(new ReportConfiguration() { TargetDirectory = "C:\\temp", ReportTypes = new[] { "Latex" } }, new Settings());
            reportBuilders = factory.GetReportBuilders(reportContext);
            Assert.Single(reportBuilders);
            Assert.Equal(typeof(AdditionalLatexReportBuilder).FullName, reportBuilders.First().GetType().FullName);
        }

        public class AdditionalLatexReportBuilder : IReportBuilder
        {
            /// <summary>
            /// Gets the type of the report.
            /// </summary>
            /// <value>
            /// The type of the report.
            /// </value>
            public string ReportType => "Latex";

            /// <summary>
            /// Gets or sets the report context.
            /// </summary>
            /// <value>
            /// The report context.
            /// </value>
            public IReportContext ReportContext { get; set; }

            /// <summary>
            /// Creates a class report.
            /// </summary>
            /// <param name="class">The class.</param>
            /// <param name="fileAnalyses">The file analyses that correspond to the class.</param>
            public void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses)
            {
            }

            /// <summary>
            /// Creates the summary report.
            /// </summary>
            /// <param name="summaryResult">The summary result.</param>
            public void CreateSummaryReport(SummaryResult summaryResult)
            {
            }
        }

        private class ReportConfiguration : IReportConfiguration
        {
            public string TargetDirectory { get; set; }

            public string HistoryDirectory { get; set; }

            public IReadOnlyCollection<string> ReportFiles { get; set; }

            public IReadOnlyCollection<string> InvalidReportFilePatterns { get; set; }

            public IReadOnlyCollection<string> ReportTypes { get; set; }

            public IReadOnlyCollection<string> Plugins { get; set; }

            public IReadOnlyCollection<string> SourceDirectories { get; set; }

            public IReadOnlyCollection<string> AssemblyFilters { get; set; }

            public IReadOnlyCollection<string> ClassFilters { get; set; }

            public IReadOnlyCollection<string> FileFilters { get; set; }

            public IReadOnlyCollection<string> RiskHotspotAssemblyFilters { get; set; }

            public IReadOnlyCollection<string> RiskHotspotClassFilters { get; set; }

            public VerbosityLevel VerbosityLevel { get; set; }

            public IReadOnlyCollection<HistoricCoverage> OverallHistoricCoverages { get; set; }

            public string Title { get; set; }

            public string Tag { get; set; }

            public string License { get; set; }

            public bool VerbosityLevelValid { get; set; }
        }
    }
}
