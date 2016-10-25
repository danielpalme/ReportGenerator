package test;
public class Program {
    public static void main(String[] args)
    {
        new TestClass().SampleFunction();

        new GenericClass<String, String>().Process(null, null);

        new AbstractClass_SampleImpl1();
        new AbstractClass_SampleImpl2();
    }
}
