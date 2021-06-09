namespace NBB.Invoices.FSharp.Application

open NBB.Application.Mediator.FSharp
open NBB.Core.Effects.FSharp
open NBB.Core.Effects
open Microsoft.Extensions.DependencyInjection
open NBB.Messaging.Effects

module Middlewares =

    let log =
        fun next req ->
            effect {
                let reqType = req.GetType().FullName
                printfn "Precessing %s" reqType

                let! result = next req
                printfn "Precessed %s" reqType
                return result
            }

    let publishMessage =
        fun _ req ->
            effect {
                do! MessageBus.publish req
                return Some()
            }

module PipelineUtils =
    let terminateRequest<'a> : (Effect<'a option> -> Effect<'a>) =
        Effect.map
            (function
            | Some value -> value
            | None -> failwith "No handler found")

    let terminateEvent : (Effect<unit option> -> Effect<unit>) = Effect.map ignore

module WriteApplication =
    open Middlewares
    open PipelineUtils

    open RequestMiddleware
    open CommandHandler

    let private commandPipeline =
        log
        << handlers [ CreateInvoice.handle |> upCast
                      MarkInvoiceAsPayed.handle |> upCast ]

    open EventMiddleware
    open EventHandler

    let private eventPipeline : EventMiddleware = (*log << *)handlers []


    let addServices (services: IServiceCollection) =
        let sendCommand (cmd: 'TCommand) =
            CommandMiddleware.run commandPipeline cmd
            |> terminateRequest

        let publishEvent (ev: 'TEvent) =
            EventMiddleware.run eventPipeline ev
            |> terminateEvent

        let sendQuery (q: IQuery) =
            RequestHandler.empty q |> terminateRequest

        let mediator =
            { SendCommand = sendCommand
              SendQuery = sendQuery
              DispatchEvent = publishEvent }


        services.AddEffects() |> ignore
        services.AddMessagingEffects() |> ignore

        services.AddSideEffectHandler(Mediator.handleGetMediator mediator)

module ReadApplication =
    open Middlewares
    open PipelineUtils

    open RequestMiddleware
    open CommandHandler

    let private commandPipeline = log << publishMessage

    open EventMiddleware
    open EventHandler

    let private eventPipeline : EventMiddleware = (*log << *)handlers []


    let addServices (services: IServiceCollection) =
        let sendCommand (cmd: 'TCommand) =
            CommandMiddleware.run commandPipeline cmd
            |> terminateRequest

        let publishEvent (ev: 'TEvent) =
            EventMiddleware.run eventPipeline ev
            |> terminateEvent

        let sendQuery (q: IQuery) =
            RequestHandler.empty q |> terminateRequest

        let mediator =
            { SendCommand = sendCommand
              SendQuery = sendQuery
              DispatchEvent = publishEvent }


        services.AddEffects() |> ignore
        services.AddMessagingEffects() |> ignore

        services.AddSideEffectHandler(Mediator.handleGetMediator mediator)
