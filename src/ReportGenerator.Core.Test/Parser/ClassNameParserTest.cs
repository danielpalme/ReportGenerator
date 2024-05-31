using Palmmedia.ReportGenerator.Core.Parser;
using Xunit;

namespace Palmmedia.ReportGenerator.Core.Test.Parser
{
    public class ClassNameParserTest
    {
        [Theory]
        // RawMode
        [InlineData(
            "Test.ClassWithLocalFunctions`1/MyNestedClass`1/&lt;&gt;c__DisplayClass4_0`1/&lt;&lt;MyAsyncMethod&gt;g__MyAsyncLocalFunction|0&gt;d`1",
            true,
            "Test.ClassWithLocalFunctions`1/MyNestedClass`1/&lt;&gt;c__DisplayClass4_0`1/&lt;&lt;MyAsyncMethod&gt;g__MyAsyncLocalFunction|0&gt;d`1",
            "Test.ClassWithLocalFunctions`1/MyNestedClass`1/&lt;&gt;c__DisplayClass4_0`1/&lt;&lt;MyAsyncMethod&gt;g__MyAsyncLocalFunction|0&gt;d`1",
            true)]

        // Coverlet
        [InlineData(
            "Test.AsyncClass/<SendAsync>d__0",
            false,
            "Test.AsyncClass",
            "Test.AsyncClass",
            true)]
        [InlineData(
            "Test.ClassWithLocalFunctions`1/MyNestedClass`1",
            false,
            "Test.ClassWithLocalFunctions`1",
            "Test.ClassWithLocalFunctions`1",
            true)]
        [InlineData(
            "Test.ClassWithLocalFunctions`1/MyNestedClass`1/<>c__DisplayClass4_0`1/<<MyAsyncMethod>g__MyAsyncLocalFunction|0>d`1",
            false,
            "Test.ClassWithLocalFunctions`1",
            "Test.ClassWithLocalFunctions`1",
            true)]
        [InlineData(
            "Test.ClassWithLocalFunctions`1/MyNestedClass`1/<MyAsyncMethod>d__4`1",
            false,
            "Test.ClassWithLocalFunctions`1",
            "Test.ClassWithLocalFunctions`1",
            true)]
        [InlineData(
            "Test.GenericAsyncClass`1/<MyAsyncMethod>d__0",
            false,
            "Test.GenericAsyncClass`1",
            "Test.GenericAsyncClass`1",
            true)]
        [InlineData(
            "Test.GenericClass`2",
            false,
            "Test.GenericClass`2",
            "Test.GenericClass`2",
            true)]
        [InlineData(
            "Test.Program/EchoHandler",
            false,
            "Test.Program",
            "Test.Program",
            true)]
        [InlineData(
            "Test.Program/<CallAsyncMethod>d__1",
            false,
            "Test.Program",
            "Test.Program",
            true)]
        [InlineData(
            "Test.TestClass/NestedClass",
            false,
            "Test.TestClass",
            "Test.TestClass",
            true)]

        // DotNet coverage
        [InlineData(
            "Test.AsyncClass.<SendAsync>d__0",
            false,
            "Test.AsyncClass",
            "Test.AsyncClass",
            true)]
        [InlineData(
            "Test.GenericAsyncClass.<MyAsyncMethod>d__0<T>",
            false,
            "Test.GenericAsyncClass",
            "Test.GenericAsyncClass<T>",
            true)]
        [InlineData(
            "Test.Program.<CallAsyncMethod>d__1",
            false,
            "Test.Program",
            "Test.Program",
            true)]
        [InlineData(
            "Test.TestClass.<>c",
            false,
            "Test.TestClass",
            "Test.TestClass",
            true)]
        [InlineData(
            "Test.TestClass2.<>c__DisplayClass14_0",
            false,
            "Test.TestClass2",
            "Test.TestClass2",
            true)]
        [InlineData(
            "Test.ClassWithLocalFunctions.MyNestedClass.<MyAsyncMethod>d__4<T1, T2, T3>",
            false,
            "Test.ClassWithLocalFunctions.MyNestedClass",
            "Test.ClassWithLocalFunctions.MyNestedClass<T1, T2, T3>",
            true)]
        [InlineData(
            "Test.ClassWithLocalFunctions.MyNestedClass.<>c__DisplayClass4_0.<<MyAsyncMethod>g__MyAsyncLocalFunction|0>d<T1, T2, T3, T4>",
            false,
            "Test.ClassWithLocalFunctions.MyNestedClass",
            "Test.ClassWithLocalFunctions.MyNestedClass<T1, T2, T3, T4>",
            true)]
        [InlineData(
            "Test.AbstractGenericClass<TModel, TState>",
            false,
            "Test.AbstractGenericClass<TModel, TState>",
            "Test.AbstractGenericClass<TModel, TState>",
            true)]
        [InlineData(
            "Test.AnalyzerTestClass",
            false,
            "Test.AnalyzerTestClass",
            "Test.AnalyzerTestClass",
            true)]
        [InlineData(
            "Test.ClassWithExcludes",
            false,
            "Test.ClassWithExcludes",
            "Test.ClassWithExcludes",
            true)]
        [InlineData(
            "Test.ClassWithLocalFunctions.MyNestedClass<T1, T2>",
            false,
            "Test.ClassWithLocalFunctions.MyNestedClass<T1, T2>",
            "Test.ClassWithLocalFunctions.MyNestedClass<T1, T2>",
            true)]
        [InlineData(
            "Test.GenericClass<TModel, TState>",
            false,
            "Test.GenericClass<TModel, TState>",
            "Test.GenericClass<TModel, TState>",
            true)]
        [InlineData(
            "Test.Program",
            false,
            "Test.Program",
            "Test.Program",
            true)]
        [InlineData(
            "Test.Program.EchoHandler",
            false,
            "Test.Program.EchoHandler",
            "Test.Program.EchoHandler",
            true)]
        [InlineData(
            "Test.TestClass",
            false,
            "Test.TestClass",
            "Test.TestClass",
            true)]
        [InlineData(
            "Test.TestClass.NestedClass",
            false,
            "Test.TestClass.NestedClass",
            "Test.TestClass.NestedClass",
            true)]
        [InlineData(
            "Test.TestClass2",
            false,
            "Test.TestClass2",
            "Test.TestClass2",
            true)]
        public void ParseClassName(string rawName, bool rawMode, string expectedName,
            string expectedDisplayName, bool expectedInclude)
        {
            var result = ClassNameParser.ParseClassName(rawName, rawMode);

            Assert.Equal(expectedName, result.Name);
            Assert.Equal(expectedDisplayName, result.DisplayName);
            Assert.Equal(rawName, result.RawName);
            Assert.Equal(expectedInclude, result.Include);
        }
    }
}
