# NBB.EventStore.AdoNet

This package provides ADO.NET implementations for the *Event Repository* and *Snapshot Repository* abstractions.

## NuGet install
```
dotnet add package NBB.EventStore.AdoNet
```


## Usage

The repository implementations must be configured with one of available options:
* use options from configuration
```csharp
services.AddEventStore(b =>
{
    b.UseAdoNetEventRepository(o => o.FromConfiguration());
});
```
* use options from tenant configuration
```charp
services.AddEventStore(es =>
{
    es.UseAdoNetEventRepository(opts => opts.From<ITenantConfiguration>((c, o)
        => o.ConnectionString = c.GetConnectionString("EventStore")));
});
```


