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
open NBB.Application.Mediator.FSharp

[<EntryPoint>]
let main argv =

    // App configuration
    let appConfig (context: HostBuilderContext) (configApp: IConfigurationBuilder) =
        configApp
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional = true)
            .AddJsonFile(sprintf "appsettings.%s.json" context.HostingEnvironment.EnvironmentName, optional = true)
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .AddEnvironmentVariables()
            .AddCommandLine(argv)
        |> ignore

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
        HostBuilder()
            .ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> appConfig)
            .ConfigureServices(Action<HostBuilderContext, IServiceCollection> serviceConfig)
            .ConfigureLogging(Action<HostBuilderContext, ILoggingBuilder> loggingConfig)
            .UseConsoleLifetime()
            .Build()

    host.RunAsync()
    |> Async.AwaitTask
    |> Async.RunSynchronously

    0
