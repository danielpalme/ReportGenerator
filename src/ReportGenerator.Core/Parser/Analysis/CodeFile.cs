using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser.FileReading;

namespace Palmmedia.ReportGenerator.Core.Parser.Analysis
{
    /// <summary>
    /// Represents a source code file.
    /// </summary>
    public class CodeFile
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(CodeFile));

        /// <summary>
        /// The line coverage by test method.
        /// </summary>
        private readonly IDictionary<TestMethod, CoverageByTrackedMethod> lineCoveragesByTestMethod = new Dictionary<TestMethod, CoverageByTrackedMethod>();

        /// <summary>
        /// The method metrics of the class.
        /// </summary>
        private readonly HashSet<MethodMetric> methodMetrics = new HashSet<MethodMetric>();

        /// <summary>
        /// The code elements.
        /// </summary>
        private readonly HashSet<CodeElement> codeElements = new HashSet<CodeElement>();

        /// <summary>
        /// Array containing the coverage information by line number.
        /// -1: Not coverable
        /// 0: Not visited
        /// >0: Number of visits
        /// </summary>
        private int[] lineCoverage;

        /// <summary>
        /// Array containing the line visit status by line number.
        /// </summary>
        private LineVisitStatus[] lineVisitStatus;

        /// <summary>
        /// The branches by line number.
        /// </summary>
        private IDictionary<int, ICollection<Branch>> branches;

        /// <summary>
        /// The optional additional file reader.
        /// </summary>
        private IFileReader additionalFileReader;

        /// <summary>
        /// The number of covered lines.
        /// </summary>
        private int coveredLines;

        /// <summary>
        /// The number of coverable lines.
        /// </summary>
        private int coverableLines;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFile" /> class.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="lineCoverage">The line coverage.</param>
        /// <param name="lineVisitStatus">The line visit status.</param>
        internal CodeFile(string path, int[] lineCoverage, LineVisitStatus[] lineVisitStatus)
            : this(path, lineCoverage, lineVisitStatus, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFile" /> class.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="lineCoverage">The line coverage.</param>
        /// <param name="lineVisitStatus">The line visit status.</param>
        /// <param name="additionalFileReader">The optional additional file reader.</param>
        internal CodeFile(string path, int[] lineCoverage, LineVisitStatus[] lineVisitStatus, IFileReader additionalFileReader)
            : this(path, lineCoverage, lineVisitStatus, null, additionalFileReader)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFile" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="lineCoverage">The line coverage.</param>
        /// <param name="lineVisitStatus">The line visit status.</param>
        /// <param name="branches">The branches.</param>
        internal CodeFile(string path, int[] lineCoverage, LineVisitStatus[] lineVisitStatus, IDictionary<int, ICollection<Branch>> branches)
            : this(path, lineCoverage, lineVisitStatus, branches, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFile" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="lineCoverage">The line coverage.</param>
        /// <param name="lineVisitStatus">The line visit status.</param>
        /// <param name="branches">The branches.</param>
        /// <param name="additionalFileReader">The optional additional file reader.</param>
        internal CodeFile(
            string path,
            int[] lineCoverage,
            LineVisitStatus[] lineVisitStatus,
            IDictionary<int, ICollection<Branch>> branches,
            IFileReader additionalFileReader)
        {
            if (lineCoverage == null)
            {
                throw new ArgumentNullException(nameof(lineCoverage));
            }

            if (lineVisitStatus == null)
            {
                throw new ArgumentNullException(nameof(lineVisitStatus));
            }

            if (lineCoverage.LongLength != lineVisitStatus.LongLength)
            {
                throw new ArgumentException("Length of 'lineCoverage' and 'lineVisitStatus' must match", nameof(lineVisitStatus));
            }

            this.Path = path ?? throw new ArgumentNullException(nameof(path));
            this.lineCoverage = lineCoverage;
            this.lineVisitStatus = lineVisitStatus;
            this.branches = branches;
            this.additionalFileReader = additionalFileReader;

            this.coveredLines = this.lineCoverage.Count(l => l > 0);
            this.coverableLines = this.lineCoverage.Count(l => l >= 0);
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>The path.</value>
        public string Path { get; }

        /// <summary>
        /// Gets the test methods.
        /// </summary>
        /// <value>
        /// The test methods.
        /// </value>
        public IEnumerable<TestMethod> TestMethods => this.lineCoveragesByTestMethod.Keys;

        /// <summary>
        /// Gets the method metrics.
        /// </summary>
        /// <value>The method metrics.</value>
        public IEnumerable<MethodMetric> MethodMetrics => this.methodMetrics;

        /// <summary>
        /// Gets the code elements.
        /// </summary>
        /// <value>
        /// The code elements.
        /// </value>
        public IEnumerable<CodeElement> CodeElements => this.codeElements;

        /// <summary>
        /// Gets the number of covered lines.
        /// </summary>
        /// <value>The number of covered lines.</value>
        public int CoveredLines => this.coveredLines;

        /// <summary>
        /// Gets the number of coverable lines.
        /// </summary>
        /// <value>The number of coverable lines.</value>
        public int CoverableLines => this.coverableLines;

        /// <summary>
        /// Gets the number of total lines.
        /// </summary>
        /// <value>The number of total lines.</value>
        public int? TotalLines { get; private set; }

        /// <summary>
        /// Gets line coverage information by line number for this file.
        /// </summary>
        public ReadOnlyCollection<int> LineCoverage => Array.AsReadOnly(this.lineCoverage);

        /// <summary>
        /// Gets line visit status by line number for this file.
        /// </summary>
        public ReadOnlyCollection<LineVisitStatus> LineVisitStatus => Array.AsReadOnly(this.lineVisitStatus);

        /// <summary>
        /// Gets the branches by line number.
        /// </summary>
        public IDictionary<int, ICollection<Branch>> BranchesByLine => this.branches ?? new Dictionary<int, ICollection<Branch>>();

        /// <summary>
        /// Gets the number of covered branches.
        /// </summary>
        /// <value>
        /// The number of covered branches.
        /// </value>
        public int? CoveredBranches
        {
            get
            {
                if (this.branches == null)
                {
                    return null;
                }

                return this.branches.SafeSum(l => l.Value.Count(b => b.BranchVisits > 0));
            }
        }

        /// <summary>
        /// Gets the number of total branches.
        /// </summary>
        /// <value>
        /// The number of total branches.
        /// </value>
        public int? TotalBranches
        {
            get
            {
                if (this.branches == null)
                {
                    return null;
                }

                return this.branches.SafeSum(l => l.Value.Count);
            }
        }

        /// <summary>
        /// Gets the number of covered code elements.
        /// </summary>
        /// <value>
        /// The number of covered code elements.
        /// </value>
        public int CoveredCodeElements
        {
            get
            {
                return this.CodeElements.Count(
                    x => this.lineCoverage.Skip(x.FirstLine)
                        .Take(x.LastLine - x.FirstLine + 1)
                        .Any(y => y > 0));
            }
        }

        /// <summary>
        /// Gets the number of fully covered code elements.
        /// </summary>
        /// <value>
        /// The number of fully covered code elements.
        /// </value>
        public int FullCoveredCodeElements
        {
            get
            {
                return this.CodeElements.Count(
                    x => this.lineCoverage.Skip(x.FirstLine)
                        .Take(x.LastLine - x.FirstLine + 1)
                        .All(y => y > 0));
            }
        }

        /// <summary>
        /// Gets the number of total code elements.
        /// </summary>
        /// <value>
        /// The number of total code elements.
        /// </value>
        public int TotalCodeElements => this.codeElements.Count;

        /// <summary>
        /// Gets the coverage quota of the file.
        /// </summary>
        /// <value>The coverage quota.</value>
        public decimal? CoverageQuota
        {
            get
            {
                return (this.CoverableLines == 0) ? (decimal?)null : MathExtensions.CalculatePercentage(this.CoveredLines, this.CoverableLines);
            }
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Path;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || !obj.GetType().Equals(typeof(CodeFile)))
            {
                return false;
            }
            else
            {
                var codeFile = (CodeFile)obj;
                string fileNameToCompare = codeFile.Path.Substring(codeFile.Path.LastIndexOf('\\') + 1);

                string fileName = this.Path.Substring(this.Path.LastIndexOf('\\') + 1);
                return fileName.Equals(fileNameToCompare, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => this.Path.GetHashCode();

        /// <summary>
        /// Calculates the coverage quota in a given range of lines.
        /// </summary>
        /// <param name="firstLine">The first line.</param>
        /// <param name="lastLine">The last line.</param>
        /// <returns>The coverage quota or <code>null</code> if not applicable.</returns>
        internal decimal? CoverageQuotaInRange(int firstLine, int lastLine)
        {
            if (firstLine < 0
                || firstLine >= this.lineVisitStatus.Length
                || lastLine < 0
                || lastLine >= this.lineVisitStatus.Length
                || firstLine > lastLine)
            {
                return null;
            }

            int coverableLines = 0;
            int coveredLines = 0;

            for (int i = firstLine; i <= lastLine; i++)
            {
                if (this.lineVisitStatus[i] != Analysis.LineVisitStatus.NotCoverable)
                {
                    coverableLines++;
                }

                if (this.lineVisitStatus[i] > Analysis.LineVisitStatus.NotCovered)
                {
                    coveredLines++;
                }
            }

            return (coverableLines == 0) ? (decimal?)null : MathExtensions.CalculatePercentage(coveredLines, coverableLines);
        }

        /// <summary>
        /// Adds the coverage by test method.
        /// </summary>
        /// <param name="testMethod">The test method.</param>
        /// <param name="trackedMethodCoverage">The coverage by for test method.</param>
        internal void AddCoverageByTestMethod(TestMethod testMethod, CoverageByTrackedMethod trackedMethodCoverage)
        {
            if (testMethod == null)
            {
                throw new ArgumentNullException(nameof(testMethod));
            }

            if (trackedMethodCoverage == null)
            {
                throw new ArgumentNullException(nameof(trackedMethodCoverage));
            }

            CoverageByTrackedMethod existingTrackedMethodCoverage;
            if (!this.lineCoveragesByTestMethod.TryGetValue(testMethod, out existingTrackedMethodCoverage))
            {
                this.lineCoveragesByTestMethod.Add(testMethod, trackedMethodCoverage);
            }
            else
            {
                this.lineCoveragesByTestMethod[testMethod] = MergeCoverageByTrackedMethod(existingTrackedMethodCoverage, trackedMethodCoverage);
            }
        }

        /// <summary>
        /// Adds the given method metric.
        /// </summary>
        /// <param name="methodMetric">The method metric.</param>
        internal void AddMethodMetric(MethodMetric methodMetric)
        {
            this.methodMetrics.Add(methodMetric);
        }

        /// <summary>
        /// Adds the code element.
        /// </summary>
        /// <param name="codeElement">The code element.</param>
        internal void AddCodeElement(CodeElement codeElement)
        {
            this.codeElements.Add(codeElement);
        }

        /// <summary>
        /// Performs the analysis of the source file.
        /// </summary>
        /// <param name="fileReader">The file reader.</param>
        /// <returns>The analysis result.</returns>
        internal FileAnalysis AnalyzeFile(IFileReader fileReader)
        {
            string error = null;

            string[] lines = null;

            if (this.additionalFileReader != null)
            {
                lines = this.additionalFileReader.LoadFile(this.Path, out error);
            }

            if (this.additionalFileReader == null || error != null)
            {
                error = null;
                lines = fileReader.LoadFile(this.Path, out error);
            }

            if (error != null)
            {
                Logger.Error(error);
                lines = this.lineCoverage
                    .Select(l => string.Empty)
                    .ToArray();
            }

            this.TotalLines = lines.Length;

            int currentLineNumber = 0;

            var result = new FileAnalysis(this.Path, error);
            ICollection<Branch> branchesOfLine = null;

            foreach (var line in lines)
            {
                currentLineNumber++;
                int visits = this.lineCoverage.Length > currentLineNumber ? this.lineCoverage[currentLineNumber] : -1;
                LineVisitStatus lineVisitStatus = this.lineVisitStatus.Length > currentLineNumber ? this.lineVisitStatus[currentLineNumber] : Analysis.LineVisitStatus.NotCoverable;

                var lineCoverageByTestMethod = this.lineCoveragesByTestMethod
                    .ToDictionary(
                    l => l.Key,
                    l =>
                    {
                        if (l.Value.Coverage.Length > currentLineNumber)
                        {
                            return new ShortLineAnalysis(l.Value.Coverage[currentLineNumber], l.Value.LineVisitStatus[currentLineNumber]);
                        }
                        else
                        {
                            return new ShortLineAnalysis(-1, Analysis.LineVisitStatus.NotCoverable);
                        }
                    });

                if (this.branches != null && this.branches.TryGetValue(currentLineNumber, out branchesOfLine))
                {
                    result.AddLineAnalysis(
                        new LineAnalysis(
                            visits,
                            lineVisitStatus,
                            lineCoverageByTestMethod,
                            currentLineNumber,
                            line.TrimEnd(),
                            branchesOfLine.Count(b => b.BranchVisits > 0),
                            branchesOfLine.Count));
                }
                else
                {
                    result.AddLineAnalysis(
                        new LineAnalysis(
                            visits,
                            lineVisitStatus,
                            lineCoverageByTestMethod,
                            currentLineNumber,
                            line.TrimEnd()));
                }
            }

            return result;
        }

        /// <summary>
        /// Merges the given file with the current instance.
        /// </summary>
        /// <param name="file">The file to merge.</param>
        internal void Merge(CodeFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            // Resize coverage array if necessary
            if (file.lineCoverage.LongLength > this.lineCoverage.LongLength)
            {
                int[] newLineCoverage = new int[file.lineCoverage.LongLength];

                Array.Copy(this.lineCoverage, newLineCoverage, this.lineCoverage.LongLength);

                for (long i = this.lineCoverage.LongLength; i < file.lineCoverage.LongLength; i++)
                {
                    newLineCoverage[i] = -1;
                }

                this.lineCoverage = newLineCoverage;
            }

            // Resize line visit status array if necessary
            if (file.lineVisitStatus.LongLength > this.lineVisitStatus.LongLength)
            {
                LineVisitStatus[] newLineVisitStatus = new LineVisitStatus[file.lineVisitStatus.LongLength];
                Array.Copy(this.lineVisitStatus, newLineVisitStatus, this.lineVisitStatus.LongLength);
                this.lineVisitStatus = newLineVisitStatus;
            }

            if (file.branches != null)
            {
                if (this.branches == null)
                {
                    this.branches = new Dictionary<int, ICollection<Branch>>();
                }

                foreach (var branchByLine in file.branches)
                {
                    ICollection<Branch> existingBranches = null;

                    if (this.branches.TryGetValue(branchByLine.Key, out existingBranches))
                    {
                        foreach (var branch in branchByLine.Value)
                        {
                            Branch existingBranch = existingBranches.FirstOrDefault(b => b.Equals(branch));
                            if (existingBranch != null)
                            {
                                existingBranch.BranchVisits += branch.BranchVisits;
                            }
                            else
                            {
                                existingBranches.Add(branch);
                            }
                        }
                    }
                    else
                    {
                        this.branches.Add(branchByLine);
                    }
                }
            }

            for (long i = 0; i < file.lineCoverage.LongLength; i++)
            {
                int coverage = this.lineCoverage[i];

                if (coverage < 0)
                {
                    coverage = file.lineCoverage[i];
                }
                else if (file.lineCoverage[i] > 0)
                {
                    coverage += file.lineCoverage[i];
                }

                this.lineCoverage[i] = coverage;
            }

            this.coveredLines = this.lineCoverage.Count(l => l > 0);
            this.coverableLines = this.lineCoverage.Count(l => l >= 0);

            for (long i = 0; i < file.lineVisitStatus.LongLength; i++)
            {
                int lineVisitStatus = Math.Max((int)this.lineVisitStatus[i], (int)file.lineVisitStatus[i]);

                this.lineVisitStatus[i] = (LineVisitStatus)lineVisitStatus;

                if (this.lineVisitStatus[i] == Analysis.LineVisitStatus.PartiallyCovered
                    && this.branches != null
                    && this.branches.TryGetValue((int)i, out ICollection<Branch> branches))
                {
                    if (branches.All(b => b.BranchVisits > 0))
                    {
                        this.lineVisitStatus[i] = Analysis.LineVisitStatus.Covered;
                    }
                }
            }

            foreach (var lineCoverageByTestMethod in file.lineCoveragesByTestMethod)
            {
                CoverageByTrackedMethod existingTrackedMethodCoverage = null;

                this.lineCoveragesByTestMethod.TryGetValue(lineCoverageByTestMethod.Key, out existingTrackedMethodCoverage);

                if (existingTrackedMethodCoverage == null)
                {
                    this.lineCoveragesByTestMethod.Add(lineCoverageByTestMethod);
                }
                else
                {
                    this.lineCoveragesByTestMethod[lineCoverageByTestMethod.Key] = MergeCoverageByTrackedMethod(existingTrackedMethodCoverage, lineCoverageByTestMethod.Value);
                }
            }

            foreach (var methodMetric in file.methodMetrics)
            {
                var existingMethodMetric = this.methodMetrics.FirstOrDefault(m => m.Equals(methodMetric));
                if (existingMethodMetric != null)
                {
                    existingMethodMetric.Merge(methodMetric);
                }
                else
                {
                    this.AddMethodMetric(methodMetric);
                }
            }

            foreach (var codeElement in file.codeElements)
            {
                this.codeElements.Add(codeElement);
            }

            foreach (var codeElement in this.codeElements)
            {
                codeElement.ApplyMaximumCoverageQuota(this.CoverageQuotaInRange(codeElement.FirstLine, codeElement.LastLine));
            }

            if (file.additionalFileReader == null)
            {
                file.additionalFileReader = this.additionalFileReader;
            }
        }

        /// <summary>
        /// Merges the two tracked method coverage.
        /// </summary>
        /// <param name="existingTrackedMethodCoverage">The existing tracked method coverage.</param>
        /// <param name="lineCoverageByTestMethod">The new line coverage by test method.</param>
        /// <returns>The merged tracked method coverage.</returns>
        private static CoverageByTrackedMethod MergeCoverageByTrackedMethod(CoverageByTrackedMethod existingTrackedMethodCoverage, CoverageByTrackedMethod lineCoverageByTestMethod)
        {
            // Resize coverage array if neccessary
            if (lineCoverageByTestMethod.Coverage.LongLength > existingTrackedMethodCoverage.Coverage.LongLength)
            {
                int[] newLineCoverage = new int[lineCoverageByTestMethod.Coverage.LongLength];

                Array.Copy(lineCoverageByTestMethod.Coverage, newLineCoverage, lineCoverageByTestMethod.Coverage.LongLength);

                for (long i = existingTrackedMethodCoverage.Coverage.LongLength; i < lineCoverageByTestMethod.Coverage.LongLength; i++)
                {
                    newLineCoverage[i] = -1;
                }

                existingTrackedMethodCoverage.Coverage = newLineCoverage;
            }

            // Resize line visit status array if neccessary
            if (lineCoverageByTestMethod.LineVisitStatus.LongLength > existingTrackedMethodCoverage.LineVisitStatus.LongLength)
            {
                LineVisitStatus[] newLineVisitStatus = new LineVisitStatus[lineCoverageByTestMethod.LineVisitStatus.LongLength];
                Array.Copy(lineCoverageByTestMethod.LineVisitStatus, newLineVisitStatus, lineCoverageByTestMethod.LineVisitStatus.LongLength);
                existingTrackedMethodCoverage.LineVisitStatus = newLineVisitStatus;
            }

            for (long i = 0; i < lineCoverageByTestMethod.Coverage.LongLength; i++)
            {
                int coverage = existingTrackedMethodCoverage.Coverage[i];

                if (coverage < 0)
                {
                    coverage = lineCoverageByTestMethod.Coverage[i];
                }
                else if (lineCoverageByTestMethod.Coverage[i] > 0)
                {
                    coverage += lineCoverageByTestMethod.Coverage[i];
                }

                existingTrackedMethodCoverage.Coverage[i] = coverage;
            }

            for (long i = 0; i < lineCoverageByTestMethod.LineVisitStatus.LongLength; i++)
            {
                int lineVisitStatus = Math.Max((int)existingTrackedMethodCoverage.LineVisitStatus[i], (int)lineCoverageByTestMethod.LineVisitStatus[i]);

                existingTrackedMethodCoverage.LineVisitStatus[i] = (LineVisitStatus)lineVisitStatus;
            }

            return existingTrackedMethodCoverage;
        }
    }
}
