﻿using System;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBB.Application.DataContracts;
using NBB.Contracts.Application.CommandHandlers;
using NBB.Contracts.ReadModel.Data;
using NBB.Contracts.WriteModel.Data;
using NBB.Correlation.Serilog;
using NBB.Domain;
using NBB.EventStore;
using NBB.EventStore.AdoNet;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host;
using NBB.Messaging.Host.Builder;
using NBB.Messaging.Host.MessagingPipeline;
using NBB.Messaging.Nats;
using NBB.Resiliency;
using Serilog;
using Serilog.Events;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NBB.Contracts.Worker.MultiTenancy;
using NBB.Messaging.MultiTenancy;
using NBB.Messaging.MultiTenancy.MessagingPipeline;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Services;
using NBB.MultiTenancy.Identification.Extensions;
using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Messaging;
using NBB.MultiTenancy.Identification.Resolvers;
using NBB.MultiTenancy.Identification.Services;
using MessagingHeaders = NBB.Messaging.MultiTenancy.MessagingHeaders;

namespace NBB.Contracts.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        public static async Task MainAsync(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureHostConfiguration(config => { config.AddEnvironmentVariables("NETCORE_"); })
                .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
                {
                    configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
                    configurationBuilder.AddJsonFile("appsettings.json", true);
                    configurationBuilder.AddEnvironmentVariables();

                    if (hostBuilderContext.HostingEnvironment.IsDevelopment())
                    {
                        configurationBuilder.AddUserSecrets<Program>();
                    }
                })
                .ConfigureLogging((hostingContext, loggingBuilder) =>
                {
                    var connectionString = hostingContext.Configuration.GetConnectionString("Logs");

                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .Enrich.With<CorrelationLogEventEnricher>()
                        .WriteTo.MSSqlServer(connectionString, "Logs", autoCreateSqlTable: true)
                        .CreateLogger();

                    loggingBuilder.AddSerilog(dispose: true);
                    loggingBuilder.AddFilter("Microsoft", logLevel => logLevel >= LogLevel.Warning);
                    loggingBuilder.AddConsole();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddMediatR(typeof(ContractCommandHandlers).Assembly);

                    //services.AddKafkaMessaging();
                    services.AddNatsMessaging();

                    services.AddContractsWriteModelDataAccess();
                    services.AddContractsReadModelDataAccess();


                    services.AddEventStore()
                        .WithNewtownsoftJsonEventStoreSeserializer(new[] {new SingleValueObjectConverter()})
                        .WithAdoNetEventRepository();

                    services.AddResiliency();

                    services.AddMessagingHost()
                        .AddSubscriberServices(config =>
                            config.FromMediatRHandledCommands().AddClassesAssignableTo<Command>())
                        .WithOptions(config => config.Options.ConsumerType = MessagingConsumerType.CompetingConsumer)
                        .UsePipeline(pipelineBuilder => pipelineBuilder
                            .UseCorrelationMiddleware()
                            .UseExceptionHandlingMiddleware()
                            .UseTenantValidationMiddleware()
                            .UseDefaultResiliencyMiddleware()
                            .UseMediatRMiddleware()
                        );

                    var tenancyOptions = hostingContext.Configuration.GetSection("MultiTenancy").Get<TenancyOptions>();

                    services.AddMultiTenantMessaging();
                    services.AddSingleton<ITenantMessagingConfigService, TenantMessagingConfigService>();
                    services.Configure<TenancyOptions>(hostingContext.Configuration.GetSection("MultiTenancy"));

                    services.AddTenantIdentification();
                    switch (tenancyOptions.TenancyContextType)
                    {
                        case TenancyContextType.MultiTenant:
                            services.AddTenantIdentificationStrategy<IdTenantIdentifier>(config => config
                                .AddTenantTokenResolver<TenantIdHeaderMessagingTokenResolver>(MessagingHeaders.TenantId));
                            break;
                        case TenancyContextType.MonoTenant when tenancyOptions.MonoTenantId.HasValue:
                            services.AddTenantIdentificationStrategy<IdTenantIdentifier>(config => config
                                .AddTenantTokenResolver(new DefaultTenantTokenResolver(tenancyOptions.MonoTenantId.Value)));
                            break;
                        default:
                            throw new Exception("Invalid tenancy configuration");
                    }
                });

            await builder.RunConsoleAsync(default);
        }
    }
}
