# NBB.Application.MediatR

This package provides application extensions for Mediator

## NuGet install
```
dotnet add package NBB.Application.MediatR
```

## MediatorUowDecorator
`MediatorUowDecorator<TEntity>` is a decorator for the unit of work interface, that collects uncommitted events from evented entities in the unit of work and sends those events via Mediator.
You can register the decorator in the composition root like so:
```csharp
services.Decorate(typeof(IUow<>), typeof(MediatorUowDecorator<>));
```

