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

        public void MyMethod()
        {
        }

        public async Task MyAsyncMethod()
        {
        }

        public async Task<T> MyAsyncGenericMethod<T1>(T1 something)
        {
            return default;
        }

        public async Task<T> MyAsyncMethod(T something)
        {
            return something;
        }

        public class InnerGenericClass<U>
        {
            public void MyNestedMethod()
            {
            }

            public async Task MyNestedAsyncMethod()
            {
            }

            public async Task<T> MyNestedAsyncGenericMethod<U1>(U1 something)
            {
                return default;
            }

            public async Task<U> MyNestedAsyncMethod(U something)
            {
                return something;
            }
        }

        public class InnerNonGenericClass
        {
            public void MyNestedMethod()
            {
            }

            public async Task MyNestedAsyncMethod()
            {
            }

            public async Task<T> MyNestedAsyncGenericMethod<U1>(U1 something)
            {
                return default;
            }
        }
    }
}
