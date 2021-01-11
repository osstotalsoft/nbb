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

[<Fact>]
let ``Sequenced effect computations`` () =
    let mapper crt =    
        effect {
            let! x = Effect.from (fun _ -> 1)
            let! y = Effect.from (fun _ -> 2)
            return x + y + crt
        }
    let eff = [1..5000] |> List.traverseEffect mapper

    use interpreter = createInterpreter()

    let result = 
        eff 
        |> Effect.interpret interpreter
        |> Async.RunSynchronously

    result.Length |> should equal 5000

[<Fact>]
let ``Sequenced effect computations with CE`` () =

    let eff = 
        effect' {
            for x in effect' { return [1..5000] } do
            yield x+1
        }

    use interpreter = createInterpreter()

    let result = 
        eff 
        |> Effect.interpret interpreter
        |> Async.RunSynchronously

    result.Length |> should equal 5000