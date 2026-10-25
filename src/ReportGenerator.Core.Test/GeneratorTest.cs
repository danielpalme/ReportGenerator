using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Parser.FileReading;
using Palmmedia.ReportGenerator.Core.Reporting;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test
{
    public class GeneratorTest : IDisposable
    {
        private const string SourceMarker = "CUSTOM_FILE_READER_CONTENT";

        private readonly string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        public GeneratorTest()
        {
            Directory.CreateDirectory(this.directory);
        }

        public void Dispose()
        {
            Directory.Delete(this.directory, true);
        }

        [Fact]
        public void GenerateReport_CustomFileReader_SourceReadByFileReader()
        {
            // A source file that does not exist on disk
            string sourcePath = Path.Combine(this.directory, "src", "Source.cs");
            var fileReader = new DictionaryFileReader(new Dictionary<string, string[]>()
            {
                { sourcePath, new[] { "public class Source", "{", $"    // {SourceMarker}", "}" } }
            });

            var (reportConfiguration, settings, parserResult) = this.ParseCoberturaReport(sourcePath);

            new Generator().GenerateReport(
                reportConfiguration,
                settings,
                new MinimumCoverageThresholds(),
                new RiskHotspotsAnalysisThresholds(),
                parserResult,
                fileReader);

            Assert.Contains(sourcePath, fileReader.RequestedPaths);
            Assert.Contains(
                Directory.EnumerateFiles(reportConfiguration.TargetDirectory, "*.html"),
                file => File.ReadAllText(file).Contains(SourceMarker));
        }

        [Fact]
        public void GenerateReport_CustomFileReaderError_ErrorShownInReport()
        {
            string sourcePath = Path.Combine(this.directory, "src", "Source.cs");
            var fileReader = new DictionaryFileReader(new Dictionary<string, string[]>());

            var (reportConfiguration, settings, parserResult) = this.ParseCoberturaReport(sourcePath);

            new Generator().GenerateReport(
                reportConfiguration,
                settings,
                new MinimumCoverageThresholds(),
                new RiskHotspotsAnalysisThresholds(),
                parserResult,
                fileReader);

            Assert.Contains(sourcePath, fileReader.RequestedPaths);
            Assert.Contains(
                Directory.EnumerateFiles(reportConfiguration.TargetDirectory, "*.html"),
                file => File.ReadAllText(file).Contains(DictionaryFileReader.Error));
        }

        [Fact]
        public void GenerateReport_DefaultFileReader_SourceReadFromDisk()
        {
            string sourcePath = Path.Combine(this.directory, "src", "Source.cs");
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath));
            File.WriteAllLines(sourcePath, new[] { "public class Source", "{", $"    // {SourceMarker}", "}" });

            var (reportConfiguration, settings, parserResult) = this.ParseCoberturaReport(sourcePath);

            new Generator().GenerateReport(
                reportConfiguration,
                settings,
                new MinimumCoverageThresholds(),
                new RiskHotspotsAnalysisThresholds(),
                parserResult);

            Assert.Contains(
                Directory.EnumerateFiles(reportConfiguration.TargetDirectory, "*.html"),
                file => File.ReadAllText(file).Contains(SourceMarker));
        }

        [Fact]
        public void GenerateReport_NullFileReader_ArgumentNullException()
        {
            string sourcePath = Path.Combine(this.directory, "src", "Source.cs");
            var (reportConfiguration, settings, parserResult) = this.ParseCoberturaReport(sourcePath);

            var exception = Assert.Throws<ArgumentNullException>(() => new Generator().GenerateReport(
                reportConfiguration,
                settings,
                new MinimumCoverageThresholds(),
                new RiskHotspotsAnalysisThresholds(),
                parserResult,
                null));
            Assert.Equal("fileReader", exception.ParamName);
        }

        /// <summary>
        /// Parses a Cobertura report covering the given source file, for a HTML report in the test directory.
        /// </summary>
        private (IReportConfiguration ReportConfiguration, Settings Settings, ParserResult ParserResult) ParseCoberturaReport(string sourcePath)
        {
            string reportPath = Path.Combine(this.directory, "coverage.cobertura.xml");
            File.WriteAllText(reportPath, $@"<?xml version=""1.0""?>
<coverage line-rate=""1"" branch-rate=""0"" version=""1"">
  <packages>
    <package name=""Sample"" line-rate=""1"" branch-rate=""0"">
      <classes>
        <class name=""Source"" filename=""{sourcePath}"" line-rate=""1"" branch-rate=""0"">
          <methods />
          <lines><line number=""1"" hits=""1"" /></lines>
        </class>
      </classes>
    </package>
  </packages>
</coverage>");

            // The overloads taking a parser result do not validate the configuration, which creates the target directory
            Directory.CreateDirectory(Path.Combine(this.directory, "report"));

            var reportConfiguration = new ReportConfigurationBuilder().Create(new Dictionary<string, string>()
            {
                { "reports", reportPath },
                { "targetdir", Path.Combine(this.directory, "report") },
                { "reporttypes", "Html" }
            });
            var settings = new Settings();

            var parserResult = new CoverageReportParser(new ReportContext(reportConfiguration, settings))
                .ParseFiles(reportConfiguration.ReportFiles);

            return (reportConfiguration, settings, parserResult);
        }
        
        private class DictionaryFileReader : IFileReader
        {
            public const string Error = "CUSTOM_FILE_READER_ERROR";

            private readonly IReadOnlyDictionary<string, string[]> files;

            public DictionaryFileReader(IReadOnlyDictionary<string, string[]> files)
            {
                this.files = files;
            }

            public List<string> RequestedPaths { get; } = new List<string>();

            public string[] LoadFile(string path, out string error)
            {
                lock (this.RequestedPaths)
                {
                    this.RequestedPaths.Add(path);
                }

                if (this.files.TryGetValue(path, out string[] lines))
                {
                    error = null;
                    return lines;
                }

                error = Error;
                return null;
            }
        }
    }
}
