// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

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

module WriteApplication =
    open Middlewares

    open RequestMiddleware
    open CommandHandler

    let private commandPipeline =
        log
        << handlers [ CreateInvoice.handle |> upCast
                      MarkInvoiceAsPayed.handle |> upCast
                      ProcessInvoice.handle |> upCast ]

    let private queryPipeline: QueryMiddleware = handlers []

    open EventMiddleware

    let private eventPipeline: EventMiddleware =
        handlers [ PublishedLanguage.publishIntegrationEvents |> EventHandler.upCast ]


    let addServices (services: IServiceCollection) =
        services.AddEffects() |> ignore
        services.AddMessagingEffects() |> ignore

        services.AddMediator(commandPipeline, queryPipeline, eventPipeline)

module ReadApplication =
    open RequestMiddleware
    open QueryHandler

    let private commandPipeline: CommandMiddleware = handlers []

    let private queryPipeline: QueryMiddleware =
        handlers [ GetInvoices.handle |> upCast
                   GetInvoice.handle |> upCast ]

    open EventMiddleware

    let private eventPipeline: EventMiddleware = handlers []


    let addServices (services: IServiceCollection) =
        services.AddEffects() |> ignore
        services.AddMessagingEffects() |> ignore

        services.AddMediator(commandPipeline, queryPipeline, eventPipeline)
