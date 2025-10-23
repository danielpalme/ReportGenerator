using System;
using System.Threading.Tasks;

namespace Test
{
    public class GenericAsyncClass<T>
    {
        public void DoWork()
        {
            Console.WriteLine("BaseService DoWork");
        }

        public async Task MyAsyncMethod()
        {
        }
        public async Task<T> MyAsyncMethod(T something)
        {
            return something;
        }
    }
}
