# NBB.Application.MediatR

This package provides application extensions for MediatR

## NuGet install
```
dotnet add package NBB.Application.MediatR
```

## Registration
```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<MyHandler>());
services.AddMediatRIntegration();
```

`AddMediatRIntegration()` registers the MediatR implementations of the NBB application ports (`NBB.Core.Abstractions`):
* `IEventPublisher` - `MediatREventPublisher`
* `IContractKindClassifier` - `MediatRContractKindClassifier`: `IRequest` - `Command`, `IRequest<T>` - `Query`, `INotification` - `Event`, anything else - `Other`

An application uses a single mediator library: registering the Mediator implementations (or another `IEventPublisher`) too throws.

## MediatREventPublisher
`MediatREventPublisher` implements `IEventPublisher` (`NBB.Core.Abstractions`): it publishes the events that are MediatR `INotification`s, sequentially, to the MediatR notification handlers.
It is used by the event sourced repositories (`NBB.Data.EventSourcing`) and by `EventPublishingUowDecorator<TEntity>` (`NBB.Data.Abstractions`).

```csharp
// publish the events of units of work after they are saved
services.Decorate(typeof(IUow<>), typeof(EventPublishingUowDecorator<>));
```

## MediatorUowDecorator (obsolete)
`MediatorUowDecorator<TEntity>` is obsolete and will be removed in NBB 11: use `EventPublishingUowDecorator<TEntity>` together with `AddMediatRIntegration()`.

