# NBB.Messaging.MediatR

Integrates the NBB messaging host with [MediatR](https://github.com/jbogard/MediatR):
* subscriber discovery from the MediatR handlers registered in the IoC container
* a pipeline middleware that dispatches the received messages to MediatR

The types keep the `NBB.Messaging.Host` namespace, so existing code only needs the package reference.

## NuGet install
```
dotnet add package NBB.Messaging.MediatR
```

## Usage
```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<MyHandler>());
services.AddMediatRIntegration(); // NBB.Application.MediatR: event publisher + contract classifier (TenantMiddleware, NBB 4 topics)

services.AddMessagingHost(
    Configuration,
    hostBuilder => hostBuilder
    .Configure(configBuilder => configBuilder
        .AddSubscriberServices(subscriberBuilder => subscriberBuilder
            .FromMediatRHandledCommands().AddAllClasses()
            .FromMediatRHandledEvents().AddAllClasses())
        .WithDefaultOptions()
        .UsePipeline(pipelineBuilder => pipelineBuilder
            .UseCorrelationMiddleware()
            .UseExceptionHandlingMiddleware()
            .UseDefaultResiliencyMiddleware()
            .UseMediatRMiddleware())));
```

## Subscriber discovery
- `FromMediatRHandledEvents()` - `INotificationHandler<T>`
- `FromMediatRHandledCommands()` - `IRequestHandler<T>`
- `FromMediatRHandledQueries()` - `IRequestHandler<T, R>`
- `FromMediatRHandledMessages()` - all of the above

## Dispatch middleware
`UseMediatRMiddleware()` publishes `INotification` payloads and sends `IRequest` payloads to MediatR; any other payload throws.

## Contract classification
`TenantMiddleware` and the NBB 4 topic resolution use the application's `IContractKindClassifier`, registered by `AddMediatRIntegration()` (see [`NBB.Application.MediatR`](../../Application/NBB.Application.MediatR#readme)).
