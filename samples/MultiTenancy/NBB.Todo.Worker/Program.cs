// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using NBB.Todos.Data;
using NBB.Todo.Worker.Application;
using NBB.Messaging.Host;
using NBB.Messaging.MultiTenancy;
using NBB.MultiTenancy.Abstractions.Repositories;
using Serilog.Events;
using Microsoft.Extensions.Logging;
using NBB.Correlation.Serilog;
using NBB.Tools.Serilog.Enrichers.TenantId;

namespace NBB.Todo.Worker
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                var host = BuildConsoleHost(args);

                Log.Information("Starting NBB.Tasks.Worker");

                await host.RunAsync(CancellationToken.None);

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHost BuildConsoleHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .UseSerilog((context, services, logConfig) =>
                {
                    logConfig
                        .ReadFrom.Configuration(context.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.With<CorrelationLogEventEnricher>()
                        .Enrich.With(services.GetRequiredService<TenantEnricher>())
                        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {TenantCode:u}] {Message:lj}{NewLine}{Exception}");
                })
                .UseConsoleLifetime()
                .Build();

        private static void ConfigureServices(HostBuilderContext hostingContext, IServiceCollection services)
        {
            // MediatR 
            services.AddMediatR(typeof(CreateTodoTaskHandler).Assembly);

            // Data
            services.AddTodoDataAccess();

            // Messaging
            services.AddMessageBus().AddNatsTransport(hostingContext.Configuration);

            services.AddMessagingHost(
                hostingContext.Configuration,
                hostBuilder => hostBuilder
                .Configure(configBuilder => configBuilder
                        .AddSubscriberServices(selector => selector
                            .FromMediatRHandledCommands().AddAllClasses())
                        .WithDefaultOptions()
                        .UsePipeline(builder => builder
                            .UseCorrelationMiddleware()
                            .UseTenantMiddleware()
                            .UseExceptionHandlingMiddleware()
                            .UseDefaultResiliencyMiddleware()
                            .UseMediatRMiddleware())
                    )
                );

            Log.Information("Messaging.Env=" + hostingContext.Configuration.GetSection("Messaging")["Env"]);

            // Multitenancy
            services.AddMultitenancy(hostingContext.Configuration)
                .AddMultiTenantMessaging()
                .AddDefaultMessagingTenantIdentification()
                .AddTenantRepository<ConfigurationTenantRepository>();

            services.AddSingleton<TenantEnricher>();
        }
    }
}
