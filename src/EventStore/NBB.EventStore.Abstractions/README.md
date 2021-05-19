# NBB.Eventstore.Abstractions

This package provides core abstractions for an event store

## NuGet install
```
dotnet add package NBB.Eventstore.Abstractions
```

## Abstractions

The following main abstractions are provided

## IEventStore

This is the main event store abstraction that specifies how events should be added or retrieved from the store:

```csharp
public interface IEventStore
{
    Task AppendEventsToStreamAsync(string stream, IEnumerable<object> events, int? expectedVersion, CancellationToken cancellationToken = default);
    Task<List<object>> GetEventsFromStreamAsync(string stream, int? startFromVersion, CancellationToken cancellationToken = default);
}
```

### ISnapshotStore

This abstraction enables [`snapshotting`](../#snapshotting) and specifies how snapshots are stored or loaded from the store

```csharp
public interface ISnapshotStore
{
    Task StoreSnapshotAsync(SnapshotEnvelope snapshotEnvelope, CancellationToken cancellationToken = default);
    Task<SnapshotEnvelope> LoadSnapshotAsync(string stream, CancellationToken cancellationToken = default);
}
```

If the provider does not support snapshotting, the *NullSnapshotStore* that is available in this package can be registered.


