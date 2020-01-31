namespace NBB.Core.Effects.FSharp

open System
open NBB.Core.Effects


module Effect = 
    let bind func eff = Effect.Bind(Func<'a, IEffect<'b>>(func), eff)
    let map func eff = Effect.Map(Func<'a, 'b>(func), eff)
    let apply func eff = Effect.Apply(func, eff)
    let pureEffect x = Effect.Pure x
    let ignore eff = map (fun _ -> ()) eff

    let composeK fn1 fn2 = fn1 >> bind fn2
    let lift2 f = map f >> apply

module EffectBuilder =
    type EffectBuilder() =
        member _.Bind(eff, func) = Effect.bind func eff
        member _.Return(value) = Effect.pureEffect value
        member _.ReturnFrom(value) = value
        member _.Combine(eff1, eff2) = Effect.bind (fun _ -> eff2) eff1
        member _.Zero() = Effect.pureEffect ()


[<AutoOpen>]
module Effects =
    let effect = new EffectBuilder.EffectBuilder()

    let (<!>) = Effect.map
    let (<*>) = Effect.apply
    let (>>=) = Effect.bind
    let (>=>) = Effect.composeK

module private Tests =
    module Domain =
        type AggRoot = AggRoot of int

        let increment (AggRoot x) = AggRoot (x+1)

    module Data =
        let loadById id = Effect.pureEffect (Domain.AggRoot id)
        let save agg = Effect.pureEffect ()

    module Aoolication =
        open Domain
        open Data
        type IncrementCommand = IncrementCommand of int

        let handler (IncrementCommand id) = 
            effect {
                let! agg = loadById id
                let agg' = agg |> increment
                save agg' |> ignore
            }

        let handler' (IncrementCommand id) = id |> loadById |> Effect.map increment |> Effect.bind save

        let handler'' (IncrementCommand id) = save >>= (increment <!> loadById id)
