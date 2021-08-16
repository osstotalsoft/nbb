// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

module Tests

open System
open Xunit
open FsUnit.Xunit
open NBB.Core.Evented.FSharp

type AggRoot = AggRoot of int
type DomainEvent =
    | Added
    | Updated

let create x =  Evented(AggRoot x, [Added])

let increment (AggRoot x) = Evented(AggRoot(x+1), [Updated])

[<Fact>]
let ``Evented workflow test`` () =
    let (Evented(entity, events)) =
        evented {
            let! x = create 1
            return! increment x
        }

    entity|> should equal (AggRoot 2)
    events|> should equal [Added; Updated]


[<Fact>]
let ``Pure evented values should contain no events`` () =
    let (Evented(_, events)) =
        evented {
            return 1
        }

    events|> should equal []

[<Fact>]
let ``List traverse evented should accumulate events`` () =
    let xs = [1;2]
    let fn = fun i ->
        match i with
        |1 -> Evented (1,[Added])
        |_ -> Evented (2,[Updated])

    let (Evented(_, events)) = xs |> List.traverseEvented fn

    events|> should equal [Added;Updated]

[<Fact>]
let ``List sequence evented should accumulate events`` () =
    let fn = fun i ->
        match i with
        |1 -> Evented (1,[Added])
        |_ -> Evented (2,[Updated])
    let xs = [1;2] |> List.map fn

    let (Evented(_, events)) = xs |> List.sequenceEvented

    events|> should equal [Added;Updated]

