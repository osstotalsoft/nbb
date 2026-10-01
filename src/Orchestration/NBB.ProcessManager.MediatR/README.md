# NBB.ProcessManager.MediatR

Dispatches the [MediatR](https://github.com/jbogard/MediatR) notifications (events) handled by the NBB process manager definitions, and registers the MediatR effects (`NBB.Application.MediatR.Effects`) used by definitions.

## NuGet install
```
dotnet add package NBB.ProcessManager.MediatR
```

## Usage
```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<MyHandler>());
services
    .AddProcessManager(typeof(MyProcessManager).Assembly)
    .AddProcessManagerMediatRHandlers();
```

A MediatR `INotificationHandler<TEvent>` (`ProcessManagerNotificationHandler<TDefinition, TData, TEvent>`) is registered for every event of every (non obsolete) definition.
