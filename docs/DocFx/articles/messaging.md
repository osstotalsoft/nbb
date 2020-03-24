
Messaging
===============

When it comes to messaging systems we embrace the Microservices philosofy:
> smart endpoints dumb pipes


Althought NBB targets distributed systems, the messaging system is not a core building block, it is treated as an infrastructure detail.
It is your choise to make to use or not to use one, and what provider to choose. (Note. You may choose to integrate services via Http or Event Store Streams).

Messaging abstractions
----------------

The package *NBB.Messaging.Abstractions* contains some very lightweight abstractions over messaging concepts: IMessageBusPublisher and IMessageBusSubscriber
```csharp
public interface IMessageBusPublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default(CancellationToken), Action<MessagingEnvelope> envelopeCustomizer = null, string topicName = null)
}

public interface IMessageBusSubscriber
{
    Task SubscribeAsync(Func<MessagingEnvelope<TMessage>, Task> handler, CancellationToken token, string topicName = null);
    Task UnSubscribeAsync(Func<MessagingEnvelope<TMessage>, Task> handler, CancellationToken token);
}

```

Messaging data contracts
----------------
The package *NBB.Messaging.DataContracts* expose message related abstractions.

Kafka
----------------
![kafka](/images/kafka.png)

Out of the box, we are supporting Kafka with the package *NBB.Messaging.Kafka*, but in the future you should expect more implementations for well known messaging transports.
The implementation is based on Confluent's client apis.

In order to register Kafka as the default messaging:
```csharp
services.AddKafkaMessaging();
```

The topic resolution for messages is done by a simple interface *ITopicRegistry*. There is a simple implementation of this interface, *TopicRegistry* that resolves topic names from messages full type name.
To change this behavoiur you can use the *TopicNameResolverAttribute*, for ex:
```csharp
 [MessagingEventDescriptorTopicNameResolver]
public class MessagingEventDescriptor : SerializedMessage
{
  //code omitted for brevity

}

public class MessagingEventDescriptorTopicNameResolverAttribute : TopicNameResolverAttribute
{
    public override string ResolveTopicName(Type messageType, IConfiguration configuration)
    {
        var topicSufix = configuration.GetSection("EventStore")["TopicSufix"];
        var topic = "ch.eventStore." + topicSufix;

        return topic;
    }
}
```
Messaging host
----------------

The package *NBB.Messaging.Host* can be used to create background services (hosted services) for the message bus subscribers.

For each topic (denoted by the message type) a new background service is created (`MessageBusSubscriberService<TMessage>`). This service is used to host a message bus subscriber (`IMessageBusSubscriber<TMessage>`) for the topic and executes a *pipeline* for processing the incomming messages.

The `MessagingHostBuilder` can be used to register the subscriber services and to configure the message pipeline.


### Registration examples:

#### Using default options:
```csharp
services.AddMessagingHost()
    .AddSubscriberServices(config => config
        .FromMediatRHandledCommands().AddClassesAssignableTo<LeasingCommandBase>()
        .FromMediatRHandledEvents().AddClassesAssignableTo<DocumentSignerEventBase>())
    .WithDefaultOptions()
    .UsePipeline(builder => builder
        .UseCorrelationMiddleware()
        .UseExceptionHandlingMiddleware()
        .UseDefaultResiliencyMiddleware()
        .UseMediatRMiddleware());
```

#### Using custom options per message type and custom middleware:
```csharp
services.AddMessagingHost()
    .AddSubscriberServices(config => config.FromTopics(topics))
        .WithOptions(config => config.Options.SerDes.DeserializationType = DeserializationType.HeadersOnly)
    .AddSubscriberServices(config => config.FromMediatRHandledMessages().AddClassesWhere(x => x != typeof(Shutdown.Command)))
        .WithDefaultOptions()
    .AddSubscriberServices(config => config.AddType<Shutdown.Command>())
        .WithOptions(config => config.Options.ConsumerType = MessagingConsumerType.CollaborativeConsumer)
    .UsePipeline(builder => builder
        .UseCorrelationMiddleware()
        .UseExceptionHandlingMiddleware()
        .UseDefaultResiliencyMiddleware()
        .UseMiddleware<CustomMiddleware>()
        .UseMediatRMiddleware());
```

#### Using parallel handling on speciffic message type:
- Note: when using "parallel" HandlerStrategy you must also use "manual" AcknowledgeStrategy and increase the MaxInFlight value.
```csharp
    services.AddMessagingHost()
        .AddSubscriberServices(config => config.FromMediatRHandledCommands().AddClassesWhere(x => x != typeof(LongRunningIOCommand)))
            .WithDefaultOptions()
        .AddSubscriberServices(config => config.AddType<LongRunningIOCommand>())
            .WithOptions(config =>
            {
                config.Options.HandlerStrategy = MessagingHandlerStrategy.Parallel;
                config.Options.AcknowledgeStrategy = MessagingAcknowledgeStrategy.Manual;
                config.Options.MaxInFlight = 10;
            })
        .UsePipeline(pipelineBuilder => pipelineBuilder
            .UseCorrelationMiddleware()
            .UseExceptionHandlingMiddleware()
            .UseDefaultResiliencyMiddleware()
            .UseMediatRMiddleware()
        );
```
### Adding subscriber services:
`AddSubscriberServices(Action<ITypeSourceSelector> builder)` registers messagebus subscriber services for messages from the following sources:
The fluent API for configuration starts with specifying the sources of message types/topics
- Assemblies: scans the specified assemblies for message types.
   - `FromAssemblyOf<T>()`
   - `FromCallingAssembly()`
   - `FromExecutingAssembly()`
   - `FromEntryAssembly()`
   - `FromAssembliesOf(params Type[] types)`
   - `FromAssembliesOf(IEnumerable<Type> types)`
   - `FromAssemblies(params Assembly[] assemblies)`
   - `FromAssemblies(IEnumerable<Assembly> assemblies)`
- Topics: specifies the messaging topic(s) 
   - `FromTopic(string topic)`
   - `FromTopics(params string[] topics)`
   - `FromTopics(IEnumerable<string> topics)`
- Individual types:
   - `AddType<TMessage>()`
   - `AddTypes(params Type[] types)`
   - `AddTypes(IEnumerable<Type> types)`
- MediatR handled messages: finds MediatR handlers registerd in the IoC container and extracts the handled types:
   - `FromMediatRHandledEvents()`
   - `FromMediatRHandledCommands()`
   - `FromMediatRHandledQueries()`
   - `FromMediatRHandledMessages()` - includes all handled types (commands, events, queries)

For the Assembly and MediatR sources, the types should be selected using the following methods:
   - `AddClasses(bool publicOnly = true)` - selects all (public) types from the current source 
   - `AddClassesAssignableTo<TBase>(bool publicOnly = true)` - selects all (public) types from the current source that inherit/implement TBase
   - `AddClassesWhere(Func<Type, bool> predicate, bool publicOnly = true)` - selects all (public) types that 

###  Options for subscriber/consumer configuration:
When adding subscriberservices the options should be specified
- `WithDefaultOptions()` - use default options (see below for default values)
- `WithOptions(Action<MessagingSubscriberOptionsBuilder> subscriberOptionsBuilder)`

The following options that can be specified:
- `ConsumerType` Options for how the messages are consumed
  - `ConsumerType.CompetingConsumer` (default) Messages are consumed by a single consumer in a consumer group
  - `ConsumerType.CollaborativeConsumer` Messages are consumed by all consumers in a consumer group
- `AcknowledgeStrategy` Opions for message acknowledgment
  - `MessagingAcknowledgeStrategy.Serial` Acknowlegments are sent synchroniously after the message handler is called
  - `MessagingAcknowledgeStrategy.Auto` (default) Messages are acknowledged automatically by the transport library (eg: Nats/Kafka auto commit)
- `HandlerStrategy` Opions for message handling
  - `MessagingHandlerStrategy.Serial` (default) The handler is called sinchroniously for each message. The next message is consumed after the current handler is finished.
- `SerDes` Options for serialization/deserialization. See next section.

####  Options for serialization/deserialization:
- `DeserializationType` Options for message deserialization
  - `DeserializationType.TypeSafe` (default) Uses strongly typed deserialization
  - `DeserializationType.Dynamic` Uses type information included in the MessageType header to deserialize the message
  - `DeserializationType.HeadersOnly` Deserialize the headers but keep payload serialized
- `DynamicDeserializationScannedAssemblies` The list of assemblies to be scanned in case of the 'Dynamic' deserialization option.

###  Message pipeline configuration:
The message processing pipeline can be specified with the following method
- `UsePipeline(Action<IPipelineBuilder<MessagingEnvelope>> configurePipeline)`

The following pipeline configuration methods are available:
- `UseMiddleware<TMiddleware>()` - adds a middleware type to the message pipeline.
- `Use(Func<TMessage, CancellationToken, Func<Task>, Task> middleware)` - adds a middleware delegate defined in-line to the message pipeline.
- `UseExceptionHandlingMiddleware()` - basic exception handling and logging. This middleware swallows all exceptions.
- `UseCorrelationMiddleware` - handles the correlation id
- `UseDefaultResiliencyMiddleware()` - uses default resiliency policies (for out of order and concurrency exceptions)
- `UseMediatRMiddleware` - sends messages that are events, commands or queries to MediatR to be processed.



  
