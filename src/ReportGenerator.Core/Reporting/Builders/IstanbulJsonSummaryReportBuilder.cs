using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders
{

    /// <summary>
    /// Creates summary report in JSON format, matching istanbul.js json-summary output (no reports for classes are generated).
    /// istanbul json-summary Typescript typings are here (for each entry in the object): https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/istanbul-lib-coverage/index.d.ts#L7-L12
    /// The format as {"total": { "total": number, "covered": number, "skipped": number, "pct": number }, "<filePath>": { "total": number, "covered": number, "skipped": number, "pct": number }}
    /// </summary>
    public class IstanbulJsonSummaryReportBuilder : IReportBuilder
    {
        private class IstanbulCoverageInfo
        {
            public IstanbulCoverageInfo(int total, int covered)
            {
                this.Total = total;
                this.Covered = covered;
            }

            public int Total { get; set; }

            public int Covered { get; set; }

            public int Skipped { get => this.Total - this.Covered; }

            public double Pct { get => (double)this.Covered / this.Total * 100; }


            public string ToJSON()
            {
                return $"{{ \"total\": {this.Total}, \"covered\": {this.Covered}, \"skipped\": {this.Total}, \"pct\": {(double)this.Covered / this.Total * 100} }}";
            }
        }

        private class IstanbulCoverageSummary
        {
            public IstanbulCoverageInfo Lines { get; set; }

            public IstanbulCoverageInfo Statements { get; set; }

            public IstanbulCoverageInfo Functions { get; set; }

            public IstanbulCoverageInfo Branches { get; set; }

            public string ToJSON()
            {
                return $"{{ \"lines\": {this.Lines.ToJSON()}, \"statements\": {this.Statements.ToJSON()}, \"functions\": {this.Functions.ToJSON()}, \"branches\": {this.Branches.ToJSON()} }}";
            }
        }

        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(IstanbulJsonSummaryReportBuilder));

        /// <summary>
        /// Gets the report type.
        /// </summary>
        /// <value>
        /// The report format.
        /// </value>
        public virtual string ReportType => "IstanbulJSONSummary";

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

            string targetPath = Path.Combine(targetDirectory, "istanbul-coverage-summary.json");

            Logger.InfoFormat(Resources.WritingReportFile, targetPath);

            using (var reportTextWriter = new StreamWriter(new FileStream(targetPath, FileMode.Create), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {

                IstanbulCoverageInfo totalLines = new IstanbulCoverageInfo(summaryResult.CoverableLines, summaryResult.CoveredLines);
                IstanbulCoverageInfo totalBranches = new IstanbulCoverageInfo(summaryResult.CoveredBranches.GetValueOrDefault(), summaryResult.TotalBranches.GetValueOrDefault());
                IstanbulCoverageInfo totalFunctions = new IstanbulCoverageInfo(summaryResult.CoveredBranches.GetValueOrDefault(), summaryResult.TotalBranches.GetValueOrDefault());
                IstanbulCoverageInfo totalStatements = new IstanbulCoverageInfo(0, 0); // Would a "summable" metric be good here maybe?
                IstanbulCoverageSummary totalsSummary = new IstanbulCoverageSummary() { Lines = totalLines, Branches = totalBranches, Functions = totalFunctions };

                reportTextWriter.Write($"{{ \"total\": { totalsSummary.ToJSON() } ");


                foreach (var assembly in summaryResult.Assemblies)
                {
                    foreach (var @class in assembly.Classes)
                    {
                        IstanbulCoverageInfo classLines = new IstanbulCoverageInfo(@class.CoverableLines, summaryResult.CoveredLines);
                        IstanbulCoverageInfo classBranches = new IstanbulCoverageInfo(@class.CoveredBranches.GetValueOrDefault(), @class.TotalBranches.GetValueOrDefault());
                        IstanbulCoverageInfo classFunctions = new IstanbulCoverageInfo(@class.CoveredCodeElements, @class.CoveredCodeElements);
                        IstanbulCoverageInfo classStatements = new IstanbulCoverageInfo(0, 0); // Would a "summable" metric be good here maybe?
                        IstanbulCoverageSummary classSummary = new IstanbulCoverageSummary() { Lines = totalLines, Branches = totalBranches, Functions = totalFunctions };

                        string entryName = $"{ JsonSerializer.EscapeString(assembly.Name) }.{ JsonSerializer.EscapeString(@class.Name)}"
                        reportTextWriter.Write($", \"{entryName}\": {classSummary.ToJSON()}");
                    }
                }

                reportTextWriter.Write("}");

                reportTextWriter.Flush();
            }
        }
    }
}
