package test;
public class AbstractClass_SampleImpl2 extends AbstractClass {
    public AbstractClass_SampleImpl2() {
    	System.out.println("SampleImpl2 constructed");
    }

    public void Method1() throws Exception {
        throw new Exception();
    }

    public void Method2() throws Exception {
        throw new Exception();
    }
}