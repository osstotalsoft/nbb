# NBB.Eventstore.AdoNet.MultiTenancy

This package provides ADO.NET muti-tenant implementations for the *Event Repository* and *Snapshot Repository* abstractions.


## NuGet install
```
dotnet add package NBB.Eventstore.AdoNet.MultiTenancy
```

## Usage

The repository implementations must be registered in the DI container:

```csharp
services.AddEventStore()
    ...
    .WithMultiTenantAdoNetEventRepository();
```



