using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Logging;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test
{
    /// <summary>
    /// This is a test class for ReportConfigurationBuilder and is intended
    /// to contain all ReportConfigurationBuilder Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class ReportConfigurationBuilderTest
    {
        private static readonly string ReportPath = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");

        private ReportConfigurationBuilder reportConfigurationBuilder;

        public ReportConfigurationBuilderTest()
        {
            this.reportConfigurationBuilder = new ReportConfigurationBuilder();
        }

        [Fact]
        public void InitWithNamedArguments_OldFilters_AllPropertiesApplied()
        {
            string[] namedArguments = new string[]
            {
                "-reports:" + ReportPath,
                "-targetdir:C:\\temp",
                "-reporttype:Latex",
                "-filters:+Test;-Test",
                "-verbosity:" + VerbosityLevel.Info.ToString()
            };

            var configuration = this.reportConfigurationBuilder.Create(namedArguments);

            Assert.True(configuration.ReportFiles.Contains(ReportPath), "ReportPath does not exist in ReportFiles.");
            Assert.True(configuration.ReportTypes.Contains("Latex"), "Wrong report type applied.");
            Assert.True(configuration.AssemblyFilters.Contains("+Test"), "AssemblyFilters does not exist in ReportFiles.");
            Assert.True(configuration.AssemblyFilters.Contains("-Test"), "AssemblyFilters does not exist in ReportFiles.");
            Assert.NotNull(configuration.ReportFiles);
            Assert.NotNull(configuration.AssemblyFilters);
            Assert.NotNull(configuration.ClassFilters);
        }

        [Fact]
        public void InitWithNamedArguments_NewFilters_AllPropertiesApplied()
        {
            string[] namedArguments = new string[]
            {
                "-reports:" + ReportPath,
                "-targetdir:C:\\temp",
                "-reporttype:Latex",
                "-assemblyfilters:+Test;-Test",
                "-classfilters:+Test2;-Test2",
                "-verbosity:" + VerbosityLevel.Info.ToString()
            };

            var configuration = this.reportConfigurationBuilder.Create(namedArguments);

            Assert.True(configuration.ReportFiles.Contains(ReportPath), "ReportPath does not exist in ReportFiles.");
            Assert.Equal("C:\\temp", configuration.TargetDirectory);
            Assert.True(configuration.ReportTypes.Contains("Latex"), "Wrong report type applied.");
            Assert.True(configuration.AssemblyFilters.Contains("+Test"), "AssemblyFilters does not exist in ReportFiles.");
            Assert.True(configuration.ClassFilters.Contains("+Test2"), "ClassFilters does not exist in ReportFiles.");
            Assert.True(configuration.AssemblyFilters.Contains("-Test"), "AssemblyFilters does not exist in ReportFiles.");
            Assert.True(configuration.ClassFilters.Contains("-Test2"), "ClassFilters does not exist in ReportFiles.");
            Assert.NotNull(configuration.ReportFiles);
            Assert.NotNull(configuration.AssemblyFilters);
            Assert.NotNull(configuration.ClassFilters);
        }
    }
}
