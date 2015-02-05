using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palmmedia.ReportGenerator.Parser.Preprocessing.FileSearch;

namespace Palmmedia.ReportGeneratorTest.Parser.Preprocessing.FileSearch
{
    /// <summary>
    /// This is a test class for ClassSearcherFactory and is intended
    /// to contain all ClassSearcherFactory Unit Tests
    /// </summary>
    [TestClass]
    public class ClassSearcherFactoryTest
    {
        /// <summary>
        /// A test for CreateClassSearcher
        /// </summary>
        [TestMethod]
        public void CreateClassSearcher_PassNull_ClassSearcherWithNullDirectoryIsReturned()
        {
            var sut = new ClassSearcherFactory();

            var classSearcher = sut.CreateClassSearcher((string)null);

            Assert.IsNotNull(classSearcher, "ClassSearcher must not be null");
            Assert.IsNull(classSearcher.Directory, "ClassSearcher directory must be null");
        }

        /// <summary>
        /// A test for CreateClassSearcher
        /// </summary>
        [TestMethod]
        public void CreateClassSearcher_PassSubdirectory_CachedInstanceIsReturned()
        {
            var sut = new ClassSearcherFactory();

            var classSearcher1 = sut.CreateClassSearcher("C:\\temp");
            var classSearcher2 = sut.CreateClassSearcher("C:\\temp\\sub");

            Assert.AreSame(classSearcher1, classSearcher2, "ClassSearchers are not the same instance.");
        }

        /// <summary>
        /// A test for CreateClassSearcher
        /// </summary>
        [TestMethod]
        public void CreateClassSearcher_PassParentDirectory_NewInstanceIsReturned()
        {
            var sut = new ClassSearcherFactory();

            var classSearcher1 = sut.CreateClassSearcher("C:\\temp");
            var classSearcher2 = sut.CreateClassSearcher("C:\\");

            Assert.AreNotSame(classSearcher1, classSearcher2, "ClassSearchers are the same instance.");
        }
    }
}
