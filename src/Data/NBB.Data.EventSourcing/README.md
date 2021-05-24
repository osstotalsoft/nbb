NBB.Data.EventSourcing
===============

This package aims to help you deal with data access when working with event sourced domain models.

NuGet install
----------------
```
dotnet add package NBB.Data.EventSourcing
```

Philosophy
----------------
It offers an `EventSourcedRepository` that:
* reads/persists events from/into an `IEventStore`
* manages snapshots using an `ISnapshotStore`
* dispatches events using `MediatR`

`EventSourcedRepository` offers two operations needed when working with ES domains:
```csharp
public interface IEventSourcedRepository<TAggregateRoot>
        where TAggregateRoot : IEventSourcedAggregateRoot, new()
{
    Task SaveAsync(TAggregateRoot aggregate, CancellationToken cancellationToken = default(CancellationToken));
    Task<TAggregateRoot> GetByIdAsync(object id, CancellationToken cancellationToken = default(CancellationToken));
}
```

For EventSore and SnapshotStore configuration see [`NBB.EventStore`](../../EventStore/#readme).

For more info about how to model event sourced snapshot-able entities see [`NBB.Domain`](../../Domain/#readme).


Loading domain aggregates
-------------
When loading domain aggregates, the repository uses the following algorithm:
* if the entity is snapshot-able it tries to load the last snapshot
* it loads the events from the event store (starting with last one from the snapshot, if any)
* it applies the events, re-hydrating the state of the aggregate, by  calling the `LoadFromHistory` instance method on the ES aggregate root

Saving domain aggregates
-------------
When saving domain aggregates, the repository uses the following algorithm:
* fetches uncommitted events from the aggregate
* saves the events in the event store, with the concurrency control set to aggregate-loaded-at-version
* if the entity is snapshot-able it checks if it should persist a snapshot
* dispatches events using `MediatR`


Stream identity
-------------
The process of mapping an event sourced entity to a stream, is a pure function like this:
```csharp
 public static string GetStream(this IIdentifiedEntity entity)
    => entity.GetType().FullName + ":" + entity.GetIdentityValue();
```

Container registration
-------------

You can register custom options for ES data access like this:

```csharp
services.AddEventSourcingDataAccess((sp, builder)=>builder.Options.DefaultSnapshotVersionFrequency = 5)
```

Register an ES repository for an aggregate like so:
```csharp
services.AddEventSourcedRepository<Contract>();
```

Sample usage
-------------
For a working sample see: [`NBB.Contracts sample`](../../../samples/MicroServices/NBB.Contracts/#readme)


