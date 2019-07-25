namespace NBB.Contracts.FSharpMvc.Api

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open MediatR
open NBB.Application.DataContracts
open NBB.Contracts.Application.CommandHandlers
open NBB.Contracts.Application.Commands
open NBB.Messaging.Nats
open NBB.Contracts.ReadModel.Data
open NBB.Contracts.Application.Queries
open NBB.Correlation.AspNet

type Startup private () =
    new (configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices(services: IServiceCollection) =
        // Add framework services.
        services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2) |> ignore
        services.AddMediatR typeof<GetContracts>  |> ignore
        services.AddScopedContravariant<IRequestHandler<Command>, MessageBusPublisherCommandHandler> typeof<CreateContract>.Assembly 
        services.AddNatsMessaging() 
        services.AddContractsReadModelDataAccess()

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore

        app.UseCorrelation() |> ignore
        app.UseMvc() |> ignore


    member val Configuration : IConfiguration = null with get, set
