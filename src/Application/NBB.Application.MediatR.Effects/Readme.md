# NBB.Application.MediatR.Effects

This package provides effects for working with MediatR

## NuGet install
```
dotnet add package NBB.Application.MediatR.Effects
```

## Registration
You need to register the side-effects somewhere in the composition root, like so:

```csharp
services.AddMediatorEffects();
```

## Mediator.SendQuery
```csharp
var q1 = Mediator.SendQuery(new GetClientQuery());
```



