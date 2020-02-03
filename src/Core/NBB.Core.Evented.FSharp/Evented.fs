namespace NBB.Core.Evented.FSharp

type Evented<'a, 'e> = Evented of payload:'a * events:'e list

module Evented = 
    let map func (Evented(value, events)) = Evented(func value, events)

    let bind (Evented(value, events)) func = 
        let (Evented(result, events')) = func value
        Evented(result, events @ events')

    let apply (Evented(func, events)) (Evented(value, events')) = Evented(func value, events @ events')

    let result value = Evented(value, [])

    let composeK f g x = bind (f x) g

    let lift2 f = map f >> apply

module EventedBuilder =
    type EventedBuilder() =
        member _.Bind(eff, func) = Evented.bind eff func
        member _.Return(value) = Evented.result value
        member _.ReturnFrom(value) = value
        member _.Combine(eff1, eff2) = Evented.bind eff1 (fun _ -> eff2)
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

    let create x =  Evented(AggRoot x, [Added])

    let update (x:AggRoot) = Evented(x, [Updated])
    let increment (AggRoot x) = AggRoot (x + 1)

    let createAndUpdate x = x |> create >>= update
    let createAndUpdate''' = create >=> update
    let createAndUpdate'''' x =
        evented {
            let! x' = create x
            let! x'' = update x'
            return x''
        }

    let createAndUpdate'''''  x =
        let (Evented(agg, events)) = create x
        let (Evented(agg', events')) = update agg
        Evented(agg', events @ events')


    let createAndIncrement x = x |> create |> Evented.map increment
    let createAndIncrement' = create >> Evented.map increment
    let createAndIncrement'' x = increment <!> create x
    let createAndIncrement''' x =
        evented {
            let! x' = create x
            return increment x'
        }

    let liftedSum = Evented.lift2 (+)
    let z = liftedSum (Evented(1, [Added])) (Evented(2, [Updated]))