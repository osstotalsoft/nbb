module NBB.Contracts.Functional.Api.App

open System
open System.Reflection
open Giraffe
open Giraffe.Serialization
open MediatR
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open NBB.Application.DataContracts
open NBB.Contracts.Application.CommandHandlers
open NBB.Contracts.Application.Commands
open NBB.Contracts.Application.Queries
open NBB.Contracts.Handlers
open NBB.Contracts.ReadModel.Data
open NBB.Correlation.AspNet
open NBB.Messaging.Nats


// ---------------------------------
// Web app
// ---------------------------------

let webApp =
    choose [
        route "/" >=> redirectTo false "/api/contracts"
        subRoute "/api"
            (choose [
                contractsHandler
                contractsAlternativeHandler
            ])
        setStatusCode 401 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080")
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IHostingEnvironment>()
    (match env.IsDevelopment() with
    | true  -> app.UseDeveloperExceptionPage()
    | false -> app.UseGiraffeErrorHandler errorHandler)
        .UseCors(configureCors)
        .UseCorrelation()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddCors()
        .AddGiraffe() 
        .AddSingleton<IJsonSerializer>(
            NewtonsoftJsonSerializer(NewtonsoftJsonSerializer.DefaultSettings))
        .AddMediatR typeof<GetContracts> |> ignore

    services.AddScopedContravariant<IRequestHandler<Command>, MessageBusPublisherCommandHandler> typeof<CreateContract>.Assembly
    services.AddNatsMessaging() 
    services.AddContractsReadModelDataAccess() 

let configureAppConfiguration  (context: WebHostBuilderContext) (config: IConfigurationBuilder) =  
    config
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile(sprintf "appsettings.%s.json" context.HostingEnvironment.EnvironmentName, true)
        .AddUserSecrets(Assembly.GetExecutingAssembly())
        .AddEnvironmentVariables() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddFilter(fun l -> l.Equals LogLevel.Error)
           .AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .ConfigureAppConfiguration(configureAppConfiguration)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0