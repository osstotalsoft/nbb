namespace NBB.Core.Effects.FSharp

open System
open NBB.Core.Effects


type Effect<'T> = Effect of IEffect<'T>

[<RequireQualifiedAccess>]
module Effect = 
    let wrap eff = Effect eff
    let unWrap (Effect eff) = eff

    let map func (Effect eff) = Effect.Map(Func<'a, 'b>(func), eff)|> wrap

    let bind func (Effect eff) =
        Effect.Bind(eff, Func<'a, IEffect<'b>>(fun a -> func a |> unWrap)) |> wrap

    let apply (func:Effect<'a->'b>) (eff:Effect<'a>) = bind (fun a -> func |> map (fun fn -> fn a)) eff

    let pure' x = Effect.Pure x |> wrap

    let return' = pure'

    let from (func: unit -> 'a) = 
        let func' = Func<'a>(func)
        Effect.From func' |> wrap

    let ignore eff = map (fun _ -> ()) eff

    let composeK f g x = bind g (f x)
    let lift2 f = map f >> apply

    let flatten eff = bind id eff

    let interpret<'a> (interpreter:IInterpreter) (Effect eff) = interpreter.Interpret<'a>(eff) |> Async.AwaitTask

type Effect<'T> with
    static member Map (x, f) = Effect.map  f x
    static member Return (x) = Effect.return' x
    static member (>>=) (x,f) = Effect.bind f x
    static member (<*>) (f,x) = Effect.apply f x

module EffectBuilder =
    type EffectBuilder() =
        member _.Bind(eff, func) = Effect.bind func eff
        member _.Return(value) = Effect.pure' value
        member _.ReturnFrom(value) = value
        member _.Combine(eff1, eff2) = Effect.bind (fun _ -> eff2) eff1
        member _.Zero() = Effect.pure' ()
        member _.Delay(f) = f |> Effect.from |> Effect.flatten
        //member _.Run(f) = Effect.flatten f


[<AutoOpen>]
module Effects =
    let effect = new EffectBuilder.EffectBuilder()

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
