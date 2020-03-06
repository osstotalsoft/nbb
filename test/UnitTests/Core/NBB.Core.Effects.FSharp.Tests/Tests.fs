module Tests
open Xunit
open FsUnit.Xunit
open NBB.Core.Effects.FSharp
open Moq

[<Fact>]
let ``Pure(1) + Pure(2) should equal Pure(3)`` () =
    let interpreter = NBB.Core.Effects.Interpreter(Mock.Of<NBB.Core.Effects.ISideEffectHandlerFactory>());
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
