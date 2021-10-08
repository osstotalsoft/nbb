// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBB.Application.MediatR;
using NBB.Core.Abstractions;
using NBB.Correlation.Serilog;
using NBB.Domain;
using NBB.Domain.Abstractions;
using NBB.EventStore.Abstractions;
using NBB.Messaging.Host;
using NBB.Payments.Application.CommandHandlers;
using NBB.Payments.Data;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Payments.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host
                .CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, loggingBuilder) =>
                {
                    var connectionString = hostingContext.Configuration.GetConnectionString("Logs");

                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
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
                    services.AddMediatR(typeof(PayPayableCommandHandler).Assembly);
                    //services.AddKafkaMessaging();
                    services.AddMessageBus().AddNatsTransport(hostingContext.Configuration);

                    services.AddPaymentsWriteDataAccess();

                    services.AddEventStore()
                        .WithNewtownsoftJsonEventStoreSeserializer(new[] {new SingleValueObjectConverter()})
                        .WithAdoNetEventRepository();

                    services.AddMessagingHost(
                        hostingContext.Configuration,
                        hostBuilder => hostBuilder
                        .Configure(configBuilder => configBuilder
                            .AddSubscriberServices(subscriberBuilder => subscriberBuilder
                                .FromMediatRHandledCommands().AddAllClasses()
                                .FromMediatRHandledEvents().AddAllClasses()
                            )
                            .WithDefaultOptions()
                            .UsePipeline(pipelineBuilder => pipelineBuilder
                                .UseCorrelationMiddleware()
                                .UseExceptionHandlingMiddleware()
                                .UseDefaultResiliencyMiddleware()
                                .UseMediatRMiddleware()
                            )
                        ));

                    services
                        .Decorate(typeof(IUow<>), typeof(DomainUowDecorator<>))
                        .Decorate(typeof(IUow<>), typeof(MediatorUowDecorator<>))
                        .Decorate(typeof(IUow<>), typeof(EventStoreUowDecorator<>));
                });

            await builder.RunConsoleAsync(CancellationToken.None);
        }
    }
}
