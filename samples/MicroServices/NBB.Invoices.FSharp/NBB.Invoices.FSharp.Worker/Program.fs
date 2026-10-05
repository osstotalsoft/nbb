// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open NBB.Core.Effects
open NBB.Messaging.Host
open NBB.Messaging.OpenTelemetry
open NBB.Invoices.FSharp.Application
open NBB.Invoices.FSharp.Data
open OpenTelemetry.Trace

[<EntryPoint>]
let main args =
    let builder = Host.CreateApplicationBuilder(args)

    builder.AddServiceDefaults() |> ignore

    builder.Services
    |> WriteApplication.addServices
    |> DataAccess.addServices builder.Configuration
    |> ignore

    builder.Services.AddMessageBus().AddJetStreamTransport(builder.Configuration) |> ignore

    // Subscribes to the NBB.Invoices published language commands, the same ones handled by the C# Invoices worker
    builder.Services.AddMessagingHost(
        builder.Configuration,
        fun hostBuilder ->
            hostBuilder.Configure(fun configBuilder ->
                configBuilder
                    .AddSubscriberServices(fun config -> config.AddTypes(Array.ofList PublishedLanguage.commandTypes) |> ignore)
                    .WithDefaultOptions()
                    .UsePipeline(fun pipelineBuilder ->
                        pipelineBuilder
                            .UseCorrelationMiddleware()
                            .UseExceptionHandlingMiddleware()
                            .UseDefaultResiliencyMiddleware()
                            .UseEffectMiddleware(fun message ->
                                message |> PublishedLanguage.handleMessage |> EffectExtensions.ToUnit)
                        |> ignore)
                |> ignore)
            |> ignore
    )
    |> ignore

    // Registered after the message bus: the instrumentation decorates the bus publisher and subscriber
    builder.Services
        .AddOpenTelemetry()
        .WithTracing(fun tracing -> tracing.AddMessageBusInstrumentation().AddSqlClientInstrumentation() |> ignore)
    |> ignore

    builder.Build().Run()
    0
