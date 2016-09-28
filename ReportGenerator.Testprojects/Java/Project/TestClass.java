class TestClass
{
    public void SampleFunction() {
        String test = String.format(
            "%s %s",
             "Hello",
             "World");

        System.out.println(test);
        int i = 10;

        if (i > 0 || i > 1) {
        	System.out.println(i + " is greater that 0");
        }
        else {
        	System.out.println(i + " is not greater that 0");
        }
    }

    public class NestedClass {
        public void SampleFunction() {
        	System.out.printf(
                "%s %s",
                 "Hello",
                 "World");
        }
    }
}