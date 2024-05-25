using System.IO;
using NSubstitute;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Reporting;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test
{
    [Collection("FileManager")]
    public class ReportConfigurationValidatorTest
    {
        private static readonly string ReportPath = Path.Combine(FileManager.GetCSharpReportDirectory(), "OpenCover.xml");

        private readonly IReportBuilderFactory reportBuilderFactory = Substitute.For<IReportBuilderFactory>();

        public ReportConfigurationValidatorTest()
        {
            this.reportBuilderFactory.GetAvailableReportTypes()
                .Returns(new[] { "Latex", "Xml", "Html", "Something" });
        }

        [Fact]
        public void Validate_AllPropertiesApplied_ValidationPasses()
        {
            var configuration = new ReportConfiguration(
                new[] { ReportPath },
                "C:\\temp",
                System.Array.Empty<string>(),
                null,
                new[] { "Latex", "Xml", "Html" },
                System.Array.Empty<string>(),
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                new[] { "+Test3", "-Test3" },
                VerbosityLevel.Info.ToString(),
                null);

            var sut = new ReportConfigurationValidator(this.reportBuilderFactory);

            Assert.True(sut.Validate(configuration));
        }

        [Fact]
        public void Validate_NoReport_ValidationFails()
        {
            var configuration = new ReportConfiguration(
                System.Array.Empty<string>(),
                "C:\\temp",
                System.Array.Empty<string>(),
                null,
                new[] { "Latex" },
                System.Array.Empty<string>(),
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                System.Array.Empty<string>(),
                VerbosityLevel.Info.ToString(),
                null);

            var sut = new ReportConfigurationValidator(this.reportBuilderFactory);

            Assert.False(sut.Validate(configuration));
        }

        [Fact]
        public void Validate_NonExistingReport_ValidationFails()
        {
            var configuration = new ReportConfiguration(
                new[] { "123.xml" },
                "C:\\temp",
                System.Array.Empty<string>(),
                null,
                new[] { "Latex" },
                System.Array.Empty<string>(),
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                System.Array.Empty<string>(),
                VerbosityLevel.Info.ToString(),
                null);

            var sut = new ReportConfigurationValidator(this.reportBuilderFactory);

            Assert.False(sut.Validate(configuration));
        }

        [Fact]
        public void Validate_NoTargetDirectory_ValidationFails()
        {
            var configuration = new ReportConfiguration(
                new[] { ReportPath },
                string.Empty,
                System.Array.Empty<string>(),
                null,
                new[] { "Latex" },
                System.Array.Empty<string>(),
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                System.Array.Empty<string>(),
                VerbosityLevel.Info.ToString(),
                null);

            var sut = new ReportConfigurationValidator(this.reportBuilderFactory);

            Assert.False(sut.Validate(configuration));
        }

        [Fact]
        public void Validate_InvalidTargetDirectory_ValidationFails()
        {
            var configuration = new ReportConfiguration(
                new[] { ReportPath },
                "C:\\temp:?$",
                System.Array.Empty<string>(),
                null,
                new[] { "Latex" },
                System.Array.Empty<string>(),
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                System.Array.Empty<string>(),
                VerbosityLevel.Info.ToString(),
                null);

            var sut = new ReportConfigurationValidator(this.reportBuilderFactory);

            Assert.False(sut.Validate(configuration));
        }

        [Fact]
        public void Validate_InvalidSourceDirectory_ValidationFails()
        {
            var configuration = new ReportConfiguration(
                new[] { ReportPath },
                @"C:\\temp",
                new string[] { "C:\\doesnotexist" },
                null,
                new[] { "Latex" },
                new string[] { "notexistingplugin.dll" },
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                System.Array.Empty<string>(),
                VerbosityLevel.Info.ToString(),
                null);

            var sut = new ReportConfigurationValidator(this.reportBuilderFactory);

            Assert.False(sut.Validate(configuration));
        }

        [Fact]
        public void Validate_InvalidHistoryDirectory_ValidationFails()
        {
            var configuration = new ReportConfiguration(
                new[] { ReportPath },
                "C:\\temp",
                System.Array.Empty<string>(),
                "C:\\temp:?$",
                new[] { "Latex" },
                System.Array.Empty<string>(),
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                System.Array.Empty<string>(),
                VerbosityLevel.Info.ToString(),
                null);

            var sut = new ReportConfigurationValidator(this.reportBuilderFactory);

            Assert.False(sut.Validate(configuration));
        }

        [Fact]
        public void Validate_InvalidReportType_ValidationFails()
        {
            var configuration = new ReportConfiguration(
                new[] { ReportPath },
                "C:\\temp",
                System.Array.Empty<string>(),
                null,
                new[] { "DoesNotExist" },
                System.Array.Empty<string>(),
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                System.Array.Empty<string>(),
                VerbosityLevel.Info.ToString(),
                null);

            var sut = new ReportConfigurationValidator(this.reportBuilderFactory);

            Assert.False(sut.Validate(configuration));
        }

        [Fact]
        public void Validate_InvalidFilter_ValidationFails()
        {
            var configuration = new ReportConfiguration(
                new[] { ReportPath },
                @"C:\\temp",
                System.Array.Empty<string>(),
                null,
                new[] { "Latex" },
                System.Array.Empty<string>(),
                new[] { "Test" },
                new[] { "Test2" },
                new[] { "Test3" },
                VerbosityLevel.Info.ToString(),
                null);

            var sut = new ReportConfigurationValidator(this.reportBuilderFactory);

            Assert.False(sut.Validate(configuration));
        }

        [Fact]
        public void Validate_InvalidRiskHotspotFilter_ValidationFails()
        {
            var configuration = new ReportConfiguration(
                new[] { ReportPath },
                "C:\\temp",
                System.Array.Empty<string>(),
                null,
                new[] { "Latex", "Xml", "Html" },
                System.Array.Empty<string>(),
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                new[] { "+Test3", "-Test3" },
                new[] { "Test" },
                new[] { "Test2" },
                VerbosityLevel.Info.ToString(),
                null,
                null,
                null);

            var sut = new ReportConfigurationValidator(this.reportBuilderFactory);

            Assert.False(sut.Validate(configuration));
        }

        [Fact]
        public void Validate_InvalidPlugin_ValidationFails()
        {
            var configuration = new ReportConfiguration(
                new[] { ReportPath },
                @"C:\\temp",
                System.Array.Empty<string>(),
                null,
                new[] { "Latex" },
                new string[] { "notexistingplugin.dll" },
                new[] { "+Test", "-Test" },
                new[] { "+Test2", "-Test2" },
                System.Array.Empty<string>(),
                VerbosityLevel.Info.ToString(),
                null);

            var sut = new ReportConfigurationValidator(this.reportBuilderFactory);

            Assert.False(sut.Validate(configuration));
        }
    }
}