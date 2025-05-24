using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    class TestClass2
    {
        private string name;

        private Dictionary<string, int> dict = new Dictionary<string, int>();

        public string ExecutedProperty { get; set; }

        public string UnExecutedProperty { get; set; }

        public TestClass2()
        {
            this.name = "Nobody";
            this.ExecutedProperty = "Nobody";
        }

        public TestClass2(string name)
        {
            this.name = name;
            this.ExecutedProperty = name + name;
        }

        public void ExecutedMethod()
        {
            Console.WriteLine(this.name);
            Console.WriteLine(this.ExecutedProperty);
        }

        public void UnExecutedMethod()
        {
            Console.WriteLine(this.name);
            Console.WriteLine(this.ExecutedProperty);
        }

        public void SampleFunction(string city)
        {
            int[] values = new int[] { 0, 1, 2, 3 };

            var doubled = values.Select(i => i * 2);

            foreach (var item in doubled)
            {
                Console.WriteLine(item);
            }

            string[] cities = new string[] { "Berlin", "Munich", "Paris" };

            if (cities.SingleOrDefault(c => c.Equals(city, StringComparison.OrdinalIgnoreCase)) != null)
            {
                Console.WriteLine("Found " + city);
            }
        }

        public string DoSomething(string value,
            string[] stringArray,
            Guid id,
            IEnumerable<string> stringEnumerable,
            IList<string> stringList,
            decimal dec,
            int i,
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
    }
}
