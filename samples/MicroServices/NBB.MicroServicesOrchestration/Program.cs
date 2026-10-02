// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Messaging.Host;
using NBB.Messaging.OpenTelemetry;
using OpenTelemetry.Trace;
using System.Linq;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// generated types are internal because NBB.Mono references this host and generates its own Mediator
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.GenerateTypesAsInternal = true;
});

builder.Services.AddMessageBus().AddJetStreamTransport(builder.Configuration);

builder.Services.AddEventStore(es =>
{
    es.UseNewtownsoftJson();
    es.UseAdoNetEventRepository(o => o.FromConfiguration());
});

var integrationMessageAssemblies = new[] {
    typeof(NBB.Contracts.PublishedLanguage.ContractValidated).Assembly,
    typeof(NBB.Invoices.PublishedLanguage.InvoiceCreated).Assembly,
    typeof(NBB.Payments.PublishedLanguage.PayableCreated).Assembly,
};

builder.Services.AddMessagingHost(
    builder.Configuration,
    hostBuilder => hostBuilder
    .Configure(configBuilder => configBuilder
        .AddSubscriberServices(subscriberBuilder => subscriberBuilder
            .FromMediatorHandledEvents().AddClassesWhere(t => integrationMessageAssemblies.Contains(t.Assembly))
        )
        .WithDefaultOptions()
        .UsePipeline(pipelineBuilder => pipelineBuilder
            .UseCorrelationMiddleware()
            .UseExceptionHandlingMiddleware()
            .UseDefaultResiliencyMiddleware()
            .UseMediatorMiddleware()
        )
    ));

builder.Services
    .AddProcessManager(Assembly.GetEntryAssembly())
    .AddProcessManagerMediatorHandlers();

// Registered after the message bus: the instrumentation decorates the bus publisher and subscriber
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddMessageBusInstrumentation()
        .AddSqlClientInstrumentation());

var host = builder.Build();

await host.RunAsync();
