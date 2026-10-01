# NBB.Messaging.Mediator

Integrates the NBB messaging host with the source generated [Mediator](https://github.com/martinothamar/Mediator) library:
* subscriber discovery from the Mediator handlers registered in the IoC container
* a pipeline middleware that dispatches the received messages to Mediator

The package references only `Mediator.Abstractions`. Add `Mediator.SourceGenerator` to the application (host) project, where `AddMediator` is called.

## NuGet install
```
dotnet add package NBB.Messaging.Mediator
dotnet add package Mediator.SourceGenerator
```

## Usage
```csharp
services
    .AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped)
    .AddMediatorIntegration(); // NBB.Application.Mediator: event publisher + contract classifier (TenantMiddleware, NBB 4 topics)

services.AddMessagingHost(
    Configuration,
    hostBuilder => hostBuilder
    .Configure(configBuilder => configBuilder
        .AddSubscriberServices(subscriberBuilder => subscriberBuilder
            .FromMediatorHandledCommands().AddAllClasses()
            .FromMediatorHandledEvents().AddAllClasses())
        .WithDefaultOptions()
        .UsePipeline(pipelineBuilder => pipelineBuilder
            .UseCorrelationMiddleware()
            .UseExceptionHandlingMiddleware()
            .UseDefaultResiliencyMiddleware()
            .UseMediatorMiddleware())));
```

## Subscriber discovery
The handled message types are read from the handler registrations made by the generated `AddMediator()`, so `AddMediator()` must be called before the messaging host starts.

| Method | Handler registrations |
|---|---|
| `FromMediatorHandledEvents()` | `INotificationHandler<T>` |
| `FromMediatorHandledCommands()` | `IRequestHandler<T, Unit>`, `ICommandHandler<T, Unit>` |
| `FromMediatorHandledQueries()` | `IQueryHandler<T, R>`, `IRequestHandler<T, R>` and `ICommandHandler<T, R>` with a response |
| `FromMediatorHandledMessages()` | all of the above |

Handlers registered manually as `INotificationHandler<T>` (e.g. the NBB process manager and ProjectR bridges) are discovered too.

## Dispatch middleware
`UseMediatorMiddleware()` publishes `INotification` payloads (`IPublisher.Publish(object)`) and sends request/command/query payloads (`ISender.Send(object)`); any other payload throws.
Mediator dispatches `object` messages only for message types known at compile time by the source generator.

## Contract classification
`TenantMiddleware` (`NBB.Messaging.MultiTenancy`) and the NBB 4 topic resolution (`NBB.Messaging.BackwardCompatibility`) need to know whether a message is a command, a query or an event.
They use the application's `IContractKindClassifier`, registered by `AddMediatorIntegration()` (see [`NBB.Application.Mediator`](../../Application/NBB.Application.Mediator#readme)).
