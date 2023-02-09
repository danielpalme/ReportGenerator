using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser.Analysis
{
    /// <summary>
    /// This is a test class for ClassTest and is intended
    /// to contain all ClassTest Unit Tests
    /// </summary>
    public class ClassTest
    {
        /// <summary>
        /// A test for the Constructor
        /// </summary>
        [Theory]
        [InlineData("TestClass", "TestClass")]
        [InlineData("TestClass`1", "TestClass<T>")]
        [InlineData("TestClass`2", "TestClass<T1, T2>")]
        [InlineData("TestClass`3", "TestClass<T1, T2, T3>")]
        public void Constructor(string classname, string expectedClassDisplayName)
        {
            Assembly assembly = new Assembly("C:\\test\\TestAssembly.dll");

            var sut = new Class(classname, assembly);

            Assert.Equal(assembly, sut.Assembly);
            Assert.Equal(classname, sut.Name);
            Assert.Equal(expectedClassDisplayName, sut.DisplayName);
        }

        /// <summary>
        /// A test for AddFile
        /// </summary>
        [Fact]
        public void AddFile_AddSingleFile_FileIsStored()
        {
            var assembly = new Assembly("C:\\test\\TestAssembly.dll");
            var sut = new Class("Test", assembly);
            var file = new CodeFile("C:\\temp\\Program.cs", System.Array.Empty<int>(), System.Array.Empty<LineVisitStatus>());
            sut.AddFile(file);

            Assert.Equal(file, sut.Files.First());
            Assert.Single(sut.Files);
        }

        /// <summary>
        /// A test for Equals
        /// </summary>
        [Fact]
        public void Class_Equals()
        {
            Assembly assembly = new Assembly("C:\\test\\TestAssembly.dll");
            string classname = "TestClass";

            var target1 = new Class(classname, assembly);
            var target2 = new Class(classname, assembly);
            var target3 = new Class(classname + "123", assembly);

            Assert.True(target1.Equals(target2), "Objects are not equal");
            Assert.False(target1.Equals(target3), "Objects are equal");
            Assert.False(target1.Equals(null), "Objects are equal");
            Assert.False(target1.Equals(new object()), "Objects are equal");
        }
    }
}
