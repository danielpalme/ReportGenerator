using System;
using System.Threading.Tasks;

namespace Test
{
    public class ClassWithLocalFunctions<T1>
    {
        public class MyNestedClass<T2>
        {
            public T2 MyProperty { get; set; }

            public async Task MyAsyncMethod<T3>(T3 myParam)
            {
                await MyAsyncLocalFunction<T3>();

                async Task MyAsyncLocalFunction<T4>()
                {
                    Console.WriteLine(myParam);
                    Console.WriteLine(MyProperty);

                    await Task.CompletedTask;
                }
            }
        }
    }
}
