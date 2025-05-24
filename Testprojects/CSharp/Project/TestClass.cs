using System;

namespace Test
{
    class TestClass
    {
        public void SampleFunction()
        {
            string test = string.Format(
                "{0} {1}",
                 "Hello",
                 "World");

            Console.WriteLine(test);
            int i = 10;

            if (i > 0 || i > 1)
            {
                Console.WriteLine(i + " is greater that 0");
            }
            else
            {
                Console.WriteLine(i + " is not greater that 0");
            }
        }

        public class NestedClass
        {
            public void SampleFunction()
            {
                Console.WriteLine(
                    "{0} {1}",
                     "Hello",
                     "World");
            }
        }
    }
}