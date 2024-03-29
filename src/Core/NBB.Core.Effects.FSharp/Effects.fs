﻿// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

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

    let interpret (interpreter:IInterpreter) eff = interpreter.Interpret eff 
type EffectList<'a> = Effect<list<'a>>

[<RequireQualifiedAccess>]
module EffectList =
    [<RequireQualifiedAccess>]
    module private List =
        let flatten (listOfLists: list<list<'a>>): list<'a> = listOfLists |> List.collect id

    let map (func: 'a -> 'b) (effectList: EffectList<'a>): EffectList<'b> =
        effectList |> Effect.map (List.map func)

    let sequence (list: Effect<'a> list) = list |> List.toSeq |> Effect.Sequence |> Effect.map Seq.toList
    let traverse f list = list |> List.map f |> sequence


    let bind (func: 'a -> EffectList<'b>) (elemList: EffectList<'a>): EffectList<'b> =
        elemList
        |> Effect.bind
            ((List.map func)
             >> sequence
             >> Effect.map List.flatten)

    let return' (x: 'a): EffectList<'a> = Effect.return' [ x ]

    let hoist (list: 'a list): EffectList<'a> = Effect.return' list
    let lift (elem: Effect<'a>): EffectList<'a> = elem |> Effect.map (fun x -> [ x ])

    let filter (predicate: 'a -> bool) (coll: EffectList<'a>): EffectList<'a> =
        coll |> Effect.map (List.filter predicate)

    let filterEffect (predicate: 'a -> Effect<bool>) (coll: EffectList<'a>): EffectList<'a> =
        let (<*>) = Effect.apply
        let return' = Effect.return'
        let initState = hoist []
        let consIf cond head tail = if cond then head :: tail else tail

        let folder (head: 'a) (tail: EffectList<'a>) =
            return' consIf
            <*> (predicate head)
            <*> (return' head)
            <*> tail

        coll
        |> Effect.bind (fun list -> List.foldBack folder list initState)

module EffectBuilder =
    let private safeDispose (resource: #IDisposable) = if (not (isNull resource)) then resource.Dispose()

    type LazyEffectBuilder() =
        member _.Bind(eff, func) = Effect.bind func eff
        member _.Return(value) = Effect.pure' value
        member _.ReturnFrom(value) = value
        member _.Combine(eff1, eff2) = Effect.bind (fun _ -> eff2) eff1
        member _.Zero() = Effect.pure' ()
        member _.For(coll, f) = coll |> EffectList.bind f
        member _.Yield(value) = EffectList.return' value
        member _.YieldFrom(x) = EffectList.lift x
        member _.Delay(f) = f |> Effect.from |> Effect.flatten
        member _.TryWith(eff, handler) =  Effect.TryWith(eff, Func<_, _>(handler))
        member _.TryFinally(eff, compensation) = Effect.TryFinally(eff, Action(compensation))
        member _.Using(resource, expr) = Effect.TryFinally(expr resource, fun () -> safeDispose resource)

    type StrictEffectBuilder() =
        member _.Bind(eff, func) = Effect.bind func eff
        member _.Return(value) = Effect.pure' value
        member _.ReturnFrom(value) = value
        member _.Combine(eff1, eff2Fn) = Effect.bind eff2Fn eff1
        member _.Zero() = Effect.pure' ()
        member _.For(coll, f) = coll |> EffectList.bind f
        member _.Yield(value) = EffectList.return' value
        member _.YieldFrom(x) = EffectList.lift x
        member _.Delay(f) = f
        member _.Run(f) = f ()
        member _.TryWith(expr, handler) = try expr() with ex -> handler ex
        member _.TryFinally(expr, compensation) = try expr() finally compensation()
        member _.Using(resource, expr) =  try expr resource finally safeDispose resource

[<AutoOpen>]
module Effects =
    let (<!>) = Effect.map
    let (<*>) = Effect.apply
    let (>>=) eff func = Effect.bind func eff
    let (>=>) = Effect.composeK


    [<RequireQualifiedAccess>]
    module List =
        let traverse  = EffectList.traverse
        let traverse_ f list  = traverse f list |> Effect.ignore

        let sequence = EffectList.sequence
        let sequence_ list = sequence list |> Effect.ignore

    [<RequireQualifiedAccess>]
    module Result =
        let traverse (f: 'a-> Effect<'c>) (result:Result<'a,'e>) : Effect<Result<'c, 'e>> =
            match result with
                | Error err -> Effect.pure' (Error err)
                | Ok v -> Effect.map Ok (f v)

        let sequence result = traverse id result

[<AutoOpen>]
module Lazy =
    let effect = new EffectBuilder.LazyEffectBuilder()

module Strict =
    let effect = new EffectBuilder.StrictEffectBuilder()
