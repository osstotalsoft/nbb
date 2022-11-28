// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBB.Contracts.Application;
using NBB.Contracts.Application.CommandHandlers;
using NBB.Contracts.ReadModel.Data;
using NBB.Contracts.WriteModel.Data;
using NBB.Correlation.Serilog;
using NBB.Domain;
using NBB.Messaging.Host;
using NBB.Messaging.OpenTracing;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Extensions.Propagators;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .Enrich.With<CorrelationLogEventEnricher>()
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
                        services
                            .AddMessageBus()
                            .AddNatsTransport(hostingContext.Configuration)
                            .UseTopicResolutionBackwardCompatibility(hostingContext.Configuration);
                    }
                    else if (transport.Equals("Rusi", StringComparison.InvariantCultureIgnoreCase))
                    {
                        services
                            .AddMessageBus()
                            .AddRusiTransport(hostingContext.Configuration)
                            .UseTopicResolutionBackwardCompatibility(hostingContext.Configuration);
                    }
                    else
                    {
                        throw new Exception($"Messaging:Transport={transport} not supported");
                    }

                    services.AddContractsWriteModelDataAccess();
                    services.AddContractsReadModelDataAccess();


                    services.AddEventStore(b =>
                    {
                        b.UseNewtownsoftJson(new SingleValueObjectConverter());
                        b.UseAdoNetEventRepository(o => o.FromConfiguration());
                    });

                    //services.Decorate<IMessageBusPublisher, OpenTracingPublisherDecorator>();
                    services.AddMessagingHost(hostingContext.Configuration, hostBuilder => hostBuilder.UseStartup<MessagingHostStartup>());

                    string serviceName = hostingContext.Configuration.GetValue<string>("OpenTelemetry:Jaeger:ServiceName");
                    Action<ResourceBuilder> configureResource =
                        r => r.AddService(serviceName, serviceVersion: "1.0", serviceInstanceId: Environment.MachineName);

                    if (hostingContext.Configuration.GetValue<bool>("OpenTelemetry:TracingEnabled"))
                    {
                        Sdk.SetDefaultTextMapPropagator(new JaegerPropagator());

                        services.AddOpenTelemetryTracing(builder => builder
                                .ConfigureResource(configureResource)
                                .AddSource(MessagingTags.ComponentMessaging)
                                .SetSampler(new AlwaysOnSampler())
                                .AddEntityFrameworkCoreInstrumentation()
                                .AddJaegerExporter()
                        );
                        services.Configure<JaegerExporterOptions>(hostingContext.Configuration.GetSection("OpenTelemetry:Jaeger"));
                    }

                    if (hostingContext.Configuration.GetValue<bool>("OpenTelemetry:MetricsEnabled"))
                    {
                        services.AddOpenTelemetryMetrics(options =>
                        {
                            options.ConfigureResource(configureResource)
                                .AddRuntimeInstrumentation()
                                .AddPrometheusHttpListener();
                            AddContractMetrics(options);
                        });
                    }
                    else
                    {
                        services.TryAddSingleton<ContractDomainMetrics>();
                    }
                });

            var host = builder.Build();

            await host.RunAsync();
        }

        public static MeterProviderBuilder AddContractMetrics(MeterProviderBuilder builder)
        {
            builder.AddMeter(ContractDomainMetrics.InstrumentationName);
            return builder.AddInstrumentation<ContractDomainMetrics>();
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
