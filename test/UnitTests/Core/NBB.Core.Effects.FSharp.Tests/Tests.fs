module Tests
open Xunit
open FsUnit.Xunit
open NBB.Core.Effects.FSharp
open NBB.Core.Effects.FSharp.Interpreter

[<Fact>]
let ``Pure(1) + Pure(2) should equal Pure(3)`` () =
    let interpreter = createInterpreter()
    let eff = 
        effect {
            let! x = Effect.pure' 1
            let! y = Effect.pure' 2
            return x + y
        }

    eff 
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> should equal 3

[<Fact>]
let ``Effect computations are lazy evaluated`` () =
    let interpreter = createInterpreter()
    let mutable printfnWasCalled = false
    let printfn _str = 
        printfnWasCalled <- true

    let eff = 
        effect' {
            printfn "Execute side effect"
        }

    eff |> ignore
    printfnWasCalled |> should equal false
        
    eff 
        |> Effect.interpret interpreter
        |> Async.RunSynchronously
        |> should equal ()

    printfnWasCalled |> should equal true