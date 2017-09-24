using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Logging;
using Palmmedia.ReportGenerator.Parser.Analysis;
using Palmmedia.ReportGenerator.Reporting;

namespace Palmmedia.ReportGeneratorTest.Reporting
{
    /// <summary>
    /// This is a test class for MefReportBuilderFactory and is intended
    /// to contain all MefReportBuilderFactory Unit Tests
    /// </summary>
    [TestClass]
    public class MefReportBuilderFactoryTest
    {
        [TestMethod]
        public void GetAvailableReportTypes_AllReportTypesReturned()
        {
            var factory = new MefReportBuilderFactory();

            Assert.IsTrue(factory.GetAvailableReportTypes().Count() > 6, "Not all default report builders available.");
        }

        [TestMethod]
        public void GetReportBuilders_DefaultReportBuilderReturned()
        {
            var factory = new MefReportBuilderFactory();

            var reportBuilders = factory.GetReportBuilders(new ReportConfiguration() { TargetDirectory = "C:\\temp", ReportTypes = new[] { "Html" } });
            Assert.AreEqual(1, reportBuilders.Count(), "Default report builder not available.");

            reportBuilders = factory.GetReportBuilders(new ReportConfiguration() { TargetDirectory = "C:\\temp", ReportTypes = new[] { "Latex" } });
            Assert.AreEqual(1, reportBuilders.Count(), "Report builder not available.");
            Assert.AreEqual(typeof(AdditionalLatexReportBuilder), reportBuilders.First().GetType(), "Non default report builder should get returned");
        }

        [System.ComponentModel.Composition.Export(typeof(IReportBuilder))]
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
            /// Gets or sets the report configuration.
            /// </summary>
            /// <value>
            /// The report configuration.
            /// </value>
            public IReportConfiguration ReportConfiguration { get; set; }

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

            public IEnumerable<string> ReportTypes { get; set; }

            public IEnumerable<string> SourceDirectories { get; set; }

            public IEnumerable<string> AssemblyFilters { get; set; }

            public IEnumerable<string> ClassFilters { get; set; }

            public IEnumerable<string> FileFilters { get; set; }

            public VerbosityLevel VerbosityLevel { get; set; }

            public string Tag { get; set; }
        }
    }
}
