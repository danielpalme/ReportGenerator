using System;
using System.Collections.Generic;

namespace Test
{
    public class AnalyzerTestClass
    {
        public AnalyzerTestClass()
        {
            Console.WriteLine(".ctor");
        }

        public string DoSomething(
            string value,
            string[] stringArray,
            Guid id,
            IEnumerable<string> stringEnumerable,
            IList<string> stringList,
            decimal dec,
            int i,
            long l,
            Dictionary<string, int> dict,
            out int g,
            float fff,
            double dou,
            bool bo,
            byte by,
            char ch,
            object o,
            sbyte sby,
            short sh,
            uint ui,
            ulong ul,
            ushort usho)
        {
            g = 0;
            return null;
        }

        public void GenericMethod<T1, T2>(T1 t1, T2 t2, int i)
        {
        }

        public string AutoProperty { get; set; }
    }
}
