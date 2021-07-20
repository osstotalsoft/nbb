# NBB.EventStore.Effects

This package provides event-store side effects and handlers for the NBB effects infrastructure.

## NuGet install
```
dotnet add package NBB.EventStore.Effects
```

## Registration
You need to register the side-effects somewhere in the composition root, like so:

```csharp
services.AddEventStoreEffects();
```

## Sample usage
```csharp
var eff = EventStore.GetEventsFromStream("stream1");

var eff2 = EventStore.AppendEventsToStream("stream2", new object[] { new SomethingHappened { Value = 1 } });

var eff3 =
    EventStore.GetEventsFromStream("stream1").Then(
        events => EventStore.AppendEventsToStream("stream2", events));

var eff4 =
    from events in EventStore.GetEventsFromStream("stream1")
    from _ in EventStore.AppendEventsToStream("stream2", events)
    select _;
```



