// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Messaging.Host;
using NBB.Messaging.MultiTenancy;
using NBB.Messaging.OpenTelemetry;
using NBB.MultiTenancy.Abstractions.Repositories;
using NBB.Todos.Data;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddMessageBusInstrumentation()
        .AddSqlClientInstrumentation());

// Mediator
builder.Services
    .AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped)
    .AddMediatorIntegration(); // the contract classifier is required by the TenantMiddleware

// Data
builder.Services.AddTodoDataAccess();

// Messaging
builder.Services.AddMessageBus().AddJetStreamTransport(builder.Configuration);

builder.Services.AddMessagingHost(
    builder.Configuration,
    hostBuilder => hostBuilder
    .Configure(configBuilder => configBuilder
            .AddSubscriberServices(selector => selector
                .FromMediatorHandledCommands().AddAllClasses())
            .WithDefaultOptions()
            .UsePipeline(pipeline => pipeline
                .UseCorrelationMiddleware()
                .UseTenantMiddleware()
                .UseExceptionHandlingMiddleware()
                .UseDefaultResiliencyMiddleware()
                .UseMediatorMiddleware())
        )
    );

// Multitenancy
builder.Services.AddMultitenancy(builder.Configuration)
    .AddMultiTenantMessaging()
    .AddDefaultMessagingTenantIdentification()
    .AddTenantRepository<ConfigurationTenantRepository>();

var host = builder.Build();

await host.RunAsync();
