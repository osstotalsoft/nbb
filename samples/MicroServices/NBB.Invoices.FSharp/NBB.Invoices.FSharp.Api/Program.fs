namespace NBB.Invoices.FSharp.Api

open System
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open NBB.Correlation.AspNet
open Giraffe
open NBB.Invoices.FSharp.Application
open NBB.Invoices.FSharp.Data
open NBB.Messaging.Abstractions
open NBB.Messaging.Nats
open Microsoft.AspNetCore.Cors.Infrastructure
open System.Reflection

module Program =
    let webApp =
        choose [
            route "/" >=>  text "Hello"
            subRoute "/api"
                (choose [
                    Handlers.Invoice.handler
                    //Handlers.ElemDefinitions.handler
                    //Handlers.Compilation.handler
                ])
            setStatusCode 404 >=> text "Not Found" ]

    let errorHandler (ex : Exception) (logger : ILogger) =
        logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
        clearResponse >=> setStatusCode 500 >=> text ex.Message

    // ---------------------------------
    // Config and Main
    // ---------------------------------

    let configureCors (builder : CorsPolicyBuilder) =
        builder.WithOrigins("http://localhost:5000")
               .AllowAnyMethod()
               .AllowAnyHeader()
               |> ignore

    let configureApp (app : IApplicationBuilder) =
        let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
        (match env.IsDevelopment() with
        | true  -> app.UseDeveloperExceptionPage()
        | false -> app.UseGiraffeErrorHandler errorHandler)
            .UseCors(configureCors)
            .UseCorrelation()
            .UseGiraffe(webApp)

    let configureServices (context: WebHostBuilderContext) (services : IServiceCollection) =
        services
        |> ReadApplication.addServices
        |> DataAccess.addServices
        |> ignore

        services
            .AddMessageBus()
            .AddNatsTransport(context.Configuration)
        |> ignore

        services
            .AddCors()
            .AddGiraffe() 
            //.AddSingleton<Giraffe.IJsonSerializer>(
            //    NewtonsoftJsonSerializer(NewtonsoftJsonSerializer.DefaultSettings))
        |> ignore

    let configureAppConfiguration  (context: HostBuilderContext) (config: IConfigurationBuilder) =  
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
    let main args =
        Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(
                fun webHostBuilder ->
                    webHostBuilder
                        .Configure(configureApp)
                        .ConfigureServices(configureServices)
                        .ConfigureLogging(configureLogging)
                        |> ignore)
            .Build()
            .Run()
        0
