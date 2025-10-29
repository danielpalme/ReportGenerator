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
            "Test.AbstractClass",
            true)]
        [InlineData(
            "TestClass.NestedClass",
            "Test",
            "TestClass",
            "Test.TestClass",
            true)]
        [InlineData(
            "AsyncClass.<SendAsync>d__0",
            "Test",
            "AsyncClass",
            "Test.AsyncClass",
            true)]
        [InlineData(
            "GenericAsyncClass<T>",
            "Test",
            "GenericAsyncClass",
            "Test.GenericAsyncClass",
            true)]
        [InlineData(
            "ClassWithLocalFunctions.MyNestedClass<T1, T2>",
            "Test",
            "ClassWithLocalFunctions",
            "Test.ClassWithLocalFunctions",
            true)]
        [InlineData(
            "GenericAsyncClass.<MyAsyncMethod>d__1<T>",
            "Test",
            "GenericAsyncClass",
            "Test.GenericAsyncClass",
            true)]
        [InlineData(
            "Program.<CallAsyncMethod>d__1",
            "Test",
            "Program",
            "Test.Program",
            true)]
        [InlineData(
            "AutoMapperExtensions.<>c__1<TResult>",
            "Test",
            "AutoMapperExtensions",
            "Test.AutoMapperExtensions",
            true)]
        [InlineData(
            "TestClass.<GenericAsyncMethod>d__4<T>",
            "Test",
            "TestClass",
            "Test.TestClass",
            true)]
        public void ParseClassName(
            string rawName,
            string @namespace,
            string expectedName,
            string expectedFullName,
            bool expectedInclude)
        {
            var result = DynamicCodeCoverageClassNameParser.ParseClassName(rawName, @namespace);

            Assert.Equal(expectedName, result.Name);
            Assert.Equal(expectedFullName, result.FullName);
            Assert.Equal(expectedInclude, result.Include);
        }
    }
}
