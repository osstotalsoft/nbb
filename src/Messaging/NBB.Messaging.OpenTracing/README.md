# NBB.Messaging.OpenTracing

This package provides open tracing support in the messaging publishers and subscribers.


## NuGet install
```
dotnet add package NBB.Messaging.OpenTracing
```

## Publisher
To enable creating a publisher span, register the following publisher decorator in DI.

```csharp
services.Decorate<IMessageBusPublisher, OpenTracingPublisherDecorator>();
```

The span is tagged with **span.kind** = "producer", **component** = "NBB.Messaging", **message_bus.destination** = the topic for the published message, **nbb.correlation_id** = the current correlation ID

## Subscriber 
To enable creating a subscriber span, add the following middleware in the messaging host pipeline:

```csharp
.UsePipeline(pipelineBuilder =>
{
    pipelineBuilder
        ...
        .UseOpenTracing()
        ...
});
```

This span relates to the publisher span  using a reference of type "follows_from". The following tags are associated with the span: **span.kind** = "consumer", **component** = "NBB.Messaging", **peer.service** = the name of the service that published the message , **nbb.correlation_id** = the current correlation ID
