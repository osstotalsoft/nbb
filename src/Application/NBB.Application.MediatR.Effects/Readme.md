# NBB.Application.MediatR.Effects

This package provides effects for working with MediatR

## NuGet install
```
dotnet add package NBB.Application.MediatR.Effects
```

## Registration
You need to register the side-effects somewhere in the composition root, like so:

```csharp
services.AddMediatREffects();
```

## MediatorEff.Send / MediatorEff.Publish
```csharp
var q1 = MediatorEff.Send(new GetClientQuery());
```

`AddMediatorEffects()` and the `Mediator` effects class are obsolete aliases of `AddMediatREffects()` / `MediatorEff`, and will be removed in NBB 11.

`NBB.Application.Mediator.Effects` exposes the same `MediatorEff.Send` / `MediatorEff.Publish` for the source generated Mediator library, so effect code does not change when an application moves to Mediator.
The side effect and handler types used by the effects are in `MediatorEffects`.



