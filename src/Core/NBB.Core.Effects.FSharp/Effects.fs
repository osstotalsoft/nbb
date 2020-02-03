namespace NBB.Core.Effects.FSharp

open System
open NBB.Core.Effects

module Effect = 
    let bind eff func = Effect.Bind(eff, Func<'a, IEffect<'b>>(func))
    let map func eff = Effect.Map(Func<'a, 'b>(func), eff)
    let apply func eff = Effect.Apply(func, eff)
    let pureEffect x = Effect.Pure x
    let ignore eff = map (fun _ -> ()) eff

    let composeK f g x = bind (f x) g
    let lift2 f = map f >> apply

module EffectBuilder =
    type EffectBuilder() =
        member _.Bind(eff, func) = Effect.bind eff func
        member _.Return(value) = Effect.pureEffect value
        member _.ReturnFrom(value) = value
        member _.Combine(eff1, eff2) = Effect.bind eff1 (fun _ -> eff2)
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
