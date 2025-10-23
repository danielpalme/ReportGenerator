using Palmmedia.ReportGenerator.Core.Parser;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser
{
    public class DynamicCodeCoverageClassNameParserTest
    {
        [Theory]
        [InlineData(
            "AbstractClass",
            "Test",
            "AbstractClass",
            "AbstractClass",
            null,
            "Test.AbstractClass",
            true)]
        [InlineData(
            "TestClass.NestedClass",
            "Test",
            "TestClass",
            "TestClass",
            null,
            "Test.TestClass",
            true)]
        [InlineData(
            "AsyncClass.<SendAsync>d__0",
            "Test",
            "AsyncClass",
            "AsyncClass",
            null,
            "Test.AsyncClass",
            true)]
        [InlineData(
            "GenericAsyncClass<T>",
            "Test",
            "GenericAsyncClass",
            "GenericAsyncClass<T>",
            "<T>",
            "Test.GenericAsyncClass<T>",
            true)]
        [InlineData(
            "ClassWithLocalFunctions.MyNestedClass<T1, T2>",
            "Test",
            "ClassWithLocalFunctions",
            "ClassWithLocalFunctions",
            null,
            "Test.ClassWithLocalFunctions",
            true)]
        [InlineData(
            "GenericAsyncClass.<MyAsyncMethod>d__1<T>",
            "Test",
            "GenericAsyncClass",
            "GenericAsyncClass<T>",
            "<T>",
            "Test.GenericAsyncClass<T>",
            true)]
        [InlineData(
            "Program.<CallAsyncMethod>d__1",
            "Test",
            "Program",
            "Program",
            null,
            "Test.Program",
            true)]
        public void ParseClassName(
            string rawName,
            string @namespace,
            string expectedName,
            string expectedDisplayName,
            string expectedGenericType,
            string expectedFullName,
            bool expectedInclude)
        {
            var result = DynamicCodeCoverageClassNameParser.ParseClassName(rawName, @namespace);

            Assert.Equal(expectedName, result.Name);
            Assert.Equal(expectedDisplayName, result.DisplayName);
            Assert.Equal(expectedGenericType, result.GenericType);
            Assert.Equal(expectedFullName, result.FullName);
            Assert.Equal(expectedInclude, result.Include);
        }
    }
}
