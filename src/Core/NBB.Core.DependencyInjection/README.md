# NBB.Core.DependencyInjection

This package is based on [Scrutor](https://www.nuget.org/packages/Scrutor/) which allows registering decorators in ASP.Net DI containers.
Currently it adds just one functionality on top of Scrutor, it allows one to register constrained generic decorators for open generic interfaces.

## DecorateOpenGenericWhen
```csharp
public class DomainUowDecorator<TEntity> : IUow<TEntity>
        where TEntity : IEventedAggregateRoot
{...}

services.DecorateOpenGenericWhen(
    typeof(IUow<>), typeof(DomainUowDecorator<>),
    serviceType => typeof(IEventedAggregateRoot).IsAssignableFrom(
        serviceType.GetGenericArguments()[0]));
```

As this is the only functionality of this package, we should try to make a pull request in Scrutor with this functionality and Obsolete this one.

## NuGet install
```
dotnet add package NBB.Core.Abstractions
```


