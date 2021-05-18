# NBB.Eventstore.InMemory

This package provides in-process implementations for the *Event Repository* and *Snapshot Repository* abstractions.

These implementations can be used as *test doubles* in integration tests.


## NuGet install
```
dotnet add package NBB.Eventstore.InMemory
```

## Usage

The repository implementations must be registered in the DI container:

```csharp
services.AddEventStore()
    ...
    .WithInMemoryEventRepository();
```

