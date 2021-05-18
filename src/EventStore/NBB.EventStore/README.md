# NBB.EventStore

This package provides an implementation of the *Event Store* abstractions as defined in package [`NBB.EventStore.Abstractions`](../NBB.EventStore.Abstractions).

The package also provides abstractions for the event and shapshot repositories.


## NuGet install
```
dotnet add package NBB.Eventstore
```

## Usage

The event store must be registered in the DI container.

```csharp
services.AddEventStore()
    .WithNewtownsoftJsonEventStoreSeserializer()
    ...
```


