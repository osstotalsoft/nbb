// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

module EffectBenchmark

open BenchmarkDotNet.Attributes
open NBB.Core.Effects.FSharp
open NBB.Core.Effects.FSharp.Interpreter

[<SimpleJob (launchCount= 1, warmupCount= 0, iterationCount= 10)>]
type Benchmark() =
    let interpreter = createInterpreter()

    [<Params(50, 500, 5000)>]
    [<DefaultValue>]
    val mutable N : int

    [<Benchmark>]
    member this.RunLazy() =
        task {
            let mapper crt =
                effect {
                    let! x = Effect.from (fun _ -> 1)
                    let! y = Effect.from (fun _ -> 2)
                    return x + y + crt
                }
            let eff = [1..this.N] |> List.traverse mapper

            let! _  = eff |> Effect.interpret interpreter
            return ()
        }

    [<Benchmark>]
    member this.RunStrict() =
        task {
            let mapper crt =
                Strict.effect {
                    let! x = Effect.from (fun _ -> 1)
                    let! y = Effect.from (fun _ -> 2)
                    return x + y + crt
                }
            let eff = [1..this.N] |> List.traverse mapper

            let! _ = eff |> Effect.interpret interpreter
            return ()
        }

    [<GlobalCleanup>]
    member _.GlobalCleanup() =
        interpreter.Dispose()
