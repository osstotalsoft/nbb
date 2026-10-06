// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

module NBB.Invoices.FSharp.Api.Program

open System
open System.Text.Json
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Giraffe
open NBB.Correlation.AspNet
open NBB.Invoices.FSharp.Application
open NBB.Invoices.FSharp.Data

let webApp =
    choose [ subRouteCi "/api" (choose [ Handlers.Invoice.handler ]) ]

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    builder.AddServiceDefaults() |> ignore

    builder.Services
    |> ReadApplication.addServices
    |> DataAccess.addServices builder.Configuration
    |> ignore

    builder.Services.AddMessageBus().AddJetStreamTransport(builder.Configuration) |> ignore

    builder.Services
        .AddGiraffe()
        .AddSingleton<Json.ISerializer>(Json.Serializer(JsonSerializerOptions(JsonSerializerDefaults.Web)))
    |> ignore

    let app = builder.Build()

    if app.Environment.IsDevelopment() then
        app.UseDeveloperExceptionPage() |> ignore
    else
        app.UseGiraffeErrorHandler errorHandler |> ignore

    app.UseCorrelation() |> ignore
    app.MapDefaultEndpoints() |> ignore

    // Requests not handled by Giraffe fall through to the endpoints above (the health checks)
    app.UseGiraffe webApp

    app.Run()
    0
