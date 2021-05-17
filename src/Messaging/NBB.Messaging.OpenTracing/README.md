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

The span is tagged with span kind **producer**, component **NBB.Messaging**

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