using System;

namespace Test
{
    partial class PartialClass
    {
        public void ExecutedMethod_1()
        {
            Console.WriteLine("Test");
        }

        public void UnExecutedMethod_1()
        {
            Console.WriteLine("Test");
        }

        private int someProperty;

        public int SomeProperty
        {
            get { return this.someProperty; }

            set
            {
                if (value < 0)
                {
                    this.someProperty = 0;
                }
                else
                {
                    this.someProperty = value;
                }
            }
        }
    }
}
