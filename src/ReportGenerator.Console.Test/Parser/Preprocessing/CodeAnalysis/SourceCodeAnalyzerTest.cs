using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser.Preprocessing.CodeAnalysis;

namespace Palmmedia.ReportGeneratorTest.Parser.Preprocessing.CodeAnalysis
{
    /// <summary>
    /// This is a test class for SourceCodeAnalyzer and is intended
    /// to contain all SourceCodeAnalyzer Unit Tests
    /// </summary>
    [TestClass]
    public class SourceCodeAnalyzerTest
    {
        private static string elementClassFile = Path.Combine(FileManager.GetCSharpCodeDirectory(), "AnalyzerTestClass.cs");

        /// <summary>
        /// A test for GetClassesInFile
        /// </summary>
        [TestMethod]
        public void GetClassesInFile_AllClassesAreReturned()
        {
            var classes = SourceCodeAnalyzer.GetClassesInFile(Path.Combine(FileManager.GetCSharpCodeDirectory(), "TestClass.cs"));

            Assert.IsNotNull(classes, "Classes must not be null.");
            Assert.IsTrue(classes.Contains("Test.TestClass"), "Classes does not contain root class");
            Assert.IsTrue(classes.Contains("Test.TestClassNestedClass"), "Classes does not contain nested class");
        }

        /// <summary>
        /// A test for FindMethod
        /// </summary>
        [TestMethod]
        public void FindMethod_SearchExistingMethod_PositionMustNotBeNullAndSupplyCorrectLinenumber()
        {
            PartCoverMethodElement partCoverMethodElement = new PartCoverMethodElement(
                "Test.AnalyzerTestClass",
                "DoSomething",
                "string  (string, string[], System.Guid, string, string, System.Decimal, int, long, stringint, ref int, float, double, bool, unsigned byte, char, object, byte, short, unsigned int, unsigned long, unsigned short, ICSharpCode.NRefactory.Ast.INode)");

            var methodPosition = SourceCodeAnalyzer.FindSourceElement(elementClassFile, partCoverMethodElement);

            Assert.IsNotNull(methodPosition, "MethodPosition must not be null.");

            Assert.AreEqual(37, methodPosition.Start, "Start line number does not match.");
            Assert.AreEqual(40, methodPosition.End, "End line number does not match.");
        }

        /// <summary>
        /// A test for FindMethod
        /// </summary>
        [TestMethod]
        public void FindMethod_SearchExistingConstructor_PositionMustNotBeNullAndSupplyCorrectLinenumber()
        {
            PartCoverMethodElement partCoverMethodElement = new PartCoverMethodElement(
                "Test.AnalyzerTestClass",
                ".ctor",
                "void  ()");

            var methodPosition = SourceCodeAnalyzer.FindSourceElement(elementClassFile, partCoverMethodElement);

            Assert.IsNotNull(methodPosition, "MethodPosition must not be null.");

            Assert.AreEqual(10, methodPosition.Start, "Start line number does not match.");
            Assert.AreEqual(12, methodPosition.End, "End line number does not match.");
        }

        /// <summary>
        /// A test for FindMethod
        /// </summary>
        [TestMethod]
        public void FindMethod_SearchNonExistingGenericMethod_PositionIsNull()
        {
            PartCoverMethodElement partCoverMethodElement = new PartCoverMethodElement(
                "TestNamespace.AnalyzerTestClass",
                "GenericMethod",
                "void  (int)");

            var methodPosition = SourceCodeAnalyzer.FindSourceElement(elementClassFile, partCoverMethodElement);

            Assert.IsNull(methodPosition, "MethodPosition is not null.");
        }

        /// <summary>
        /// A test for FindProperty
        /// </summary>
        [TestMethod]
        public void FindProperty_SearchExistingProperty_PositionMustNotBeNullAndSupplyCorrectLinenumber()
        {
            PropertyElement propertyElement = new PropertyElement("Test.AnalyzerTestClass", "get_AutoProperty");

            var propertyPosition = SourceCodeAnalyzer.FindSourceElement(elementClassFile, propertyElement);

            Assert.IsNotNull(propertyPosition, "PropertyPosition must not be null.");

            Assert.AreEqual(46, propertyPosition.Start, "Start line number does not match.");
            Assert.AreEqual(46, propertyPosition.End, "End line number does not match.");
        }

        /// <summary>
        /// A test for FindProperty
        /// </summary>
        [TestMethod]
        public void FindProperty_SearchNonExistingProperty_PositionIsNull()
        {
            PropertyElement propertyElement = new PropertyElement("Test.AnalyzerTestClass", "get_DoesNotExist");

            var propertyPosition = SourceCodeAnalyzer.FindSourceElement(elementClassFile, propertyElement);

            Assert.IsNull(propertyPosition, "PropertyPosition is not null.");
        }
    }
}
