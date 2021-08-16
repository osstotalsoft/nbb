// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

// Learn more about F# at http://fsharp.org

open System
open BenchmarkDotNet.Running

[<EntryPoint>]
let main argv =
    BenchmarkRunner.Run<EffectBenchmark.Benchmark>() |> ignore
    0 // return an integer exit code
