module EffectBenchmark

open BenchmarkDotNet.Attributes
open NBB.Core.Effects.FSharp
open NBB.Core.Effects.FSharp.Interpreter
open BenchmarkDotNet.Attributes.Jobs

[<SimpleJob (launchCount= 1, warmupCount= 0, targetCount= 10)>]
type Benchmark() =
    let interpreter = createInterpreter()

    [<Params(50, 500, 5000)>]
    [<DefaultValue>] val mutable N : int

    [<GlobalSetup(Target = "RunStrict")>]
    member _.RunStrictSetup() =
        ()

    [<Benchmark>]
    member this.RunLazy() =
        
        let eff = 
            effect' {
             let mapper crt =    
                 effect' { 
                     crt + 1 |> ignore
                     let! x = Effect.pure' 1
                     let! y = Effect.pure' 2
                     return x + y + crt
                 }
             let effects = [1..this.N] |> List.map mapper
             let! vals = List.sequenceEffect effects
         
             return vals
            }

        eff 
            |> Effect.interpret interpreter
            |> Async.RunSynchronously


    [<Benchmark>]
    member this.RunStrict() =
        
        let eff = 
            effect {
               
                let mapper crt =    
                    effect { 
                        crt + 1 |> ignore
                        let! x = Effect.pure' 1
                        let! y = Effect.pure' 2
                        return x + y + crt
                    }
                let effects = [1..this.N] |> List.map mapper
                let! vals = List.sequenceEffect effects
         
                return vals
            }

        eff 
            |> Effect.interpret interpreter
            |> Async.RunSynchronously
    
    



