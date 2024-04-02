using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Palmmedia.ReportGenerator.Core.Parser.FileReading;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser.Analysis
{
    /// <summary>
    /// This is a test class for CodeFile and is intended
    /// to contain all CodeFile Unit Tests
    /// </summary>
    [Collection("FileManager")]
    public class CodeFileTest
    {
        /// <summary>
        /// A test for the Constructor
        /// </summary>
        [Fact]
        public void Constructor()
        {
            var sut = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, 0, 2 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered });

            Assert.Equal(2, sut.CoverableLines);
            Assert.Equal(1, sut.CoveredLines);

            Assert.Null(sut.CoveredBranches);
            Assert.Null(sut.TotalBranches);
        }

        /// <summary>
        /// A test for the Constructor
        /// </summary>
        [Fact]
        public void Constructor_WithBranches()
        {
            var branches = new Dictionary<int, ICollection<Branch>>()
            {
                { 1, new List<Branch>() { new Branch(1, "1"), new Branch(0, "2") } },
                { 2, new List<Branch>() { new Branch(0, "3"), new Branch(2, "4") } }
            };

            var sut = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered }, branches);

            Assert.Equal(2, sut.CoveredBranches);
            Assert.Equal(4, sut.TotalBranches);
        }

        /// <summary>
        /// A test for Merge
        /// </summary>
        [Fact]
        public void Merge_MergeOneMethodMetric_MethodMetricIsStored()
        {
            var sut = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered });
            var methodMetric = new MethodMetric("Test", "Test", Enumerable.Empty<Metric>());
            sut.AddMethodMetric(methodMetric);

            var codeFileToMerge = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered });

            sut.Merge(codeFileToMerge);

            Assert.Equal(methodMetric, sut.MethodMetrics.First());
            Assert.Single(sut.MethodMetrics);
        }

        /// <summary>
        /// A test for Merge
        /// </summary>
        [Fact]
        public void Merge_CodeFileToMergeHasNoBranches_BranchCoverageInformationIsUpdated()
        {
            var branches = new Dictionary<int, ICollection<Branch>>()
            {
                { 1, new List<Branch>() { new Branch(1, "1"), new Branch(0, "2") } },
                { 2, new List<Branch>() { new Branch(0, "3"), new Branch(2, "4") } }
            };
            var sut = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered }, branches);

            var codeFileToMerge = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered });

            sut.Merge(codeFileToMerge);

            Assert.Equal(2, sut.CoveredBranches);
            Assert.Equal(4, sut.TotalBranches);
        }

        /// <summary>
        /// A test for Merge
        /// </summary>
        [Fact]
        public void Merge_TargetCodeFileHasNoBranches_BranchCoverageInformationIsUpdated()
        {
            var branches = new Dictionary<int, ICollection<Branch>>()
            {
                { 1, new List<Branch>() { new Branch(1, "1"), new Branch(0, "2") } },
                { 2, new List<Branch>() { new Branch(0, "3"), new Branch(2, "4") } }
            };
            var sut = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered });

            var codeFileToMerge = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered }, branches);

            sut.Merge(codeFileToMerge);

            Assert.Equal(2, sut.CoveredBranches);
            Assert.Equal(4, sut.TotalBranches);
        }

        /// <summary>
        /// A test for Merge
        /// </summary>
        [Fact]
        public void Merge_AllBranchesCovered_LineVisitStatusUpdated()
        {
            var branches = new Dictionary<int, ICollection<Branch>>()
            {
                { 1, new List<Branch>() { new Branch(1, "1"), new Branch(0, "2") } },
                { 2, new List<Branch>() { new Branch(0, "3"), new Branch(0, "4") } }
            };
            var sut = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, 1, 0 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.PartiallyCovered, LineVisitStatus.NotCovered }, branches);

            var branches2 = new Dictionary<int, ICollection<Branch>>()
            {
                { 1, new List<Branch>() { new Branch(0, "1"), new Branch(1, "2") } },
                { 2, new List<Branch>() { new Branch(0, "3"), new Branch(2, "4") } }
            };

            var codeFileToMerge = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, 1, 0 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.PartiallyCovered, LineVisitStatus.PartiallyCovered }, branches2);

            sut.Merge(codeFileToMerge);

            Assert.Equal(3, sut.CoveredBranches);
            Assert.Equal(4, sut.TotalBranches);
            Assert.Equal(LineVisitStatus.Covered, sut.LineVisitStatus[1]);
            Assert.Equal(LineVisitStatus.PartiallyCovered, sut.LineVisitStatus[2]);
        }

        /// <summary>
        /// A test for Merge
        /// </summary>
        [Fact]
        public void Merge_MergeCodeFileWithEqualLengthCoverageArray_CoverageInformationIsUpdated()
        {
            var sut = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered });
            var codeFileToMerge = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, 0, 1, -1, 0, 1, -1, 0, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered });
            var testMethod = new TestMethod("TestFull", "Test");
            codeFileToMerge.AddCoverageByTestMethod(testMethod, new CoverageByTrackedMethod() { Coverage = new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 }, LineVisitStatus = new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered } });

            sut.Merge(codeFileToMerge);

            Assert.Equal(8, sut.CoverableLines);
            Assert.Equal(5, sut.CoveredLines);

            Assert.Null(sut.CoveredBranches);
            Assert.Null(sut.TotalBranches);

            Assert.Contains(testMethod, sut.TestMethods);
        }

        /// <summary>
        /// A test for Merge
        /// </summary>
        [Fact]
        public void Merge_MergeCodeFileWithLongerCoverageArray_CoverageInformationIsUpdated()
        {
            var branches = new Dictionary<int, ICollection<Branch>>()
            {
                { 1, new List<Branch>() { new Branch(1, "1"), new Branch(0, "2") } },
                { 2, new List<Branch>() { new Branch(0, "3"), new Branch(2, "4") } }
            };
            var sut = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered }, branches);
            var testMethod = new TestMethod("TestFull", "Test");
            sut.AddCoverageByTestMethod(testMethod, new CoverageByTrackedMethod() { Coverage = new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 }, LineVisitStatus = new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered } });

            var branches2 = new Dictionary<int, ICollection<Branch>>()
            {
                { 1, new List<Branch>() { new Branch(4, "1"), new Branch(3, "5") } },
                { 3, new List<Branch>() { new Branch(0, "3"), new Branch(2, "4") } }
            };
            var codeFileToMerge = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered }, branches2);
            testMethod = new TestMethod("TestFull", "Test");
            codeFileToMerge.AddCoverageByTestMethod(testMethod, new CoverageByTrackedMethod() { Coverage = new int[] { -1, 0, 1, -1, 0, 1, -1, 0, 1, -1, 0, 1 }, LineVisitStatus = new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered } });

            sut.Merge(codeFileToMerge);

            Assert.Equal(10, sut.CoverableLines);
            Assert.Equal(6, sut.CoveredLines);

            Assert.Equal(4, sut.CoveredBranches);
            Assert.Equal(7, sut.TotalBranches);
        }

        /// <summary>
        /// A test for AnalyzeFile
        /// </summary>
        [Fact]
        public void AnalyzeFile_ExistingFile_AnalysisIsReturned()
        {
            var sut = new CodeFile("C:\\temp\\Program.cs", new int[] { -2, -1, 0, 1, 2 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.PartiallyCovered, LineVisitStatus.Covered });

            Assert.Null(sut.TotalLines);

            var fileAnalysis = sut.AnalyzeFile(new CachingFileReader(new LocalFileReader(), 0, null));

            Assert.NotNull(fileAnalysis);
            Assert.Null(fileAnalysis.Error);
            Assert.Equal(fileAnalysis.Path, fileAnalysis.Path);
            Assert.Equal(84, sut.TotalLines);
            Assert.Equal(84, fileAnalysis.Lines.Count());

            Assert.Equal(1, fileAnalysis.Lines.ElementAt(0).LineNumber);
            Assert.Equal(-1, fileAnalysis.Lines.ElementAt(0).LineVisits);
            Assert.Equal(LineVisitStatus.NotCoverable, fileAnalysis.Lines.ElementAt(0).LineVisitStatus);

            Assert.Equal(2, fileAnalysis.Lines.ElementAt(1).LineNumber);
            Assert.Equal(0, fileAnalysis.Lines.ElementAt(1).LineVisits);
            Assert.Equal(LineVisitStatus.NotCovered, fileAnalysis.Lines.ElementAt(1).LineVisitStatus);

            Assert.Equal(3, fileAnalysis.Lines.ElementAt(2).LineNumber);
            Assert.Equal(1, fileAnalysis.Lines.ElementAt(2).LineVisits);
            Assert.Equal(LineVisitStatus.PartiallyCovered, fileAnalysis.Lines.ElementAt(2).LineVisitStatus);

            Assert.Equal(4, fileAnalysis.Lines.ElementAt(3).LineNumber);
            Assert.Equal(2, fileAnalysis.Lines.ElementAt(3).LineVisits);
            Assert.Equal(LineVisitStatus.Covered, fileAnalysis.Lines.ElementAt(3).LineVisitStatus);
        }

        /// <summary>
        /// A test for AnalyzeFile
        /// </summary>
        [Fact]
        public void AnalyzeFile_ExistingFileWithTrackedMethods_AnalysisIsReturned()
        {
            var sut = new CodeFile("C:\\temp\\Program.cs", new int[] { -2, -1, 0, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered });
            var testMethod = new TestMethod("TestFull", "Test");
            sut.AddCoverageByTestMethod(testMethod, new CoverageByTrackedMethod() { Coverage = new int[] { -2, 2, -1, 0 }, LineVisitStatus = new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered } });

            var fileAnalysis = sut.AnalyzeFile(new CachingFileReader(new LocalFileReader(), 0, null));

            Assert.Equal(2, fileAnalysis.Lines.First().LineCoverageByTestMethod[testMethod].LineVisits);
            Assert.Equal(LineVisitStatus.Covered, fileAnalysis.Lines.First().LineCoverageByTestMethod[testMethod].LineVisitStatus);
        }

        /// <summary>
        /// A test for AnalyzeFile
        /// </summary>
        [Fact]
        public void AnalyzeFile_NonExistingFile_AnalysisIsReturned()
        {
            var sut = new CodeFile("C:\\temp\\Other.cs", new int[] { -2, -1, 0, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered });

            Assert.Null(sut.TotalLines);

            var fileAnalysis = sut.AnalyzeFile(new CachingFileReader(new LocalFileReader(), 0, null));

            Assert.NotNull(fileAnalysis);
            Assert.NotNull(fileAnalysis.Error);
            Assert.Equal(fileAnalysis.Path, fileAnalysis.Path);
            Assert.Equal(4, sut.TotalLines);
            Assert.Equal(4, fileAnalysis.Lines.Count());
        }

        /// <summary>
        /// A test for AnalyzeFile
        /// </summary>
        [Fact]
        public void AnalyzeFile_AdditionFileReaderNoError_RegularFileReaderIgnored()
        {
            var additionalFileReader = Substitute.For<IFileReader>();
            string errorArg = Arg.Any<string>();
            additionalFileReader.LoadFile(Arg.Any<string>(), out errorArg)
                .Returns(new[] { "Test" });

            var fileReader = Substitute.For<IFileReader>();

            var sut = new CodeFile("C:\\temp\\Other.cs", new int[] { -2, -1, 0, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered }, additionalFileReader);

            Assert.Null(sut.TotalLines);

            var fileAnalysis = sut.AnalyzeFile(fileReader);

            Assert.NotNull(fileAnalysis);
            Assert.Null(fileAnalysis.Error);

            additionalFileReader.Received(1).LoadFile(Arg.Any<string>(), out errorArg);
            fileReader.DidNotReceive().LoadFile(Arg.Any<string>(), out errorArg);
        }

        /// <summary>
        /// A test for AnalyzeFile
        /// </summary>
        [Fact]
        public void AnalyzeFile_AdditionFileReaderReturnsError_RegularFileReaderUsed()
        {
            var additionalFileReader = Substitute.For<IFileReader>();
            string errorArg = Arg.Any<string>();
            string errorOut = "Some error";
            additionalFileReader.LoadFile(Arg.Any<string>(), out errorArg)
                .Returns(x =>
                {
                    x[1] = errorOut;
                    return null;
                });

            var fileReader = Substitute.For<IFileReader>();
            fileReader.LoadFile(Arg.Any<string>(), out errorArg)
                .Returns(x =>
                 {
                     x[1] = errorOut;
                     return new[] { "Test" };
                 });

            var sut = new CodeFile("C:\\temp\\Other.cs", new int[] { -2, -1, 0, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered }, additionalFileReader);

            Assert.Null(sut.TotalLines);

            var fileAnalysis = sut.AnalyzeFile(fileReader);

            Assert.NotNull(fileAnalysis);
            Assert.NotNull(fileAnalysis.Error);
            Assert.Equal(fileAnalysis.Path, fileAnalysis.Path);
            Assert.Equal(4, sut.TotalLines);
            Assert.Equal(4, fileAnalysis.Lines.Count());

            additionalFileReader.Received(1).LoadFile(Arg.Any<string>(), out errorArg);
            fileReader.Received(1).LoadFile(Arg.Any<string>(), out errorArg);
        }

        /// <summary>
        /// A test for Equals
        /// </summary>
        [Fact]
        public void CodeFile_Equals()
        {
            var target1 = new CodeFile("C:\\temp\\Program.cs", System.Array.Empty<int>(), System.Array.Empty<LineVisitStatus>());
            var target2 = new CodeFile("C:\\temp\\Program.cs", System.Array.Empty<int>(), System.Array.Empty<LineVisitStatus>());
            var target3 = new CodeFile("C:\\temp\\Other.cs", System.Array.Empty<int>(), System.Array.Empty<LineVisitStatus>());

            Assert.True(target1.Equals(target2), "Objects are not equal");
            Assert.False(target1.Equals(target3), "Objects are equal");
            Assert.False(target1.Equals(null), "Objects are equal");
            Assert.False(target1.Equals(new object()), "Objects are equal");
        }

        /// <summary>
        /// A test for AddCoverageByTestMethod
        /// </summary>
        [Fact]
        public void AddCoverageByTestMethod_AddCoverageByTestMethodForExistingMethod_CoverageInformationIsMerged()
        {
            var sut = new CodeFile("C:\\temp\\Program.cs", System.Array.Empty<int>(), System.Array.Empty<LineVisitStatus>());
            var testMethod = new TestMethod("TestFull", "Test");

            var coverageByTrackedMethod = new CoverageByTrackedMethod() { Coverage = new int[] { -1, -1, -1, -1, 0, 0, 0, 1, 1, 1 }, LineVisitStatus = new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered } };
            var repeatedCoverageByTrackedMethod = new CoverageByTrackedMethod() { Coverage = new int[] { -1, 0, 1, -1, 1, 0, 1, -1, 1, 0 }, LineVisitStatus = new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.Covered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.Covered, LineVisitStatus.NotCovered } };

            sut.AddCoverageByTestMethod(testMethod, coverageByTrackedMethod);
            sut.AddCoverageByTestMethod(testMethod, repeatedCoverageByTrackedMethod);

            Assert.Contains(testMethod, sut.TestMethods);

            // using AnalyseFile() to retrieve merged coverage by test method
            var lineAnalyses = sut.AnalyzeFile(new CachingFileReader(new LocalFileReader(), 0, null)).Lines;
            var testMethodCoverage = lineAnalyses.Take(9).Select(l => l.LineCoverageByTestMethod).ToArray();

            Assert.True(testMethodCoverage.All(coverage => coverage.ContainsKey(testMethod)), "All lines should be covered by given test method");

            var actualLineVisits = testMethodCoverage.Select(c => c[testMethod].LineVisits).ToArray();
            var actualLineVisitStatuses = testMethodCoverage.Select(c => c[testMethod].LineVisitStatus).ToArray();

            Assert.Equal(new int[] { 0, 1, -1, 1, 0, 1, 1, 2, 1 }, actualLineVisits);
            Assert.Equal(new LineVisitStatus[] { LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.Covered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered }, actualLineVisitStatuses);
        }

        /// <summary>
        /// A test for the CoveredCodeElements
        /// </summary>
        [Fact]
        public void CoveredCodeElements()
        {
            var sut = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, 0, 2 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered });
            sut.AddCodeElement(new CodeElement("NotCoverable", CodeElementType.Method, 1, 1, null));
            sut.AddCodeElement(new CodeElement("NotCovered", CodeElementType.Method, 2, 2, null));
            sut.AddCodeElement(new CodeElement("Covered", CodeElementType.Method, 3, 3, null));

            Assert.Equal(1, sut.CoveredCodeElements);
        }
    }
}
