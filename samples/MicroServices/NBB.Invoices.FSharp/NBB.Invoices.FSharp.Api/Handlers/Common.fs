// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Invoices.FSharp.Api.Handlers

open NBB.Correlation
open NBB.Application.Mediator.FSharp

// TODO: Find a place for MesageBus wrapper
module MessageBus =
    open NBB.Core.Effects.FSharp
    open NBB.Messaging.Effects

    let publish (obj: 'TMessage) =  MessageBus.Publish (obj :> obj) |> Effect.ignore

module HandlerUtils =
    open Giraffe
    open NBB.Core.Effects
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.Affine
    open Microsoft.Extensions.DependencyInjection

    type Effect<'a> = FSharp.Effect<'a>
    module Effect = FSharp.Effect

    let setError errorText =
        (clearResponse >=> setStatusCode 500 >=> text errorText)

    let interpret<'TResult> (resultHandler: 'TResult -> HttpHandler) (effect: Effect<'TResult>) : HttpHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let interpreter = ctx.RequestServices.GetRequiredService<IInterpreter>()
                let! result = interpreter.Interpret(effect)
                return! (result |> resultHandler) next ctx
            }

    let jsonResult = function
        | Ok value -> json value
        | Error err -> setError err

    let textResult = function
        | Ok value -> text value
        | Error err -> setError err

    let commandResult (command : ICommand) : HttpHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let result = {|
                CommandId = System.Guid.NewGuid()
                CorrelationId = CorrelationManager.GetCorrelationId()
            |}

            Successful.OK result next ctx

    let interpretCommand handler command =
        let resultHandler _ = commandResult command
        command |> handler |> interpret resultHandler

    let publishCommand : (ICommand -> HttpHandler) =
        MessageBus.publish |> interpretCommand

