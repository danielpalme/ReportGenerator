using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser.Analysis
{
    /// <summary>
    /// This is a test class for Assembly and is intended
    /// to contain all Assembly Unit Tests
    /// </summary>
    public class AssemblyTest
    {
        /// <summary>
        /// A test for the Constructor
        /// </summary>
        [Fact]
        public void Constructor()
        {
            string assemblyName = "C:\\test\\TestAssembly.dll";

            var sut = new Assembly(assemblyName);

            Assert.Equal(assemblyName, sut.Name);
            Assert.Equal("TestAssembly.dll", sut.ShortName);
        }

        /// <summary>
        /// A test for AddClass
        /// </summary>
        [Fact]
        public void AddClass_AddSingleClass_ClassIsStored()
        {
            var sut = new Assembly("C:\\test\\TestAssembly.dll");
            var @class = new Class("Test", sut);

            sut.AddClass(@class);

            Assert.Equal(@class, sut.Classes.First());
            Assert.Single(sut.Classes);
        }

        /// <summary>
        /// A test for Merge
        /// </summary>
        [Fact]
        public void Merge_MergeAssemblyWithOneClass_ClassIsStored()
        {
            var sut = new Assembly("C:\\test\\TestAssembly.dll");
            var assemblyToMerge = new Assembly("C:\\test\\TestAssembly.dll");
            var @class = new Class("Test", assemblyToMerge);
            assemblyToMerge.AddClass(@class);

            sut.Merge(assemblyToMerge);

            Assert.Equal(@class, sut.Classes.First());
            Assert.Single(sut.Classes);
            Assert.Same(sut.Classes.First().Assembly, sut);
        }

        /// <summary>
        /// A test for Equals
        /// </summary>
        [Fact]
        public void Assembley_Equals()
        {
            string assemblyName = "C:\\test\\TestAssembly.dll";

            var target1 = new Assembly(assemblyName);
            var target2 = new Assembly(assemblyName);
            var target3 = new Assembly("Test.dll");

            Assert.True(target1.Equals(target2), "Objects are not equal");
            Assert.False(target1.Equals(target3), "Objects are equal");
            Assert.False(target1.Equals(null), "Objects are equal");
            Assert.False(target1.Equals(new object()), "Objects are equal");
        }
    }
}
