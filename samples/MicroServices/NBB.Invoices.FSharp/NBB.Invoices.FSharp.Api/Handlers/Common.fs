// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Invoices.FSharp.Api.Handlers

module HandlerUtils =
    open Giraffe
    open NBB.Core.Effects
    open NBB.Core.Effects.FSharp
    open NBB.Messaging.Effects
    open Microsoft.AspNetCore.Http
    open Microsoft.Extensions.DependencyInjection

    let interpret<'TResult> (resultHandler: 'TResult -> HttpHandler) (effect: Effect<'TResult>) : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let interpreter = ctx.RequestServices.GetRequiredService<IInterpreter>()
                let! result = interpreter.Interpret(effect)
                return! (result |> resultHandler) next ctx
            }

    /// Publishes a message on the bus and returns an empty 200 response, like the C# Invoices API
    let publish (message: 'TMessage) : HttpHandler =
        MessageBus.Publish(message :> obj)
        |> Effect.ignore
        |> interpret (fun () -> setStatusCode 200)

    let jsonOrNotFound =
        function
        | Some value -> json value
        | None -> RequestErrors.NOT_FOUND "Not Found"
