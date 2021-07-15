open System
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open System.Reflection
open System.IO
open Microsoft.Extensions.DependencyInjection
open NBB.Invoices.FSharp.Application
open NBB.Core.Effects
open NBB.Messaging.Abstractions
open NBB.Messaging.Host
open NBB.Messaging.Nats
open Microsoft.Extensions.Logging
open NBB.Messaging.Host.MessagingPipeline
open NBB.Invoices.FSharp.Data

[<EntryPoint>]
let main argv =

    // Services configuration
    let serviceConfig (context: HostBuilderContext) (services: IServiceCollection) =
        services
        |> WriteApplication.addServices
        |> DataAccess.addServices
        |> ignore

        services
            .AddMessageBus()
            .AddNatsTransport(context.Configuration)
        |> ignore

        services.AddMessagingHost
            (fun hostBuilder ->
                hostBuilder.Configure
                    (fun configBuilder ->
                        configBuilder
                            .AddSubscriberServices(fun config ->
                                config.AddTypes(typeof<CreateInvoice.Command>, typeof<MarkInvoiceAsPayed.Command>)
                                |> ignore)
                            .WithDefaultOptions()
                            .UsePipeline(fun pipelineBuilder ->
                                pipelineBuilder
                                    .UseCorrelationMiddleware()
                                    .UseExceptionHandlingMiddleware()
                                    .UseDefaultResiliencyMiddleware()
                                    .UseEffectMiddleware(fun m ->
                                        m
                                        |> Mediator.sendMessage
                                        |> EffectExtensions.ToUnit)
                                |> ignore)
                        |> ignore)
                |> ignore)
        |> ignore

    // Logging configuration
    let loggingConfig (context: HostBuilderContext) (loggingBuilder: ILoggingBuilder) =
        loggingBuilder.AddConsole().AddDebug() |> ignore

   
    let host =
        Host
            .CreateDefaultBuilder(argv)
            .ConfigureServices(Action<HostBuilderContext, IServiceCollection> serviceConfig)
            .ConfigureLogging(Action<HostBuilderContext, ILoggingBuilder> loggingConfig)
            .UseConsoleLifetime()
            .Build()

    host.RunAsync()
    |> Async.AwaitTask
    |> Async.RunSynchronously

    0
