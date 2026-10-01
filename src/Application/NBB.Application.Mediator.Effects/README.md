# NBB.Application.Mediator.Effects

Effects for sending requests and publishing notifications with the source generated [Mediator](https://github.com/martinothamar/Mediator) library.

## NuGet install
```
dotnet add package NBB.Application.Mediator.Effects
```

## Registration
```csharp
services.AddEffects();
services.AddMediatorEffects();
```

The side effect handlers are scoped: they resolve Mediator from the scope of the effect interpreter.

## Usage
```csharp
var client = MediatorEff.Send(new GetClient.Query(clientId));   // IRequest<T>, ICommand<T>, IQuery<T> => Effect<T>
var done = MediatorEff.Send(new CreateClient.Command(...));     // IRequest, ICommand => Effect<Unit>
var published = MediatorEff.Publish(new ClientCreated(...));    // INotification => Effect<Unit>
```

`NBB.Application.MediatR.Effects` exposes the same `MediatorEff.Send` / `MediatorEff.Publish` for MediatR, so effect code does not change when an application moves from MediatR to Mediator.
The side effect and handler types are in `MediatorEffects`.
