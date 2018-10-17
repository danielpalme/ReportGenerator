using System;

namespace Test
{
    public abstract class AbstractClass
    {
        public AbstractClass()
        {
            Console.WriteLine("AbstractClass constructed");
        }

        public abstract void Method1();

        public abstract void Method2();
    }

    public class AbstractClass_SampleImpl1 : AbstractClass
    {
        public AbstractClass_SampleImpl1()
            : base()
        {
            Console.WriteLine("SampleImpl1 constructed");
        }

        public override void Method1()
        {
            throw new NotImplementedException();
        }

        public override void Method2()
        {
            throw new NotImplementedException();
        }
    }

    public class AbstractClass_SampleImpl2 : AbstractClass
    {
        public AbstractClass_SampleImpl2()
            : base()
        {
            Console.WriteLine("SampleImpl2 constructed");
        }

        public override void Method1()
        {
            throw new NotImplementedException();
        }

        public override void Method2()
        {
            throw new NotImplementedException();
        }
    }
}