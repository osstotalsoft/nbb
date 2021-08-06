# NBB.EventStore.AdoNet.MultiTenancy

This package provides ADO.NET multi-tenant implementations for the *Event Repository* and *Snapshot Repository* abstractions.


## NuGet install
```
dotnet add package NBB.EventStore.AdoNet.MultiTenancy
```

## Usage

The repository implementations must be registered in the DI container:

```csharp
services.AddEventStore()
    ...
    .WithMultiTenantAdoNetEventRepository();
```



