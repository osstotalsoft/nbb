# NBB.EventStore.AdoNet

This package provides ADO.NET implementations for the *Event Repository* and *Snapshot Repository* abstractions.

## NuGet install
```
dotnet add package NBB.EventStore.AdoNet
```


## Usage

The repository implementations must be registered in the DI container:

```csharp
services.AddEventStore()
    ...
    .WithAdoNetEventRepository();
```


