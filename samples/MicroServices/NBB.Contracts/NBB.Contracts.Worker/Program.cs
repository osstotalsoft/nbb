﻿using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBB.Contracts.Application.CommandHandlers;
using NBB.Contracts.ReadModel.Data;
using NBB.Contracts.WriteModel.Data;
using NBB.Correlation.Serilog;
using NBB.Domain;
using NBB.EventStore;
using NBB.EventStore.AdoNet;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host;
using NBB.Messaging.Host.MessagingPipeline;
using NBB.Messaging.Nats;
using Serilog;
using Serilog.Events;
using System.IO;
using System.Threading.Tasks;
using NBB.Contracts.Worker.MultiTenancy;
using NBB.Messaging.Host.Builder;
using NBB.Messaging.MultiTenancy;
using NBB.MultiTenancy.Abstractions.Hosting;
using NBB.MultiTenancy.Abstractions.Repositories;
using NBB.MultiTenancy.Identification.Messaging.Extensions;
using Serilog.Sinks.MSSqlServer;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;

namespace NBB.Contracts.Worker
{
    public class Program
    {
        public static async Task Main(string[] _args)
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
                        .WriteTo.MSSqlServer(connectionString,
                            new MSSqlServerSinkOptions {TableName = "Logs", AutoCreateSqlTable = true})
                        .CreateLogger();

                    loggingBuilder.AddSerilog(dispose: true);
                    loggingBuilder.AddFilter("Microsoft", logLevel => logLevel >= LogLevel.Warning);
                    loggingBuilder.AddConsole();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddMediatR(typeof(ContractCommandHandlers).Assembly);

                    services.AddMessageBus().AddNatsTransport(hostingContext.Configuration);

                    services.AddContractsWriteModelDataAccess();
                    services.AddContractsReadModelDataAccess();


                    services.AddEventStore()
                        .WithNewtownsoftJsonEventStoreSeserializer(new[] {new SingleValueObjectConverter()})
                        .WithAdoNetEventRepository();

                    services.AddMessagingHost(hostBuilder => hostBuilder.UseStartup<MessagingHostStartup>());

                    services.AddMultitenancy(hostingContext.Configuration, _ =>
                    {
                        services.AddMultiTenantMessaging()
                            .AddDefaultMessagingTenantIdentification()
                            .AddTenantRepository<TenantRepositoryMock>();
                    });
                });

            var host = builder.UseConsoleLifetime().Build();

            await host.RunAsync();
        }
    }

    class MessagingHostStartup : IMessagingHostStartup
    {
        private readonly IOptions<TenancyHostingOptions> _tenancyOptions;

        public MessagingHostStartup(IOptions<TenancyHostingOptions> tenancyOptions)
        {
            _tenancyOptions = tenancyOptions;
        }

        public Task Configure(IMessagingHostConfigurationBuilder hostConfigurationBuilder)
        {
            var isMultiTenant = _tenancyOptions?.Value?.TenancyType != TenancyType.None;

            hostConfigurationBuilder
                .AddSubscriberServices(subscriberBuilder => subscriberBuilder
                    .FromMediatRHandledCommands().AddAllClasses())
                .WithOptions(optionsBuilder => optionsBuilder
                    .ConfigureTransport(transportOptions =>
                        transportOptions with {MaxConcurrentMessages = 2}))
                .UsePipeline(pipelineBuilder => pipelineBuilder
                    .UseCorrelationMiddleware()
                    .UseExceptionHandlingMiddleware()
                    .When(isMultiTenant, x => x.UseTenantMiddleware())
                    .UseDefaultResiliencyMiddleware()
                    .UseMediatRMiddleware()
                );

            return Task.CompletedTask;
        }
    }
}