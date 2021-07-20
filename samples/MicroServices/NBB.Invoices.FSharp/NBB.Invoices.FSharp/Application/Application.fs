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



module WriteApplication =
    open Middlewares

    open RequestMiddleware
    open CommandHandler

    let private commandPipeline =
        log
        << handlers [ CreateInvoice.handle |> upCast
                      MarkInvoiceAsPayed.handle |> upCast ]

    let private queryPipeline: QueryMiddleware = handlers []

    open EventMiddleware

    let private eventPipeline : EventMiddleware = (*log << *)handlers []
    

    let addServices (services: IServiceCollection) =
        services.AddEffects() |> ignore
        services.AddMessagingEffects() |> ignore

        services.AddMediator(commandPipeline, queryPipeline, eventPipeline)

module ReadApplication =
    open Middlewares

    open RequestMiddleware

    let private commandPipeline = log << publishMessage
    let private queryPipeline: QueryMiddleware = handlers []


    open EventMiddleware

    let private eventPipeline : EventMiddleware = (*log << *)handlers []


    let addServices (services: IServiceCollection) =
        services.AddEffects() |> ignore
        services.AddMessagingEffects() |> ignore

        services.AddMediator(commandPipeline, queryPipeline, eventPipeline)
