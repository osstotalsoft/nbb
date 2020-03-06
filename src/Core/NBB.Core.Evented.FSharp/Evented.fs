namespace NBB.Core.Evented.FSharp

type Evented<'a, 'e> = Evented of payload:'a * events:'e list

module Evented = 
    let map func (Evented(value, events)) = Evented(func value, events)

    let bind func (Evented(value, events)) = 
        let (Evented(result, events')) = func value
        Evented(result, events @ events')

    let apply (Evented(func, events)) (Evented(value, events')) = Evented(func value, events @ events')
    let pure' value = Evented(value, [])
    let return' = pure'

    //let composeK f g x = bind g (f x)

    //let lift2 f = map f >> apply

type Evented<'a, 'e> with
    static member Map (x, f) = Evented.map  f x
    static member Return (x) = Evented.return' x
    static member (>>=) (x,f) = Evented.bind f x
    static member (<*>) (f,x) = Evented.apply f x

module EventedBuilder =
    type EventedBuilder() =
        member _.Bind(evented, func) = Evented.bind func evented
        member _.Return(value) = Evented.return' value
        member _.ReturnFrom(value) = value
        member _.Combine(evented1, evented2) = Evented.bind (fun _ -> evented2) evented1
        member _.Zero() = Evented.return' ()

[<AutoOpen>]
module Events =
    let evented = new EventedBuilder.EventedBuilder()

    //let (<!>) = Evented.map
    //let (<*>) = Evented.apply
    //let (>>=) evented func = Evented.bind func evented
    //let (>=>) = Evented.composeK

//module List =
//    let traverseEvented f list =
//        let cons head tail = head :: tail  
//        let initState = Evented.pure' []
//        let folder head tail = Evented.pure' cons <*> (f head) <*> tail
//        List.foldBack folder list initState

//    let sequenceEvented list = traverseEvented id list