using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using log4net;
using Palmmedia.ReportGenerator.Properties;

namespace Palmmedia.ReportGenerator.Parser.Analysis
{
    /// <summary>
    /// Represents a source code file.
    /// </summary>
    public class CodeFile
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CodeFile));

        /// <summary>
        /// The line coverage by test method.
        /// </summary>
        private readonly IDictionary<TestMethod, int[]> lineCoveragesByTestMethod = new Dictionary<TestMethod, int[]>();

        /// <summary>
        /// Array containing the coverage information by line number.
        /// -1: Not coverable
        /// 0: Not visited
        /// >0: Number of visits
        /// </summary>
        private int[] lineCoverage;

        /// <summary>
        /// The branches by line number.
        /// </summary>
        private IDictionary<int, List<Branch>> branches;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFile"/> class.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="lineCoverage">The line coverage.</param>
        internal CodeFile(string path, int[] lineCoverage)
            : this(path, lineCoverage, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFile"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="lineCoverage">The line coverage.</param>
        /// <param name="branches">The branches.</param>
        internal CodeFile(string path, int[] lineCoverage, IDictionary<int, List<Branch>> branches)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (lineCoverage == null)
            {
                throw new ArgumentNullException("lineCoverage");
            }

            this.Path = path;
            this.lineCoverage = lineCoverage;
            this.branches = branches;
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>The path.</value>
        public string Path { get; private set; }

        /// <summary>
        /// Gets the test methods.
        /// </summary>
        /// <value>
        /// The test methods.
        /// </value>
        public IEnumerable<TestMethod> TestMethods
        {
            get
            {
                return this.lineCoveragesByTestMethod.Keys;
            }
        }

        /// <summary>
        /// Gets the number of covered lines.
        /// </summary>
        /// <value>The number of covered lines.</value>
        public int CoveredLines
        {
            get
            {
                return this.lineCoverage.Count(l => l > 0);
            }
        }

        /// <summary>
        /// Gets the number of coverable lines.
        /// </summary>
        /// <value>The number of coverable lines.</value>
        public int CoverableLines
        {
            get
            {
                return this.lineCoverage.Count(l => l >= 0);
            }
        }

        /// <summary>
        /// Gets the number of total lines.
        /// </summary>
        /// <value>The number of total lines.</value>
        public int? TotalLines { get; private set; }

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

                return this.branches.Sum(l => l.Value.Count(b => b.BranchVisits > 0));
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

                return this.branches.Sum(l => l.Value.Count);
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
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
                return codeFile.Path.Equals(this.Path);
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.Path.GetHashCode();
        }

        /// <summary>
        /// Adds the coverage by test method.
        /// </summary>
        /// <param name="testMethod">The test method.</param>
        /// <param name="coverage">The coverage.</param>
        internal void AddCoverageByTestMethod(TestMethod testMethod, int[] coverage)
        {
            if (testMethod == null)
            {
                throw new ArgumentNullException("testMethod");
            }

            if (coverage == null)
            {
                throw new ArgumentNullException("coverage");
            }

            this.lineCoveragesByTestMethod.Add(testMethod, coverage);
        }

        /// <summary>
        /// Performs the analysis of the source file.
        /// </summary>
        /// <returns>The analysis result.</returns>
        internal FileAnalysis AnalyzeFile()
        {
            if (!System.IO.File.Exists(this.Path))
            {
                string error = string.Format(CultureInfo.InvariantCulture, " " + Resources.FileDoesNotExist, this.Path);
                Logger.Error(error);
                return new FileAnalysis(this.Path, error);
            }

            try
            {
                string[] lines = System.IO.File.ReadAllLines(this.Path);

                this.TotalLines = lines.Length;

                int currentLineNumber = 0;

                var result = new FileAnalysis(this.Path);
                List<Branch> branchesOfLine = null;

                foreach (var line in lines)
                {
                    currentLineNumber++;
                    int visits = this.lineCoverage.Length > currentLineNumber ? this.lineCoverage[currentLineNumber] : -1;

                    var lineCoverageByTestMethod = this.lineCoveragesByTestMethod
                        .ToDictionary(l => l.Key, l => new ShortLineAnalysis(l.Value.Length > currentLineNumber ? l.Value[currentLineNumber] : -1));

                    if (this.branches != null && this.branches.TryGetValue(currentLineNumber, out branchesOfLine))
                    {
                        result.AddLineAnalysis(
                            new LineAnalysis(
                                visits,
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
                                lineCoverageByTestMethod,
                                currentLineNumber,
                                line.TrimEnd()));
                    }
                }

                return result;
            }
            catch (IOException ex)
            {
                string error = string.Format(CultureInfo.InvariantCulture, " " + Resources.ErrorDuringReadingFile, this.Path, ex.Message);
                Logger.Error(error);
                return new FileAnalysis(this.Path, error);
            }
            catch (UnauthorizedAccessException ex)
            {
                string error = string.Format(CultureInfo.InvariantCulture, " " + Resources.ErrorDuringReadingFile, this.Path, ex.Message);
                Logger.Error(error);
                return new FileAnalysis(this.Path, error);
            }
        }

        /// <summary>
        /// Merges the given file with the current instance.
        /// </summary>
        /// <param name="file">The file to merge.</param>
        internal void Merge(CodeFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            // Resize coverage array if neccessary
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

            foreach (var lineCoverageByTestMethod in file.lineCoveragesByTestMethod)
            {
                int[] existingLineCoverageByTestMethod = null;

                this.lineCoveragesByTestMethod.TryGetValue(lineCoverageByTestMethod.Key, out existingLineCoverageByTestMethod);

                if (existingLineCoverageByTestMethod == null)
                {
                    this.lineCoveragesByTestMethod.Add(lineCoverageByTestMethod);
                }
                else
                {
                    // Resize coverage array if neccessary
                    if (existingLineCoverageByTestMethod.LongLength > lineCoverageByTestMethod.Value.LongLength)
                    {
                        int[] newLineCoverage = new int[lineCoverageByTestMethod.Value.LongLength];

                        Array.Copy(lineCoverageByTestMethod.Value, newLineCoverage, lineCoverageByTestMethod.Value.LongLength);

                        for (long i = existingLineCoverageByTestMethod.LongLength; i < lineCoverageByTestMethod.Value.LongLength; i++)
                        {
                            newLineCoverage[i] = -1;
                        }

                        existingLineCoverageByTestMethod = newLineCoverage;
                    }

                    for (long i = 0; i < lineCoverageByTestMethod.Value.LongLength; i++)
                    {
                        int coverage = existingLineCoverageByTestMethod[i];

                        if (coverage < 0)
                        {
                            coverage = lineCoverageByTestMethod.Value[i];
                        }
                        else if (lineCoverageByTestMethod.Value[i] > 0)
                        {
                            coverage += lineCoverageByTestMethod.Value[i];
                        }

                        existingLineCoverageByTestMethod[i] = coverage;
                    }

                    this.lineCoveragesByTestMethod[lineCoverageByTestMethod.Key] = existingLineCoverageByTestMethod;
                }
            }

            if (file.branches != null)
            {
                if (this.branches == null)
                {
                    this.branches = new Dictionary<int, List<Branch>>();
                }

                foreach (var branchByLine in file.branches)
                {
                    List<Branch> branches = null;

                    if (this.branches.TryGetValue(branchByLine.Key, out branches))
                    {
                        foreach (var branch in branchByLine.Value)
                        {
                            Branch existingBranch = branches.FirstOrDefault(b => b.Equals(branch));
                            if (existingBranch != null)
                            {
                                existingBranch.BranchVisits += branch.BranchVisits;
                            }
                            else
                            {
                                branches.Add(branch);
                            }
                        }
                    }
                    else
                    {
                        this.branches.Add(branchByLine);
                    }
                }
            }
        }
    }
}
