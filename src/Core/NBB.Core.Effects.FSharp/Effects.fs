namespace NBB.Core.Effects.FSharp

open System
open NBB.Core.Effects


module Effect = 
    let bind func eff = Effect.Bind(Func<'a, IEffect<'b>>(func), eff)
    let map func eff = Effect.Map(Func<'a, 'b>(func), eff)
    let pureEffect x = Effect.Pure x
    let ignore eff = map (fun _ -> ()) eff

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

