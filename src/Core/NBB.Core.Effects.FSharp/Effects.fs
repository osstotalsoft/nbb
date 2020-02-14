namespace NBB.Core.Effects.FSharp

open System
open NBB.Core.Effects

module Effect = 
    let bind func eff = Effect.Bind(eff, Func<'a, IEffect<'b>>(func))
    let map func eff = Effect.Map(Func<'a, 'b>(func), eff)
    let apply (func: IEffect<'a->'b>) eff = Effect.Apply(map (fun fn -> Func<'a,'b>(fn)) func, eff)
    let pure' x = Effect.Pure x
    let return' = pure'
    let ignore eff = map (fun _ -> ()) eff

    let composeK f g x = bind g (f x)
    let lift2 f = map f >> apply

    let interpret<'a> (interpreter:IInterpreter) eff = interpreter.Interpret<'a>(eff) |> Async.AwaitTask

module EffectBuilder =
    type EffectBuilder() =
        member _.Bind(eff, func) = Effect.bind func eff
        member _.Return(value) = Effect.pure' value
        member _.ReturnFrom(value) = value
        member _.Combine(eff1, eff2) = Effect.bind (fun _ -> eff2) eff1
        member _.Zero() = Effect.pure' ()


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
        let initState = Effect.pure' []
        let folder head tail = Effect.pure' cons <*> (f head) <*> tail
        List.foldBack folder list initState

    let sequenceEffect list = traverseEffect id list
