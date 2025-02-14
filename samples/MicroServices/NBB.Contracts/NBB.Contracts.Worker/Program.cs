// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Contracts.Application;
using NBB.Contracts.Application.CommandHandlers;
using NBB.Contracts.ReadModel.Data;
using NBB.Contracts.WriteModel.Data;
using NBB.Correlation.Serilog;
using NBB.Domain;
using NBB.Messaging.Host;
using NBB.Messaging.OpenTelemetry;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Extensions.Propagators;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using NBB.Tools.Serilog.OpenTelemetryTracingSink;

namespace NBB.Contracts.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host
                .CreateDefaultBuilder(args)
                .UseSerilog((context, services, logConfig) =>
                {
                    logConfig
                        .ReadFrom.Configuration(context.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.With<CorrelationLogEventEnricher>()
                        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {TenantCode:u}] {Message:lj}{NewLine}{Exception}")
                        .WriteTo.OpenTelemetryTracing();
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ContractCommandHandlers>());

                    var transport = hostingContext.Configuration.GetValue("Messaging:Transport", "NATS");
                    if (transport.Equals("NATS", StringComparison.InvariantCultureIgnoreCase))
                    {
                        services
                            .AddMessageBus(hostingContext.Configuration)
                            .AddNatsTransport(hostingContext.Configuration)
                            .UseTopicResolutionBackwardCompatibility(hostingContext.Configuration);
                    }
                    else if (transport.Equals("Rusi", StringComparison.InvariantCultureIgnoreCase))
                    {
                        services
                            .AddMessageBus(hostingContext.Configuration)
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

                    services.AddMessagingHost(hostingContext.Configuration, hostBuilder => hostBuilder.UseStartup<MessagingHostStartup>());

                    var assembly = Assembly.GetExecutingAssembly().GetName();
                    void configureResource(ResourceBuilder r) =>
                        r.AddService(assembly.Name, serviceVersion: assembly.Version?.ToString(), serviceInstanceId: Environment.MachineName);

                    if (hostingContext.Configuration.GetValue<bool>("OpenTelemetry:TracingEnabled"))
                    {
                        Sdk.SetDefaultTextMapPropagator(new JaegerPropagator());

                        services.AddOpenTelemetry().WithTracing(builder => builder
                                .ConfigureResource(configureResource)
                                .SetSampler(new AlwaysOnSampler())
                                .AddMessageBusInstrumentation()
                                .AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
                                .AddOtlpExporter()
                        );
                        services.Configure<OtlpExporterOptions>(hostingContext.Configuration.GetSection("OpenTelemetry:Otlp"));
                    }

                    if (hostingContext.Configuration.GetValue<bool>("OpenTelemetry:MetricsEnabled"))
                    {
                        services.AddOpenTelemetry().WithMetrics(options =>
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
