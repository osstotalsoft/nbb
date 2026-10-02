// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Contracts.Application;
using NBB.Contracts.ReadModel.Data;
using NBB.Contracts.WriteModel.Data;
using NBB.Domain;
using NBB.Messaging.Host;
using NBB.Messaging.OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped)
    .AddMediatorIntegration();

var transport = builder.Configuration.GetValue("Messaging:Transport", "JetStream");
if (transport.Equals("JetStream", StringComparison.InvariantCultureIgnoreCase))
{
    builder.Services
        .AddMessageBus()
        .AddJetStreamTransport(builder.Configuration)
        .UseTopicResolutionBackwardCompatibility(builder.Configuration);
}
else if (transport.Equals("Rusi", StringComparison.InvariantCultureIgnoreCase))
{
    builder.Services
        .AddMessageBus()
        .AddRusiTransport(builder.Configuration)
        .UseTopicResolutionBackwardCompatibility(builder.Configuration);
}
else
{
    throw new Exception($"Messaging:Transport={transport} not supported");
}

builder.Services.AddContractsWriteModelDataAccess();
builder.Services.AddContractsReadModelDataAccess();

builder.Services.AddEventStore(es =>
{
    es.UseNewtownsoftJson(new SingleValueObjectConverter());
    es.UseAdoNetEventRepository(o => o.FromConfiguration());
});

builder.Services.AddMessagingHost(
    builder.Configuration,
    hostBuilder => hostBuilder
    .Configure(configBuilder => configBuilder
        .AddSubscriberServices(subscriberBuilder => subscriberBuilder
            .FromMediatorHandledCommands().AddAllClasses())
        .WithOptions(optionsBuilder => optionsBuilder
            .ConfigureTransport(transportOptions => transportOptions with { MaxConcurrentMessages = 2 }))
        .UsePipeline(pipelineBuilder => pipelineBuilder
            .UseCorrelationMiddleware()
            .UseExceptionHandlingMiddleware()
            .UseDefaultResiliencyMiddleware()
            .UseMediatorMiddleware()
        )
    )
);

// Registered after the message bus: the instrumentation decorates the bus publisher and subscriber
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddMessageBusInstrumentation()
        .AddSqlClientInstrumentation())
    .WithMetrics(metrics => metrics
        .AddMeter(ContractDomainMetrics.InstrumentationName)
        .AddInstrumentation<ContractDomainMetrics>());

var host = builder.Build();

await host.RunAsync();
