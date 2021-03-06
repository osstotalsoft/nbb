﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using NBB.Todos.Data;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Nats;
using NBB.Todo.Worker.Application;
using NBB.Messaging.Host;
using NBB.Messaging.Host.Builder;
using NBB.Messaging.Host.MessagingPipeline;
using NBB.MultiTenancy.Abstractions.Hosting;
using NBB.Messaging.MultiTenancy;
using NBB.MultiTenancy.Abstractions.Repositories;
using NBB.MultiTenancy.Identification.Messaging.Extensions;
using Serilog.Events;
using Microsoft.Extensions.Logging;

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
                .ConfigureHostConfiguration(config => config.AddEnvironmentVariables("NETCORE_"))
                .ConfigureLogging(ConfigureLogging)
                .ConfigureAppConfiguration(ConfigureApp)
                .ConfigureServices(ConfigureServices)
                .UseSerilog()
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

            services.AddMessagingHost(hostBuilder => hostBuilder
                .Configure(configBuilder => configBuilder
                        .AddSubscriberServices(selector => selector
                            .FromMediatRHandledCommands().AddAllClasses())
                        .WithDefaultOptions()
                        .UsePipeline(builder => builder
                            .UseCorrelationMiddleware()
                            .UseExceptionHandlingMiddleware()
                            .UseTenantMiddleware()
                            .UseDefaultResiliencyMiddleware()
                            .UseMediatRMiddleware())
                    )
                );

            Log.Information("Messaging.Env=" + hostingContext.Configuration.GetSection("Messaging")["Env"]);

            // Multitenancy
            services.AddMultitenancy(hostingContext.Configuration)
                .AddMultiTenantMessaging()
                .AddDefaultMessagingTenantIdentification()
                .AddTenantRepository<BasicTenantRepository>();
        }

        private static void ConfigureApp(HostBuilderContext ctx, IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "Production"}.json",
                    optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();


            if (ctx.HostingEnvironment.IsDevelopment())
            {
                configurationBuilder.AddUserSecrets<Program>();
            }
        }

        private static void ConfigureLogging(HostBuilderContext ctx, ILoggingBuilder _builder)
        {
            Log.Logger = new LoggerConfiguration()
              .ReadFrom.Configuration(ctx.Configuration)
              .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
              .Enrich.FromLogContext()
              .WriteTo.Console()
              .CreateLogger();
        }
    }
}
