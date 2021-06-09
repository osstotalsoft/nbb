namespace NBB.Invoices.FSharp.Application

open NBB.Core.Effects.FSharp
open NBB.Core.Effects
open NBB.Application.Mediator.FSharp


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
        >>= (fun mediator -> mediator.DispatchEvent(event :> IEvent))

    let sendCommand (cmd: #ICommand) =
        getMediator
        >>= (fun mediator -> mediator.SendCommand(cmd :> ICommand))

    let dispatchEvents (events: #IEvent list) = List.traverse_ dispatchEvent events

    let sendQuery (query: #IQuery<'TResponse>) =
        getMediator
        >>= (fun mediator -> mediator.SendQuery(query :> IQuery))
        |> Effect.map unbox<'TResponse>

    let sendMessage message =
        match box message with
        | :? ICommand as command -> sendCommand command
        | :? IEvent as event -> dispatchEvent event
        | _ -> effect' { failwith "Invalid message" }

    let handleGetMediator (m: Mediator) (_: GetMediatorSideEffect) = m
