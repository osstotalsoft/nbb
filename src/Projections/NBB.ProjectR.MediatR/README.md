# NBB.ProjectR.MediatR

Dispatches the [MediatR](https://github.com/jbogard/MediatR) notifications (events) that projectors subscribe to (`ISubscribeTo<...>`).

## NuGet install
```
dotnet add package NBB.ProjectR.MediatR
```

## Usage
```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<MyHandler>());
services
    .AddProjectR(typeof(MyProjector).Assembly)
    .AddProjectRMediatRHandlers();
services.AddMediatREffects();          // when projectors use MediatorEff
```
