// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBB.Contracts.Application.CommandHandlers;
using NBB.Contracts.ReadModel.Data;
using NBB.Contracts.WriteModel.Data;
using NBB.Correlation.Serilog;
using NBB.Domain;
using NBB.Messaging.Host;
using Serilog;
using Serilog.Events;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Sinks.MSSqlServer;
using System;

namespace NBB.Contracts.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host
                .CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, loggingBuilder) =>
                {
                    var env = hostingContext.HostingEnvironment.IsDevelopment();
                    var connectionString = hostingContext.Configuration.GetConnectionString("Logs");

                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .Enrich.With<CorrelationLogEventEnricher>()
                        .WriteTo.MSSqlServer(connectionString,
                            new MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true })
                        .CreateLogger();

                    loggingBuilder.AddSerilog(dispose: true);
                    loggingBuilder.AddFilter("Microsoft", logLevel => logLevel >= LogLevel.Warning);
                    loggingBuilder.AddConsole();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddMediatR(typeof(ContractCommandHandlers).Assembly);

                    var transport = hostingContext.Configuration.GetValue("Messaging:Transport", "NATS");
                    if (transport.Equals("NATS", StringComparison.InvariantCultureIgnoreCase))
                    {
                        services.AddMessageBus().AddNatsTransport(hostingContext.Configuration).UseTopicResolutionBackwardCompatibility(hostingContext.Configuration);
                    }
                    else if (transport.Equals("Rusi", StringComparison.InvariantCultureIgnoreCase))
                    {

                        services.AddMessageBus().AddRusiTransport(hostingContext.Configuration).UseTopicResolutionBackwardCompatibility(hostingContext.Configuration);
                    }
                    else
                    {
                        throw new Exception($"Messaging:Transport={transport} not supported");
                    }

                    services.AddContractsWriteModelDataAccess();
                    services.AddContractsReadModelDataAccess();


                    services.AddEventStore()
                        .WithNewtownsoftJsonEventStoreSeserializer(new[] { new SingleValueObjectConverter() })
                        .WithAdoNetEventRepository();

                    services.AddMessagingHost(hostingContext.Configuration, hostBuilder => hostBuilder.UseStartup<MessagingHostStartup>());

                });

            var host = builder.Build();

            await host.RunAsync();
        }
    }

    class MessagingHostStartup : IMessagingHostStartup
    {
        public Task Configure(IMessagingHostConfigurationBuilder hostConfigurationBuilder)
        {
            hostConfigurationBuilder
                .AddSubscriberServices(subscriberBuilder => subscriberBuilder
                    .FromMediatRHandledCommands().AddAllClasses())
                .WithOptions(optionsBuilder => optionsBuilder
                    .ConfigureTransport(transportOptions =>
                        transportOptions with { MaxConcurrentMessages = 2 }))
                .UsePipeline(pipelineBuilder => pipelineBuilder
                    .UseCorrelationMiddleware()
                    .UseExceptionHandlingMiddleware()
                    .UseDefaultResiliencyMiddleware()
                    .UseMediatRMiddleware()
                );

            return Task.CompletedTask;
        }
    }
}
