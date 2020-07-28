namespace NBB.Application.Mediator.FSharp

open NBB.Core.Abstractions
open NBB.Core.Effects.FSharp


type EventHandler<'TEvent when 'TEvent :> IEvent> = 'TEvent -> Effect<unit option>
type EventHandler = EventHandler<IEvent>
type EventMiddleware<'TEvent when 'TEvent :> IEvent> = EventHandler<'TEvent> -> EventHandler<'TEvent>
type EventMiddleware = EventMiddleware<IEvent>


module EventHandler = 
    let empty : EventHandler<'TEvent> = 
        fun _ -> Effect.pure' None

    let rec choose (handlers : EventHandler<'TEvent> list) : EventHandler<'TEvent> =
        fun (ev : 'TEvent) ->
            effect {
                match handlers with
                | [] -> return None
                | handler :: tail ->
                    let! result = handler ev
                    match result with
                    | Some c -> return Some c
                    | None   -> return! choose tail ev
            }

    let mappend (h1: EventHandler<'TEvent>) (h2: EventHandler<'TEvent>): EventHandler<'TEvent> =
        fun ev ->
            effect {
                let! r1 = h1 ev |> Effect.map Option.isSome
                let! r2 = h2 ev |> Effect.map Option.isSome
                if r1 || r2 then return Some () else return None
            }

    let (++) = mappend

module EventMiddleware =
    let lift (handler: EventHandler<'TEvent>) : EventMiddleware<'TEvent> =
        fun (next: EventHandler<'TEvent>) ->
            fun (ev: 'TEvent) ->
                effect {
                    do! handler ev |> Effect.ignore
                    return! next ev
                }

    let handlers (hs: EventHandler<'TEvent> list): EventMiddleware<'TEvent> =
        EventHandler.choose hs |> lift

    let upCast (handler: EventHandler<'TEvent>) : EventHandler = 
        fun ev ->
            match ev with
            | :? 'TEvent as ev' -> handler ev'
            | _ -> Effect.pure' None

    let run (middleware: EventMiddleware) (ev: 'TEvent) = ev :> IEvent |> middleware EventHandler.empty

