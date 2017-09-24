using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser.Analysis;

namespace Palmmedia.ReportGeneratorTest.Parser.Analysis
{
    /// <summary>
    /// This is a test class for CodeFile and is intended
    /// to contain all CodeFile Unit Tests
    /// </summary>
    [TestClass]
    public class CodeFileTest
    {
        /// <summary>
        /// A test for the Constructor
        /// </summary>
        [TestMethod]
        public void Constructor()
        {
            var sut = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, 0, 2 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered });

            Assert.AreEqual(2, sut.CoverableLines, "Not equal");
            Assert.AreEqual(1, sut.CoveredLines, "Not equal");

            Assert.IsNull(sut.CoveredBranches, "Not null");
            Assert.IsNull(sut.TotalBranches, "Not null");
        }

        /// <summary>
        /// A test for the Constructor
        /// </summary>
        [TestMethod]
        public void Constructor_WithBranches()
        {
            var branches = new Dictionary<int, ICollection<Branch>>()
            {
                { 1, new List<Branch>() { new Branch(1, "1"), new Branch(0, "2") } },
                { 2, new List<Branch>() { new Branch(0, "3"), new Branch(2, "4") } }
            };

            var sut = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered }, branches);

            Assert.AreEqual(2, sut.CoveredBranches, "Not equal");
            Assert.AreEqual(4, sut.TotalBranches, "Not equal");
        }

        /// <summary>
        /// A test for Merge
        /// </summary>
        [TestMethod]
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

            Assert.AreEqual(2, sut.CoveredBranches, "Not equal");
            Assert.AreEqual(4, sut.TotalBranches, "Not equal");
        }

        /// <summary>
        /// A test for Merge
        /// </summary>
        [TestMethod]
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

            Assert.AreEqual(2, sut.CoveredBranches, "Not equal");
            Assert.AreEqual(4, sut.TotalBranches, "Not equal");
        }

        /// <summary>
        /// A test for Merge
        /// </summary>
        [TestMethod]
        public void Merge_MergeCodeFileWithEqualLengthCoverageArray_CoverageInformationIsUpdated()
        {
            var sut = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered });
            var codeFileToMerge = new CodeFile("C:\\temp\\Program.cs", new int[] { -1, 0, 1, -1, 0, 1, -1, 0, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered });
            var testMethod = new TestMethod("TestFull", "Test");
            codeFileToMerge.AddCoverageByTestMethod(testMethod, new CoverageByTrackedMethod() { Coverage = new int[] { -1, -1, -1, 0, 0, 0, 1, 1, 1 }, LineVisitStatus = new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered } });

            sut.Merge(codeFileToMerge);

            Assert.AreEqual(8, sut.CoverableLines, "Not equal");
            Assert.AreEqual(5, sut.CoveredLines, "Not equal");

            Assert.IsNull(sut.CoveredBranches, "Not null");
            Assert.IsNull(sut.TotalBranches, "Not null");

            Assert.IsTrue(sut.TestMethods.Contains(testMethod));
        }

        /// <summary>
        /// A test for Merge
        /// </summary>
        [TestMethod]
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

            Assert.AreEqual(10, sut.CoverableLines, "Not equal");
            Assert.AreEqual(6, sut.CoveredLines, "Not equal");

            Assert.AreEqual(4, sut.CoveredBranches, "Not equal");
            Assert.AreEqual(7, sut.TotalBranches, "Not equal");
        }

        /// <summary>
        /// A test for AnalyzeFile
        /// </summary>
        [TestMethod]
        public void AnalyzeFile_ExistingFile_AnalysisIsReturned()
        {
            var sut = new CodeFile("C:\\temp\\Program.cs", new int[] { -2, -1, 0, 1, 2 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.PartiallyCovered, LineVisitStatus.Covered });

            Assert.IsNull(sut.TotalLines);

            var fileAnalysis = sut.AnalyzeFile();

            Assert.IsNotNull(fileAnalysis);
            Assert.IsNull(fileAnalysis.Error);
            Assert.AreEqual(fileAnalysis.Path, fileAnalysis.Path);
            Assert.AreEqual(84, sut.TotalLines);
            Assert.AreEqual(84, fileAnalysis.Lines.Count());

            Assert.AreEqual(1, fileAnalysis.Lines.ElementAt(0).LineNumber);
            Assert.AreEqual(-1, fileAnalysis.Lines.ElementAt(0).LineVisits);
            Assert.AreEqual(LineVisitStatus.NotCoverable, fileAnalysis.Lines.ElementAt(0).LineVisitStatus);

            Assert.AreEqual(2, fileAnalysis.Lines.ElementAt(1).LineNumber);
            Assert.AreEqual(0, fileAnalysis.Lines.ElementAt(1).LineVisits);
            Assert.AreEqual(LineVisitStatus.NotCovered, fileAnalysis.Lines.ElementAt(1).LineVisitStatus);

            Assert.AreEqual(3, fileAnalysis.Lines.ElementAt(2).LineNumber);
            Assert.AreEqual(1, fileAnalysis.Lines.ElementAt(2).LineVisits);
            Assert.AreEqual(LineVisitStatus.PartiallyCovered, fileAnalysis.Lines.ElementAt(2).LineVisitStatus);

            Assert.AreEqual(4, fileAnalysis.Lines.ElementAt(3).LineNumber);
            Assert.AreEqual(2, fileAnalysis.Lines.ElementAt(3).LineVisits);
            Assert.AreEqual(LineVisitStatus.Covered, fileAnalysis.Lines.ElementAt(3).LineVisitStatus);
        }

        /// <summary>
        /// A test for AnalyzeFile
        /// </summary>
        [TestMethod]
        public void AnalyzeFile_ExistingFileWithTrackedMethods_AnalysisIsReturned()
        {
            var sut = new CodeFile("C:\\temp\\Program.cs", new int[] { -2, -1, 0, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered });
            var testMethod = new TestMethod("TestFull", "Test");
            sut.AddCoverageByTestMethod(testMethod, new CoverageByTrackedMethod() { Coverage = new int[] { -2, 2, -1, 0 }, LineVisitStatus = new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered } });

            var fileAnalysis = sut.AnalyzeFile();

            Assert.AreEqual(2, fileAnalysis.Lines.First().LineCoverageByTestMethod[testMethod].LineVisits);
            Assert.AreEqual(LineVisitStatus.Covered, fileAnalysis.Lines.First().LineCoverageByTestMethod[testMethod].LineVisitStatus);
        }

        /// <summary>
        /// A test for AnalyzeFile
        /// </summary>
        [TestMethod]
        public void AnalyzeFile_NonExistingFile_AnalysisIsReturned()
        {
            var sut = new CodeFile("C:\\temp\\Other.cs", new int[] { -2, -1, 0, 1 }, new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered });

            Assert.IsNull(sut.TotalLines);

            var fileAnalysis = sut.AnalyzeFile();

            Assert.IsNotNull(fileAnalysis);
            Assert.IsNotNull(fileAnalysis.Error);
            Assert.AreEqual(fileAnalysis.Path, fileAnalysis.Path);
            Assert.IsNull(sut.TotalLines);
            Assert.AreEqual(0, fileAnalysis.Lines.Count());
        }

        /// <summary>
        /// A test for Equals
        /// </summary>
        [TestMethod]
        public void Equals()
        {
            var target1 = new CodeFile("C:\\temp\\Program.cs", new int[0], new LineVisitStatus[0]);
            var target2 = new CodeFile("C:\\temp\\Program.cs", new int[0], new LineVisitStatus[0]);
            var target3 = new CodeFile("C:\\temp\\Other.cs", new int[0], new LineVisitStatus[0]);

            Assert.IsTrue(target1.Equals(target2), "Objects are not equal");
            Assert.IsFalse(target1.Equals(target3), "Objects are equal");
            Assert.IsFalse(target1.Equals(null), "Objects are equal");
            Assert.IsFalse(target1.Equals(new object()), "Objects are equal");
        }

        /// <summary>
        /// A test for AddCoverageByTestMethod
        /// </summary>
        [TestMethod]
        public void AddCoverageByTestMethod_AddCoverageByTestMethodForExistingMethod_CoverageInformationIsMerged()
        {
            var sut = new CodeFile("C:\\temp\\Program.cs", new int[0], new LineVisitStatus[0]);
            var testMethod = new TestMethod("TestFull", "Test");

            var coverageByTrackedMethod = new CoverageByTrackedMethod() { Coverage = new int[] { -1, -1, -1, -1, 0, 0, 0,  1, 1, 1 }, LineVisitStatus = new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered } };
            var repeatedCoverageByTrackedMethod = new CoverageByTrackedMethod() { Coverage = new int[] { -1,  0,  1, -1, 1, 0, 1, -1, 1, 0 }, LineVisitStatus = new LineVisitStatus[] { LineVisitStatus.NotCoverable, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.Covered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.Covered, LineVisitStatus.NotCovered } };

            sut.AddCoverageByTestMethod(testMethod, coverageByTrackedMethod);
            sut.AddCoverageByTestMethod(testMethod, repeatedCoverageByTrackedMethod);

            Assert.IsTrue(sut.TestMethods.Contains(testMethod));

            // using AnalyseFile() to retrieve merged coverage by test method
            var lineAnalyses = sut.AnalyzeFile().Lines;
            var testMethodCoverage = lineAnalyses.Take(9).Select(l => l.LineCoverageByTestMethod).ToArray();

            Assert.IsTrue(testMethodCoverage.All(coverage => coverage.ContainsKey(testMethod)), "All lines should be covered by given test method");

            var actualLineVisits = testMethodCoverage.Select(c => c[testMethod].LineVisits).ToArray();
            var actualLineVisitStatuses = testMethodCoverage.Select(c => c[testMethod].LineVisitStatus).ToArray();

            CollectionAssert.AreEqual(new int[] { 0, 1, -1, 1, 0, 1, 1, 2, 1 }, actualLineVisits, "LineVisits does not match");
            CollectionAssert.AreEqual(new LineVisitStatus[] { LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.NotCoverable, LineVisitStatus.Covered, LineVisitStatus.NotCovered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered, LineVisitStatus.Covered }, actualLineVisitStatuses, "LineVisitStatus does not match");
        }
    }
}
