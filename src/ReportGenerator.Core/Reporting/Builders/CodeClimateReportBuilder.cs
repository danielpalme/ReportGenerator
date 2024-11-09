using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{
    /// <summary>
    /// Creates Gitlab Code Climate report in JSON format.
    /// </summary>
    public class CodeClimateReportBuilder : IReportBuilder
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(CodeClimateReportBuilder));

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report format.
        /// </value>
        public virtual string ReportType => "CodeClimate";

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
        public virtual void CreateClassReport(Class @class, IEnumerable<FileAnalysis> fileAnalyses)
        {
        }

        /// <summary>
        /// Creates the summary report.
        /// </summary>
        /// <param name="summaryResult">The summary result.</param>
        public void CreateSummaryReport(SummaryResult summaryResult)
        {
            string targetDirectory = this.ReportContext.ReportConfiguration.TargetDirectory;

            if (this.ReportContext.Settings.CreateSubdirectoryForAllReportTypes)
            {
                targetDirectory = Path.Combine(targetDirectory, this.ReportType);

                if (!Directory.Exists(targetDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorFormat(Resources.TargetDirectoryCouldNotBeCreated, targetDirectory, ex.GetExceptionMessageForDisplay());
                        return;
                    }
                }
            }

            string targetPath = Path.Combine(targetDirectory, "codeclimate.json");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            var gitInformation = GitHelper.GetGitInformation();
            int missedLines = summaryResult.CoverableLines - summaryResult.CoveredLines;
            var processedFiles = new HashSet<string>();

            using (var reportTextWriter = new StreamWriter(new FileStream(targetPath, FileMode.Create), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                reportTextWriter.WriteLine("{");

                reportTextWriter.WriteLine("  \"ci_service\": {");
                reportTextWriter.WriteLine($"    \"branch\": {JsonSerializer.Serialize(gitInformation.Branch)},");
                reportTextWriter.WriteLine("    \"build_identifier\": \"\",");
                reportTextWriter.WriteLine("    \"build_url\": \"\",");
                reportTextWriter.WriteLine($"    \"commit_sha\": \"{gitInformation.Sha}\",");
                reportTextWriter.WriteLine($"    \"committed_at\": {(string.IsNullOrWhiteSpace(gitInformation.TimeStamp) ? "null" : gitInformation.TimeStamp)},");
                reportTextWriter.WriteLine("    \"name\": \"\"");
                reportTextWriter.WriteLine("  },");

                reportTextWriter.WriteLine("  \"environment\": {");
                reportTextWriter.WriteLine("    \"gem_version\": \"\",");
                reportTextWriter.WriteLine("    \"package_version\": \"\",");
                reportTextWriter.WriteLine($"    \"pwd\": {JsonSerializer.Serialize(Directory.GetCurrentDirectory())},");
                reportTextWriter.WriteLine($"    \"prefix\": {JsonSerializer.Serialize(Directory.GetCurrentDirectory())},");
                reportTextWriter.WriteLine("    \"rails_root\": \"\",");
                reportTextWriter.WriteLine("    \"reporter_version\": \"0.11.1\",");
                reportTextWriter.WriteLine("    \"simplecov_root\": \"\"");
                reportTextWriter.WriteLine("  },");

                reportTextWriter.WriteLine("  \"git\": {");
                reportTextWriter.WriteLine($"    \"branch\": {JsonSerializer.Serialize(gitInformation.Branch)},");
                reportTextWriter.WriteLine($"    \"head\": \"{gitInformation.Sha}\",");
                reportTextWriter.WriteLine($"    \"committed_at\": {(string.IsNullOrWhiteSpace(gitInformation.TimeStamp) ? "null" : gitInformation.TimeStamp)}");
                reportTextWriter.WriteLine("  },");

                reportTextWriter.WriteLine($"  \"covered_percent\": {summaryResult.CoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)},");
                reportTextWriter.WriteLine("  \"covered_strength\": 0,");

                reportTextWriter.WriteLine("  \"line_counts\": {");
                reportTextWriter.WriteLine($"    \"missed\": {missedLines.ToString(CultureInfo.InvariantCulture)},");
                reportTextWriter.WriteLine($"    \"covered\": {summaryResult.CoverableLines.ToString(CultureInfo.InvariantCulture)},");
                reportTextWriter.WriteLine($"    \"total\": {summaryResult.TotalLines.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)}");
                reportTextWriter.WriteLine("  },");

                reportTextWriter.WriteLine("  \"source_files\": [");

                var assembliesWithClasses = summaryResult.Assemblies
                    .Where(a => a.Classes.Any())
                    .ToArray();

                foreach (var assembly in assembliesWithClasses)
                {
                    foreach (var clazz in assembly.Classes)
                    {
                        foreach (var file in clazz.Files)
                        {
                            if (processedFiles.Contains(file.Path))
                            {
                                continue;
                            }

                            processedFiles.Add(file.Path);

                            missedLines = file.CoverableLines - file.CoveredLines;

                            if (processedFiles.Count > 1)
                            {
                                reportTextWriter.WriteLine(",");
                            }

                            reportTextWriter.WriteLine("    {");
                            reportTextWriter.WriteLine($"      \"blob_id\": {JsonSerializer.Serialize(GitHelper.GetFileHash(file.Path))},");
                            reportTextWriter.Write($"      \"coverage\": [");

                            var lineCoverage = file.LineCoverage;

                            for (int i = 0; i < lineCoverage.Count; i++)
                            {
                                if (i > 0)
                                {
                                    reportTextWriter.Write(",");
                                }

                                if (lineCoverage[i] < 0)
                                {
                                    reportTextWriter.Write("null");
                                }
                                else
                                {
                                    reportTextWriter.Write(lineCoverage[i].ToString(CultureInfo.InvariantCulture));
                                }
                            }

                            reportTextWriter.WriteLine($"],");
                            reportTextWriter.WriteLine($"      \"covered_percent\": {file.CoverageQuota.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)},");
                            reportTextWriter.WriteLine($"      \"covered_strength\": 0,"); // TODO: Implement
                            reportTextWriter.WriteLine("      \"line_counts\": {");
                            reportTextWriter.WriteLine($"        \"missed\": {missedLines.ToString(CultureInfo.InvariantCulture)},");
                            reportTextWriter.WriteLine($"        \"covered\": {file.CoverableLines.ToString(CultureInfo.InvariantCulture)},");
                            reportTextWriter.WriteLine($"        \"total\": {file.TotalLines.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)}");
                            reportTextWriter.WriteLine("      },");
                            reportTextWriter.WriteLine($"      \"name\": {JsonSerializer.Serialize(file.Path)}");
                            reportTextWriter.Write("    }");
                        }
                    }
                }

                reportTextWriter.WriteLine(string.Empty);
                reportTextWriter.WriteLine("  ],");

                reportTextWriter.WriteLine("  \"repo_token\": \"\"");

                reportTextWriter.Write("}");

                reportTextWriter.Flush();
            }
        }
    }
}
