namespace NBB.Core.Effects.FSharp

open System
open NBB.Core.Effects

module Effect = 
    let bind func eff = Effect.Bind(eff, Func<'a, IEffect<'b>>(func))
    let map func eff = Effect.Map(Func<'a, 'b>(func), eff)
    let apply (func: IEffect<'a->'b>) eff = Effect.Apply(map (fun fn -> Func<'a,'b>(fn)) func, eff)
    let pureEffect x = Effect.Pure x
    let ignore eff = map (fun _ -> ()) eff

    let composeK f g x = bind g (f x)
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
    let (>>=) eff func = Effect.bind func eff
    let (>=>) = Effect.composeK


module List =
    let traverseEffect f list =
        let cons head tail = head :: tail      
        let initState = Effect.pureEffect []
        let folder head tail = Effect.pureEffect cons <*> (f head) <*> tail
        List.foldBack folder list initState

    let sequenceEffect list = traverseEffect id list

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

        let handler'' (IncrementCommand id) = 
            let handle = loadById >> Effect.map increment >> Effect.bind save
            handle id

        let listHandler = List.traverseEffect handler
