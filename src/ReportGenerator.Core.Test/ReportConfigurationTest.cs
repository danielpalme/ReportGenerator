using System.IO;
using Palmmedia.ReportGenerator.Core;
using Palmmedia.ReportGenerator.Core.Logging;
using Xunit;

namespace Palmmedia.ReportGeneratorTest
{
    /// <summary>
    /// This is a test class for ReportConfiguration and is intended
    /// to contain all ReportConfiguration Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class ReportConfigurationTest
    {
        private static readonly string ReportPath = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");

        [Fact]
        public void InitByConstructor_AllDefaultValuesApplied()
        {
            var configuration = new ReportConfiguration(
                new[] { ReportPath },
                "C:\\temp",
                "C:\\temp\\historic",
                new string[] { },
                new string[] { },
                new string[] { },
                new string[] { },
                new string[] { },
                null,
                null);

            Assert.Contains(ReportPath, configuration.ReportFiles);
            Assert.Equal("C:\\temp", configuration.TargetDirectory);
            Assert.Equal("C:\\temp\\historic", configuration.HistoryDirectory);
            Assert.Contains("Html", configuration.ReportTypes);
            Assert.Empty(configuration.AssemblyFilters);
            Assert.Empty(configuration.ClassFilters);
            Assert.Equal(VerbosityLevel.Verbose, configuration.VerbosityLevel);
            Assert.NotNull(configuration.ReportFiles);
            Assert.NotNull(configuration.AssemblyFilters);
            Assert.NotNull(configuration.ClassFilters);

            Assert.True(configuration.VerbosityLevelValid);
            Assert.Equal(0, configuration.InvalidReportFilePatterns.Count);
        }

        [Fact]
        public void InitByConstructor_AllPropertiesApplied()
        {
            var configuration = new ReportConfiguration(
                new[] { ReportPath },
                "C:\\temp",
                null,
                new[] { "Latex", "Xml", "Html" },
                new string[] { "ReportGenerator.Core.Test.dll" },
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                new[] { "+Test3", "-Test3" },
                VerbosityLevel.Info.ToString(),
                "CustomTag");

            Assert.Contains(ReportPath, configuration.ReportFiles);
            Assert.Equal("C:\\temp", configuration.TargetDirectory);
            Assert.Contains("Latex", configuration.ReportTypes);
            Assert.Contains("Xml", configuration.ReportTypes);
            Assert.Contains("Html", configuration.ReportTypes);
            Assert.Contains("ReportGenerator.Core.Test.dll", configuration.Plugins);
            Assert.Contains("+Test", configuration.AssemblyFilters);
            Assert.Contains("-Test", configuration.AssemblyFilters);
            Assert.Contains("+Test2", configuration.ClassFilters);
            Assert.Contains("-Test2", configuration.ClassFilters);
            Assert.Contains("+Test3", configuration.FileFilters);
            Assert.Contains("-Test3", configuration.FileFilters);
            Assert.Equal(VerbosityLevel.Info, configuration.VerbosityLevel);
            Assert.NotNull(configuration.ReportFiles);
            Assert.NotNull(configuration.AssemblyFilters);
            Assert.NotNull(configuration.ClassFilters);
            Assert.Equal("CustomTag", configuration.Tag);

            Assert.True(configuration.VerbosityLevelValid);
            Assert.Equal(0, configuration.InvalidReportFilePatterns.Count);
        }

        [Fact]
        public void InitByConstructor_InvalidValues()
        {
            var configuration = new ReportConfiguration(
                new[] { ReportPath, "\t" },
                "C:\\temp",
                "C:\\temp\\historic",
                new string[] { },
                new string[] { "notexistingplugin.dll" },
                new string[] { },
                new string[] { },
                new string[] { },
                "Invalid",
                null);

            Assert.Contains(ReportPath, configuration.ReportFiles);
            Assert.Equal("C:\\temp", configuration.TargetDirectory);
            Assert.Equal("C:\\temp\\historic", configuration.HistoryDirectory);
            Assert.Contains("Html", configuration.ReportTypes);
            Assert.Contains("notexistingplugin.dll", configuration.Plugins);
            Assert.Empty(configuration.AssemblyFilters);
            Assert.Empty(configuration.ClassFilters);
            Assert.Equal(VerbosityLevel.Verbose, configuration.VerbosityLevel);
            Assert.NotNull(configuration.ReportFiles);
            Assert.NotNull(configuration.AssemblyFilters);
            Assert.NotNull(configuration.ClassFilters);

            Assert.False(configuration.VerbosityLevelValid);
            Assert.Equal(1, configuration.InvalidReportFilePatterns.Count);
        }
    }
}