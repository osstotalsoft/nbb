namespace NBB.Core.Evented.FSharp

type Evented<'a, 'e> = Evented of 'a * 'e list

module Evented =  
    let bind func (Evented (value, events)) = 
        let (Evented (result, events')) = func value
        Evented (result, events @ events')

    let map func (Evented (value, events)) = 
        Evented (func value, events)

    let result value = Evented (value, [])

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