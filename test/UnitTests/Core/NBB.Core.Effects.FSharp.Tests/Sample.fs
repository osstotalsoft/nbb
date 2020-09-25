﻿module Sample

open System
open NBB.Core.Effects.FSharp

module Domain =
    type AggRoot = AggRoot of int

    let increment (AggRoot x) = AggRoot (x+1)

module Data =
    let loadById id = Effect.pure' (Domain.AggRoot id)
    let save _agg = Effect.pure' ()

module Application =
    open Domain
    open Data    
    type IncrementCommand = IncrementCommand of int
  
    let handler (IncrementCommand id) = 
        effect {
            let! agg = loadById id
            let agg' = agg |> increment
            do! save agg'
        }

    let handler' (IncrementCommand id) = id |> loadById |> Effect.map increment >>= save

    let handler'' (IncrementCommand id) = 
        let handle = loadById >> Effect.map increment >> Effect.bind save
        handle id

    let listHandler (commandList: _ list) = List.traverseEffect handler commandList |> Effect.ignore

