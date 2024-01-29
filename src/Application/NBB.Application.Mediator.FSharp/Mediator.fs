// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Application.Mediator.FSharp

open NBB.Core.Effects.FSharp
open NBB.Core.Effects

type Mediator =
    { DispatchEvent: IEvent -> Effect<unit>
      SendCommand: ICommand -> Effect<unit>
      SendQuery: IQuery -> Effect<obj> }

module Mediator =
    type GetMediatorSideEffect =
        | GetMediatorSideEffect
        interface ISideEffect<Mediator>

    let private getMediator = Effect.Of(GetMediatorSideEffect)

    let dispatchEvent (event: #IEvent) =
        getMediator
        >>= _.DispatchEvent(event :> IEvent)

    let sendCommand (cmd: #ICommand) =
        getMediator
        >>= _.SendCommand(cmd :> ICommand)

    let dispatchEvents (events: #IEvent list) = List.traverse_ dispatchEvent events

    let sendQuery (query: #IQuery<'TResponse>) =
        getMediator
        >>= _.SendQuery(query :> IQuery)
        |> Effect.map unbox<'TResponse>

    let sendMessage message =
        match box message with
        | :? ICommand as command -> sendCommand command
        | :? IEvent as event -> dispatchEvent event
        | _ -> effect { failwith "Invalid message" }

    let handleGetMediator (m: Mediator) (_: GetMediatorSideEffect) = m
