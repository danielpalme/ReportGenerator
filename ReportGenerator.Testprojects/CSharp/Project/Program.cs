using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test
{
    [TestClass]
    public class Program
    {
        static void Main(string[] args)
        {
            new TestClass().SampleFunction();

            new TestClass2("Test").ExecutedMethod();
            new TestClass2("Test").SampleFunction("Munich");

            new PartialClass().ExecutedMethod_1();
            new PartialClass().ExecutedMethod_2();
            new PartialClass().SomeProperty = -10;

            new PartialClassWithAutoProperties().Property1 = "Test";
            new PartialClassWithAutoProperties().Property2 = "Test";

            new SomeClass().Property1 = "Test";

            new ClassWithExcludes().IncludedMethod();
            new ClassWithExcludes().ExcludedMethod();

            new GenericClass<SomeModel, IState>().Process(null);
            new GenericClass<SomeModel, IState>().PostProcess(null);

            new CodeContract_Target().Calculate(-1);

            new AbstractClass_SampleImpl1();
            new AbstractClass_SampleImpl2();

            try
            {
                new CodeContract_Target().Calculate(0);
            }
            catch (System.ArgumentException)
            {
            }
        }

        [TestMethod]
        public void CSharp_ExecuteTest1()
        {
            Main(null);
        }

        [TestMethod]
        public void CSharp_ExecuteTest2()
        {
            Main(null);
        }
    }
}
