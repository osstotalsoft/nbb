namespace NBB.Core.Evented.FSharp

type Evented<'a, 'e> = 'a * 'e list

module Evented = 
    let map func (value, events) = 
        (func value, events)

    let bind func (value, events) = 
        let (result, events') = func value
        (result, events @ events')

    let apply (func, events) (value, events') = (func value, events @ events')

    let result value = (value, [])

    let composeK fn1 fn2 = fn1 >> bind fn2

    let lift2 f = map f >> apply

module EventedBuilder =
    type EventedBuilder() =
        member _.Bind(eff, func) = Evented.bind func eff
        member _.Return(value) = Evented.result value
        member _.ReturnFrom(value) = value
        member _.Combine(eff1, eff2) = Evented.bind (fun _ -> eff2) eff1
        member _.Zero() = Evented.result ()

[<AutoOpen>]
module Events =
    let evented = new EventedBuilder.EventedBuilder()

    let (<!>) = Evented.map
    let (<*>) = Evented.apply
    let (>>=) = Evented.bind
    let (>=>) = Evented.composeK


module private Tests =
    type AggRoot = AggRoot of int
    type DomainEvent = 
        | Added
        | Updated


    let create x =  (AggRoot x, [Added])
    let update (x:AggRoot) = (x, [Updated])
    let increment (AggRoot x) = AggRoot (x + 1)

    let createAndUpdate x = x |> create |> Evented.bind update
    let createAndUpdate'= create >> Evented.bind update
    let createAndUpdate'' x = update >>= create x
    let createAndUpdate''' = create >=> update
    let createAndUpdate'''' x =
        evented {
            let! x' = create x
            let! x'' = update x'
            return x''
        }

    let createAndIncrement x = x |> create |> Evented.map increment
    let createAndIncrement' = create >> Evented.map increment
    let createAndIncrement'' x = increment <!> create x
    let createAndIncrement''' x =
        evented {
            let! x' = create x
            return increment x'
        }

    let liftedSum = Evented.lift2 (+)
    let z = liftedSum (1, [Added]) (2, [Updated])