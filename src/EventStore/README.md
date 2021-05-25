# Event store

> An event is a collection of data that describes a change in state, e.g. an order has been placed.  
> An event store is just a database that stores events. 
> This is a pretty simple idea with some very powerful implications.

The package [`NBB.EventStore.Abstractions`](./NBB.EventStore.Abstractions#readme) contains *IEventStore* a lightweight abstraction over any EventStore client. 
It is meant to be consumed only by infrastructure packages in order to decouple from any EventStore implementation.


The package [`NBB.EventStore`](./NBB.EventStore#readme) offers *EventStore*, a modest implementation of the concept.

## Event repositories

The *EventStore* delegates persistence responsibilities to the *IEventRepository* implementations in order to decouple from any possible implementation.

### ADO.NET
The package [`NBB.EventStore.AdoNet`](./NBB.EventStore.AdoNet#AdoNet) offers an ADO.NET based implementation for *IEventRepository*.

The package [`NBB.EventStore.AdoNet.MultiTenancy`](./NBB.EventStore.AdoNet.MultiTenancy#readme) offers an ADO.NET based implementation for *IEventRepository* with multi-tenancy support.

The package [`NBB.EventStore.AdoNet.Migrations`](./NBB.EventStore.AdoNet.Migrations#readme) offers a migrations API for *AdoNetEventRepository*.

### In-memory
The package [`NBB.EventStore.InMemory`](./NBB.EventStore.InMemory#readme) offers an in-process implementation for *IEventRepository*.


## Registration

Example: register the event store using ADO.NET event repository and JSON serialization:

```csharp
services.AddEventStore()
    .WithNewtownsoftJsonEventStoreSeserializer()
    .WithAdoNetEventRepository();
```

The connection string used by the ADO.net repository is configured in section *EventStore.NBB.ConnectionString*

```json
{
  ...
  "EventStore": {
    "NBB": {
      "ConnectionString": "..."
    }
  }
}
```

Concurrency model
----------------
The event store uses the concurrency model defined at the IEventRepository level (at the storage level).

The *EfEventRepository* and *AdoNetEventRepository* offers optimistic concurrency based on the event's SequenceNumber.

If two threads or processes want to save to events with the same sequence number to same stream of events, the last will fail with a *ConcurrencyException*.

Snapshotting
----------------
The package [`NBB.EventStore.Abstractions`](./NBB.EventStore.Abstractions#readme) contains *ISnapshotStore* a lightweight abstraction for any snapshot store client.

The *SnapshotStore* delegates persistence responsibilities to the *ISnapshotRepository* implementations in order to decouple from any possible implementation.

The package  [`NBB.EventStore.AdoNet`](./NBB.EventStore.AdoNet#AdoNet) offers an Ado.Net based implementation for *ISnapshotRepository*.




