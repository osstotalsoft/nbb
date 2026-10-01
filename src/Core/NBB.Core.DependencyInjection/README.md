# NBB.Core.DependencyInjection

Registration helpers for the Microsoft.Extensions.DependencyInjection container:
* constrained generic decorators, on top of [Scrutor](https://www.nuget.org/packages/Scrutor/)
* single-implementation registration for application-wide services
* registration requirements validated when the generic host starts

The extensions are in the `NBB.Core.DependencyInjection` namespace.

## NuGet install
```
dotnet add package NBB.Core.DependencyInjection
```

## DecorateOpenGenericWhen
Registers a constrained generic decorator for an open generic interface:
```csharp
public class DomainUowDecorator<TEntity> : IUow<TEntity>
        where TEntity : IEventedAggregateRoot
{...}

services.DecorateOpenGenericWhen(
    typeof(IUow<>), typeof(DomainUowDecorator<>),
    serviceType => typeof(IEventedAggregateRoot).IsAssignableFrom(
        serviceType.GetGenericArguments()[0]));
```

## AddSingleImplementation
Registers the single implementation of an application-wide service, e.g. a port implemented by one adapter per application:
```csharp
services.AddSingleImplementation<IEventPublisher, MediatorEventPublisher>(ServiceLifetime.Scoped,
    conflictHint: "An application uses a single mediator library.");
```
* no registration yet - the implementation is registered with the given lifetime
* the same implementation is already registered - no-op (safe to call more than once)
* a different implementation (or a factory registration) is already registered - throws `InvalidOperationException`; the optional `conflictHint` is appended to the message

## RequireRegistration / RequireNonSingletonLifetime
Declare requirements on registrations made elsewhere (e.g. by the application), validated when the generic host starts (`ValidateOnStart`).
The checks inspect the service collection, so registrations made after the call are taken into account; the last registration of a service (the one the container resolves) is checked.
```csharp
services
    .RequireRegistration(typeof(IMediator), "NBB.ProcessManager.Mediator", "call services.AddMediator(...)")
    .RequireNonSingletonLifetime(typeof(IMediator), "NBB.ProcessManager.Mediator", "call services.AddMediator(...)");
```
* `RequireRegistration` - the host fails to start if the service is not registered
* `RequireNonSingletonLifetime` - the host fails to start if the service is registered as Singleton (a missing registration is not reported, combine it with `RequireRegistration` when the service is mandatory)
