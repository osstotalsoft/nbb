namespace NBB.Core.Effects.FSharp

open System
open NBB.Core.Effects

type Effect<'a> = NBB.Core.Effects.Effect<'a>

[<RequireQualifiedAccess>]
module Effect = 
    let map func eff = Effect.Map(Func<'a, 'b>(func), eff)

    let bind func eff = Effect.Bind(eff, Func<'a, Effect<'b>>(func))

    let apply (func:Effect<'a->'b>) (eff:Effect<'a>) = Effect.Apply(func |> map(fun f -> Func<'a, 'b>(f)), eff)

    let pure' x = Effect.Pure x

    let return' = pure'

    let from (func: unit -> 'a) = Effect.From (Func<'a>(func))

    let ignore eff = map (fun _ -> ()) eff

    let composeK f g x = bind g (f x)

    let lift2 f = map f >> apply

    let flatten eff = bind id eff

    let interpret (interpreter:IInterpreter) eff = interpreter.Interpret eff |> Async.AwaitTask

module EffectBuilder =
    type LazyEffectBuilder() =
        member _.Bind(eff, func) = Effect.bind func eff
        member _.Return(value) = Effect.pure' value
        member _.ReturnFrom(value) = value
        member _.Combine(eff1, eff2) = Effect.bind (fun _ -> eff2) eff1
        member _.Zero() = Effect.pure' ()
        member _.Delay(f) = f |> Effect.from |> Effect.flatten

    type StrictEffectBuilder() =
        member _.Bind(eff, func) = Effect.bind func eff
        member _.Return(value) = Effect.pure' value
        member _.ReturnFrom(value) = value
        member _.Combine(eff1, eff2Fn) = Effect.bind eff2Fn eff1
        member _.Zero() = Effect.pure' ()
        member _.Delay(f) = f 
        member _.Run(f) = f ()
        
[<AutoOpen>]
module Effects =
    let effect = new EffectBuilder.StrictEffectBuilder()
    let effect' = new EffectBuilder.LazyEffectBuilder()

    let (<!>) = Effect.map
    let (<*>) = Effect.apply
    let (>>=) eff func = Effect.bind func eff
    let (>=>) = Effect.composeK


    [<RequireQualifiedAccess>]
    module List =
        let traverseEffect f list =
            let cons head tail = head :: tail      
            let initState = Effect.pure' []
            let folder head tail = Effect.pure' cons <*> (f head) <*> tail
            List.foldBack folder list initState

        let sequenceEffect list = traverseEffect id list

    [<RequireQualifiedAccess>]
    module Result = 
        let traverseEffect (f: 'a-> Effect<'c>) (result:Result<'a,'e>) : Effect<Result<'c, 'e>> = 
            match result with
                | Error err -> Effect.pure' (Error err)
                | Ok v -> Effect.map Ok (f v)

        let sequenceEffect result = traverseEffect id result


