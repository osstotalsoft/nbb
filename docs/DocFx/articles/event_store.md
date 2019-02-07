The event store
===============

> An event is a collection of data that describes a change in state, e.g. an order has been placed.  
> An event store is just a database that stores events. 
> This is a pretty simple idea with some very powerful implications.

![event-store](/images/event-store.png)

The package *NBB.EventStore.Abstractions* contains *IEventStore* a lightweight abstraction over any EventStore client. 
It is ment to be consumed only by infrastructure packages in order to decouple from any EventStore implementation.

```csharp
public interface IEventStore
{
    Task AppendEventsToStreamAsync(string stream, IEnumerable<ISequencedEvent> events, CancellationToken cancellationToken = default(CancellationToken));
    Task<List<ISequencedEvent>> GetEventsFromStreamAsync(string stream, CancellationToken cancellationToken = default(CancellationToken));
}
```

The package *NBB.EventStore* offers EventStore, a modest implementation of the concept.

Event repositories
----------------

The *EventStore* delegates persistence responsabilities to the *IEventRepository* implementations in order to decouple from any possible implementation.

The package *NBB.EventStore.EntityFramework* offers an Entity Framework Core based implementation for *IEventRepository*.

The package *NBB.EventStore.AdoNet* offers an Ado.Net based implementation for *IEventRepository*.

The package "NBB.EventStore.EfMigrations" offers migrations api for EfEventRepository.

Event streaming (for async domain event handlers)
----------------
![event-store-streaming](/images/event-store-streaming.jpeg)

The package "NBB.EventStore.MessagingExtensions" offers streaming capabilities for EventStore using the default configured Messaging implementation, if you have one.
Async domain event handlers have to implement the interface *INotificationHandler<EventStoreEnvelope<TEvent>>* in order to handle domain events async (in worker processes).

Registration
----------------
```csharp
services.AddEventStore(o =>
{
	o.AddAdoNetEventRepository();
	o.AddMessagingExtensions();
});
```


Concurency model
----------------
The event store uses the concurency model defined at the IEventRepository level (at the storage level).
The *EfEventRepository* and *AdoNetEventRepository* offers optimistic concurency based on the event's SequenceNumber.
If two threads or processes want to save to events with the same sequence number to same stream of events, the last will fail with a ConcurencyException.

Snapshotting
----------------
The package *NBB.EventStore.Abstractions* contains *ISnapshotStore* a lightweight abstraction for any snapshot store client.
```csharp
    public interface ISnapshotStore
    {
        Task StoreSnapshotAsync(SnapshotEnvelope snapshotEnvelope, CancellationToken cancellationToken = default);
        Task<SnapshotEnvelope> LoadSnapshotAsync(string stream, CancellationToken cancellationToken = default);
    }
```

The *SnapshotStore* delegates persistence responsabilities to the *ISnapshotRepository* implementations in order to decouple from any possible implementation.
The package *NBB.EventStore.AdoNet* offers an Ado.Net based implementation for *ISnapshotRepository*.


To be developed
----------------
* Event store filters - usefull for event upgrade

