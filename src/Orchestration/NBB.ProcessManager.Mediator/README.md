# NBB.ProcessManager.Mediator

Dispatches the [Mediator](https://github.com/martinothamar/Mediator) notifications (events) handled by the NBB process manager definitions, and registers the Mediator effects (`NBB.Application.Mediator.Effects`) used by definitions.

## NuGet install
```
dotnet add package NBB.ProcessManager.Mediator
dotnet add package Mediator.SourceGenerator
```

## Usage
```csharp
services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
services
    .AddProcessManager(typeof(MyProcessManager).Assembly)
    .AddProcessManagerMediatorHandlers();
```

A Mediator `INotificationHandler<TEvent>` is registered for every event of every (non obsolete) definition. The handlers are internal generic bridges registered in DI: the Mediator source generator ignores them and invokes them through the container.

## Requirements
* Mediator must be registered with `ServiceLifetime.Scoped` or `Transient`: the bridges depend on scoped services. The generic host fails at start otherwise.
* Events handled only by process managers have no generated handler, so the source generator reports warning `MSG0005` for them. It can be suppressed with `<NoWarn>$(NoWarn);MSG0005</NoWarn>`.
