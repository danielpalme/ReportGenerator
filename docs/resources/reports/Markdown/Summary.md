# Summary

|||
|:---|:---|
| Generated on: | 06.10.2025 - 21:58:01 |
| Parser: | OpenCover |
| Assemblies: | 1 |
| Classes: | 4 |
| Files: | 5 |
| **Line coverage:** | 69.4% (75 of 108) |
| Covered lines: | 75 |
| Uncovered lines: | 33 |
| Coverable lines: | 108 |
| Total lines: | 260 |
| **Branch coverage:** | 50% (4 of 8) |
| Covered branches: | 4 |
| Total branches: | 8 |
| **Method coverage:** | 66.6% (16 of 24) |
| **Full method coverage:** | 50% (12 of 24) |
| Covered methods: | 16 |
| Fully covered methods: | 12 |
| Total methods: | 24 |

# Risk Hotspots

No risk hotspots found.

# Coverage

| **Name** | **Covered** | **Uncovered** | **Coverable** | **Total** | **Line coverage** | **Covered** | **Total** | **Branch coverage** | **Covered** | **Total** | **Method coverage** | **Full method coverage** |
|:---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| **Sample** | **75** | **33** | **108** | **260** | **69.4%** | **4** | **8** | **50%** | **16** | **24** | **66.6%** | **50%** |
| [Sample.PartialClass](#samplepartialclass) | 12 | 10 | 22 | 53 | 54.5% | 1 | 2 | 50% | 3 | 6 | 50% | 33.3% |
| [Test.Program](#testprogram) | 15 | 0 | 15 | 84 | 100% | 0 | 0 |  | 3 | 3 | 100% | 100% |
| [Test.TestClass](#testtestclass) | 24 | 9 | 33 | 38 | 72.7% | 2 | 4 | 50% | 4 | 5 | 80% | 60% |
| [Test.TestClass2](#testtestclass2) | 24 | 14 | 38 | 85 | 63.1% | 1 | 2 | 50% | 6 | 10 | 60% | 40% |

# Sample.PartialClass

## Summary

|||
|:---|:---|
| Class: | Sample.PartialClass |
| Assembly: | Sample |
| **File(s):** | C:\temp\PartialClass.cs<br />C:\temp\PartialClass2.cs |
| **Line coverage:** | 54.5% (12 of 22) |
| Covered lines: | 12 |
| Uncovered lines: | 10 |
| Coverable lines: | 22 |
| Total lines: | 53 |
| **Branch coverage:** | 50% (1 of 2) |
| Covered branches: | 1 |
| Total branches: | 2 |
| **Method coverage:** | 50% (3 of 6) |
| **Full method coverage:** | 33.3% (2 of 6) |
| Covered methods: | 3 |
| Fully covered methods: | 2 |
| Total methods: | 6 |

## Metrics

| **Method** | **Branch coverage** | **Crap Score** | **Cyclomatic complexity** | **NPath complexity** | **Sequence coverage** |
|:---|---:|---:|---:|---:|---:|
| **ExecutedMethod_1()** | 100% | 1 | 1 | 0 | 100% |
| **ExecutedMethod_2()** | 100% | 1 | 1 | 0 | 100% |
| **UnExecutedMethod_1()** | 0% | 2 | 1 | 0 | 0% |
| **UnExecutedMethod_2()** | 0% | 2 | 1 | 0 | 0% |

## File(s)

### C:\temp\PartialClass.cs
```
 1           using System;
 2           
 3           namespace Test
 4           {
 5               partial class PartialClass
 6               {
 7                   public void ExecutedMethod_1()
 8  ✔  1             {
 9  ✔  1                 Console.WriteLine("Test");
10  ✔  1             }
11           
12                   public void UnExecutedMethod_1()
13  ❌ 0             {
14  ❌ 0                 Console.WriteLine("Test");
15  ❌ 0             }
16           
17                   private int someProperty;
18           
19                   public int SomeProperty
20                   {
21  ❌ 0                 get { return this.someProperty; }
22           
23                       set
24  ✔  1                 {
25  ✓  1  ◑                  if (value < 0)
26  ✔  1                     {
27  ✔  1                         this.someProperty = 0;
28  ✔  1                     }
29                           else
30  ❌ 0                     {
31  ❌ 0                         this.someProperty = value;
32  ❌ 0                     }
33  ✔  1                 }
34                   }
35               }
36           }
```
### C:\temp\PartialClass2.cs
```
 1           using System;
 2           
 3           namespace Test
 4           {
 5               partial class PartialClass
 6               {
 7                   public void ExecutedMethod_2()
 8  ✔  1             {
 9  ✔  1                 Console.WriteLine("Test");
10  ✔  1             }
11           
12                   public void UnExecutedMethod_2()
13  ❌ 0             {
14  ❌ 0                 Console.WriteLine("Test");
15  ❌ 0             }
16               }
17           }
```
# Test.Program

## Summary

|||
|:---|:---|
| Class: | Test.Program |
| Assembly: | Sample |
| **File(s):** | C:\temp\Program.cs |
| **Line coverage:** | 100% (15 of 15) |
| Covered lines: | 15 |
| Uncovered lines: | 0 |
| Coverable lines: | 15 |
| Total lines: | 84 |
| Covered branches: | 0 |
| Total branches: | 0 |
| **Method coverage:** | 100% (3 of 3) |
| **Full method coverage:** | 100% (3 of 3) |
| Covered methods: | 3 |
| Fully covered methods: | 3 |
| Total methods: | 3 |

## Metrics

| **Method** | **Branch coverage** | **Crap Score** | **Cyclomatic complexity** | **NPath complexity** | **Sequence coverage** |
|:---|---:|---:|---:|---:|---:|
| **CallAsyncMethod()** | 100% | 3 | 3 | 0 | 100% |
| **.ctor(...)** | 100% | 1 | 1 | 0 | 100% |
| **SendAsync(...)** | 100% | 1 | 1 | 0 | 100% |

## File(s)

### C:\temp\Program.cs
```
 1           using System.Net.Http;
 2           using System.Threading;
 3           using System.Threading.Tasks;
 4           using Microsoft.VisualStudio.TestTools.UnitTesting;
 5           
 6           namespace Test
 7           {
 8               [TestClass]
 9               public class Program
10               {
11                   static void Main(string[] args)
12                   {
13                       new TestClass().SampleFunction();
14           
15                       new TestClass2("Test").ExecutedMethod();
16                       new TestClass2("Test").SampleFunction("Munich");
17           
18                       new PartialClass().ExecutedMethod_1();
19                       new PartialClass().ExecutedMethod_2();
20                       new PartialClass().SomeProperty = -10;
21           
22  ✔  2                 new PartialClassWithAutoProperties().Property1 = "Test";
23                       new PartialClassWithAutoProperties().Property2 = "Test";
24           
25                       new SomeClass().Property1 = "Test";
26           
27                       new ClassWithExcludes().IncludedMethod();
28                       new ClassWithExcludes().ExcludedMethod();
29           
30                       new GenericClass<SomeModel, IState>().Process(null);
31                       new GenericClass<SomeModel, IState>().PostProcess(null);
32           
33                       new CodeContract_Target().Calculate(-1);
34           
35                       new AbstractClass_SampleImpl1();
36                       new AbstractClass_SampleImpl2();
37           
38                       CallAsyncMethod();
39           
40                       try
41                       {
42                           new CodeContract_Target().Calculate(0);
43                       }
44                       catch (System.ArgumentException)
45                       {
46                       }
47                   }
48           
49                   [TestMethod]
50  ✔  2             public void CSharp_ExecuteTest1()
51                   {
52                       Main(null);
53                   }
54           
55                   [TestMethod]
56                   public void CSharp_ExecuteTest2()
57                   {
58                       Main(null);
59                   }
60           
61                   private static async void CallAsyncMethod()
62  ✔  1             {
63  ✔  1                 var expected = new HttpResponseMessage();
64  ✔  1                 var handler = new AsyncClass() { InnerHandler = new EchoHandler(expected) };
65  ✔  1                 var invoker = new HttpMessageInvoker(handler, false);
66  ✔  1                 var actual = await invoker.SendAsync(new HttpRequestMessage(), new CancellationToken());
67  ✔  1             }
68           
69                   private class EchoHandler : DelegatingHandler
70                   {
71                       private HttpResponseMessage _response;
72           
73  ✔  1                 public EchoHandler(HttpResponseMessage response)
74  ✔  1                 {
75  ✔  1                     this._response = response;
76  ✔  1                 }
77           
78                       protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancell
79  ✔  1                 {
80  ✔  1                     return Task.FromResult(this._response);
81  ✔  1                 }
82                   }
83               }
84           }
```
# Test.TestClass

## Summary

|||
|:---|:---|
| Class: | Test.TestClass |
| Assembly: | Sample |
| **File(s):** | C:\temp\TestClass.cs |
| **Line coverage:** | 72.7% (24 of 33) |
| Covered lines: | 24 |
| Uncovered lines: | 9 |
| Coverable lines: | 33 |
| Total lines: | 38 |
| **Branch coverage:** | 50% (2 of 4) |
| Covered branches: | 2 |
| Total branches: | 4 |
| **Method coverage:** | 80% (4 of 5) |
| **Full method coverage:** | 60% (3 of 5) |
| Covered methods: | 4 |
| Fully covered methods: | 3 |
| Total methods: | 5 |

## Metrics

| **Method** | **Branch coverage** | **Cyclomatic complexity** | **NPath complexity** | **Sequence coverage** |
|:---|---:|---:|---:|---:|
| **SampleFunction()** | 50% | 4 | 4 | 80% |
| **ParentMethod()** | 100% | 1 | 1 | 100% |
| **NestedLocalFunction(System.String)** | 100% | 1 | 1 | 100% |
| **MethodWithLambda()** | 100% | 1 | 1 | 100% |
| **SampleFunction()** | 100% | 1 | 1 | 0% |

## File(s)

### C:\temp\TestClass.cs
```
 1           using System;
 2           
 3           namespace Test
 4           {
 5               class TestClass
 6               {
 7                   public void SampleFunction()
 8                   {
 9  ✔  2                 string test = string.Format(
10  ✔  2                     "{0} {1}",
11  ✔  2                      "Hello",
12  ✔  2                      "World");
13  ✔  2     
14                       Console.WriteLine(test);
15  ✔  2                 int i = 10;
16  ✔  2     
17                       if (i > 0 || i > 1)
18  ✓  2  ◑              {
19  ✔  2                     Console.WriteLine(i + " is greater that 0");
20  ✔  2                 }
21  ✔  2                 else
22                       {
23  ❌ 0                     Console.WriteLine(i + " is not greater that 0");
24  ❌ 0                 }
25  ❌ 0             }
26  ✔  2     
27                   public class NestedClass
28                   {
29  ✔  2                 public void SampleFunction()
30  ✔  2                 {
31                           Console.WriteLine(
32  ✔  2                         "{0} {1}",
33                                "Hello",
34                                "World");
35  ✔  2                 }
36  ✔  2             }
37  ✔  2         }
38  ✔  2     }
```
# Test.TestClass2

## Summary

|||
|:---|:---|
| Class: | Test.TestClass2 |
| Assembly: | Sample |
| **File(s):** | C:\temp\TestClass2.cs |
| **Line coverage:** | 63.1% (24 of 38) |
| Covered lines: | 24 |
| Uncovered lines: | 14 |
| Coverable lines: | 38 |
| Total lines: | 85 |
| **Branch coverage:** | 50% (1 of 2) |
| Covered branches: | 1 |
| Total branches: | 2 |
| **Method coverage:** | 60% (6 of 10) |
| **Full method coverage:** | 40% (4 of 10) |
| Covered methods: | 6 |
| Fully covered methods: | 4 |
| Total methods: | 10 |

## Metrics

| **Method** | **Branch coverage** | **Crap Score** | **Cyclomatic complexity** | **NPath complexity** | **Sequence coverage** |
|:---|---:|---:|---:|---:|---:|
| **.ctor()** | 0% | 2 | 1 | 0 | 0% |
| **.ctor(...)** | 100% | 1 | 1 | 0 | 100% |
| **ExecutedMethod()** | 100% | 1 | 1 | 0 | 100% |
| **UnExecutedMethod()** | 0% | 2 | 1 | 0 | 0% |
| **SampleFunction(...)** | 66.67% | 5 | 5 | 2 | 100% |
| **DoSomething(...)** | 0% | 2 | 1 | 0 | 0% |

## File(s)

### C:\temp\TestClass2.cs
```
 1            using System;
 2            using System.Collections.Generic;
 3            using System.Linq;
 4            
 5            namespace Test
 6            {
 7                class TestClass2
 8                {
 9                    private string name;
10            
11  ✔   2             private Dictionary<string, int> dict = new Dictionary<string, int>();
12            
13  ✔   3             public string ExecutedProperty { get; set; }
14            
15  ❌  0             public string UnExecutedProperty { get; set; }
16            
17  ❌  0             public TestClass2()
18  ❌  0             {
19  ❌  0                 this.name = "Nobody";
20  ❌  0                 this.ExecutedProperty = "Nobody";
21  ❌  0             }
22            
23  ✔   2             public TestClass2(string name)
24  ✔   2             {
25  ✔   2                 this.name = name;
26  ✔   2                 this.ExecutedProperty = name + name;
27  ✔   2             }
28            
29                    public void ExecutedMethod()
30  ✔   1             {
31  ✔   1                 Console.WriteLine(this.name);
32  ✔   1                 Console.WriteLine(this.ExecutedProperty);
33  ✔   1             }
34            
35                    public void UnExecutedMethod()
36  ❌  0             {
37  ❌  0                 Console.WriteLine(this.name);
38  ❌  0                 Console.WriteLine(this.ExecutedProperty);
39  ❌  0             }
40            
41                    public void SampleFunction(string city)
42  ✔   1             {
43  ✔   1                 int[] values = new int[] { 0, 1, 2, 3 };
44            
45  ✔   5                 var doubled = values.Select(i => i * 2);
46            
47  ✔  11                 foreach (var item in doubled)
48  ✔   4                 {
49  ✔   4                     Console.WriteLine(item);
50  ✔   4                 }
51            
52  ✔   1                 string[] cities = new string[] { "Berlin", "Munich", "Paris" };
53            
54  ✓   4  ◑              if (cities.SingleOrDefault(c => c.Equals(city, StringComparison.OrdinalIgnoreCase)) != null)
55  ✔   1                 {
56  ✔   1                     Console.WriteLine("Found " + city);
57  ✔   1                 }
58  ✔   1             }
59            
60                    public string DoSomething(string value,
61                        string[] stringArray,
62                        Guid id,
63                        IEnumerable<string> stringEnumerable,
64                        IList<string> stringList,
65                        decimal dec,
66                        int i,
67                        Dictionary<string, int> dict,
68                        out int g,
69                        float fff,
70                        double dou,
71                        bool bo,
72                        byte by,
73                        char ch,
74                        object o,
75                        sbyte sby,
76                        short sh,
77                        uint ui,
78                        ulong ul,
79                        ushort usho)
80  ❌  0             {
81  ❌  0                 g = 0;
82  ❌  0                 return null;
83  ❌  0             }
84                }
85            }
```
