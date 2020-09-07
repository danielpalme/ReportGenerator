open Microsoft.VisualStudio.TestTools.UnitTesting
open ViewModels

[<TestClass>]
type Tests() = 
    [<TestMethod>]
    member x.FSharp_ExecuteTest () =
        (new TestMouseBehavior()).RunTest()

[<EntryPoint>]
[<System.STAThread>]
let main argv = 
    printfn "Start"
    let behavior = new TestMouseBehavior()
    behavior.RunTest()
    printfn "End"
    0