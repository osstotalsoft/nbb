module EffectBenchmark

open BenchmarkDotNet.Attributes
open NBB.Core.Effects.FSharp
open NBB.Core.Effects.FSharp.Interpreter

[<SimpleJob (launchCount= 1, warmupCount= 0, targetCount= 10)>]
type Benchmark() =
    let interpreter = createInterpreter()

    [<Params(50, 500, 5000)>]
    [<DefaultValue>] 
    val mutable N : int

    [<Benchmark>]
    member this.RunLazy() =
        let mapper crt =    
            effect {
                let! x = Effect.from (fun _ -> 1)
                let! y = Effect.from (fun _ -> 2)
                return x + y + crt
            }
        let eff = [1..this.N] |> List.traverse mapper

        eff 
            |> Effect.interpret interpreter
            |> Async.RunSynchronously

    [<Benchmark>]
    member this.RunStrict() =
        let mapper crt =    
            Strict.effect { 
                let! x = Effect.from (fun _ -> 1)
                let! y = Effect.from (fun _ -> 2)
                return x + y + crt
            }
        let eff = [1..this.N] |> List.traverse mapper

        eff 
            |> Effect.interpret interpreter
            |> Async.RunSynchronously

    [<GlobalCleanup>]
    member _.GlobalCleanup() =
        interpreter.Dispose()