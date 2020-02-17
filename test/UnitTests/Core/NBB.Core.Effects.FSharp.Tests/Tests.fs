module Tests
open Xunit
open FsUnit.Xunit
open NBB.Core.Effects.FSharp
open NBB.Core.Effects
open Moq

[<Fact>]
let ``Pure(1) + Pure(2) should equal Pure(3)`` () =
    let interpreter = Interpreter(Mock.Of<ISideEffectHandlerFactory>());
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
