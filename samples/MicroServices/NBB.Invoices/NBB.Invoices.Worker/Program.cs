// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.Data.Abstractions;
using NBB.Correlation.Serilog;
using NBB.Domain;
using NBB.Domain.Abstractions;
using NBB.EventStore.Abstractions;
using NBB.Invoices.Data;
using NBB.Messaging.Host;
using Serilog;
using Serilog.Events;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Invoices.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host
                .CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, loggingBuilder) =>
                {
                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .Enrich.With<CorrelationLogEventEnricher>()
                        .WriteTo.Console()
                        .CreateLogger();

                    loggingBuilder.AddSerilog(dispose: true);
                    loggingBuilder.AddFilter("Microsoft", logLevel => logLevel >= LogLevel.Warning);
                    loggingBuilder.AddConsole();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services
                        .AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped)
                        .AddMediatorIntegration();

                    //Default transport is NATS. To opt in to Kafka, replace the line below with: services.AddMessageBus().AddKafkaTransport(hostingContext.Configuration);
                    services.AddMessageBus().AddNatsTransport(hostingContext.Configuration);
                    services.AddInvoicesWriteDataAccess();
                    services.AddEventStore(e =>
                    {
                        e.UseNewtownsoftJson(new SingleValueObjectConverter());
                        e.UseInMemoryEventRepository();
                    });

                    services.AddMessagingHost(
                        hostingContext.Configuration,
                        hostBuilder => hostBuilder
                        .Configure(configBuilder => configBuilder
                            .AddSubscriberServices(subscriberBuilder => subscriberBuilder
                                .FromMediatorHandledCommands().AddAllClasses()
                                .FromMediatorHandledEvents().AddAllClasses()
                            )
                            .WithDefaultOptions()
                            .UsePipeline(pipelineBuilder => pipelineBuilder
                                .UseCorrelationMiddleware()
                                .UseExceptionHandlingMiddleware()
                                .UseDefaultResiliencyMiddleware()
                                .UseMediatorMiddleware()
                            )
                        )
                    );

                    //services.AddSingleton<IHostedService, MessageBusSubscriberService<GetInvoice.Query>>();

                    services
                        .Decorate(typeof(IUow<>), typeof(DomainUowDecorator<>))
                        .Decorate(typeof(IUow<>), typeof(EventPublishingUowDecorator<>))
                        .Decorate(typeof(IUow<>), typeof(EventStoreUowDecorator<>));
                });

            await builder.RunConsoleAsync(CancellationToken.None);
        }
    }
}
