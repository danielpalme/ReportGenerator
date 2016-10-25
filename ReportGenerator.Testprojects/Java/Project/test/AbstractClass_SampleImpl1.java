package test;
public class AbstractClass_SampleImpl1 extends AbstractClass {
    public AbstractClass_SampleImpl1() {
    	System.out.println("SampleImpl1 constructed");
    }

    public void Method1() throws Exception {
        throw new Exception();
    }

    public void Method2() throws Exception {
        throw new Exception();
    }
}