// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Core.Abstractions;
using NBB.Data.Abstractions;
using NBB.Domain;
using NBB.Domain.Abstractions;
using NBB.EventStore.Abstractions;
using NBB.Invoices.Data;
using NBB.Messaging.Host;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped)
    .AddMediatorIntegration();

builder.Services.AddMessageBus().AddJetStreamTransport(builder.Configuration);
builder.Services.AddInvoicesWriteDataAccess();
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

builder.Services
    .Decorate(typeof(IUow<>), typeof(DomainUowDecorator<>))
    .Decorate(typeof(IUow<>), typeof(EventPublishingUowDecorator<>))
    .Decorate(typeof(IUow<>), typeof(EventStoreUowDecorator<>));

var host = builder.Build();

await host.RunAsync();
