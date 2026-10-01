# NBB.ProjectR.Mediator

Dispatches the [Mediator](https://github.com/martinothamar/Mediator) notifications (events) that projectors subscribe to (`ISubscribeTo<...>`).

## NuGet install
```
dotnet add package NBB.ProjectR.Mediator
dotnet add package Mediator.SourceGenerator
```

## Usage
```csharp
services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
services
    .AddProjectR(typeof(MyProjector).Assembly)
    .AddProjectRMediatorHandlers();
services.AddMediatorEffects();        // when projectors use MediatorEff
```

## Requirements
* Mediator must be registered with `ServiceLifetime.Scoped` or `Transient`: the projection handlers depend on scoped services. The generic host fails at start otherwise.
* Events handled only by projectors have no generated handler, so the source generator reports warning `MSG0005` for them. It can be suppressed with `<NoWarn>$(NoWarn);MSG0005</NoWarn>`.
