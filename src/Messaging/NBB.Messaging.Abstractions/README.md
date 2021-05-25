Messaging abstractions
==============

## NuGet install
```
dotnet add package NBB.Messaging.Abstractions
```

## Philosophy
The message bus is a high level api for messaging communication that abstracts away from its consumers some of the involving complexity like:
 - Messaging transport (Nats, Kafka, etc)
 - Message Serialization/Deserialization
 - Topic Registry
 - Message envelope

## Message bus registration
The services required by the message bus should be registered in the DI container. 

The message bus also requires a "Messaging" configuration section in *appsettings.json*

```json
  "Messaging": {
    "Env": "DEV",
    "Source": "Contracts.Worker",
    ...
  }
``` 

Settings:
- **Env** - name of the environment. If specified, it allows having multiple environments / deployments to use the same messaging infrastructure. It is added as a prefix in messaging topics so the topics will be partitioned by environment.
- **Source** - specifies the name of the service that publishes messages on the bus. It is recorded in the message headers.

The messaging transport should also be specified in the registration:

### Message bus with NATS transport:
Add a reference to *NBB.Messaging.Nats* package

```csharp
services.AddMessageBus().AddNatsTransport(Configuration);
```
Nats specific settings should be specified in *appsettings.json* in the *Messaging* section:


```json
{
  "Messaging": {
    ...
    "Nats": {
      "natsUrl": "YOUR_NATS_URL",
      "cluster": "faas-cluster",
      "clientId": "NBB_Samples",
      "qGroup": "NBB.Contracts.Worker",
      "durableName": "durable"
    }
  }
}
```

### Message bus with in-process transport:
Add a reference to *NBB.Messaging.InProcessMessaging* package

```csharp
services.AddMessageBus().AddInProcessTransport();
```

## Publish

To publish messages, you should resolve/inject from the DI container one of the following:
- **IMessageBusPublisher**
- **IMessageBus** (it also implements the IMessageBusPublisher interface)

### Publish a message with default options:
```javascript
await _messageBusPublisher.PublishAsync(command, cancellationToken);
```

The topic is determined from the message type using the policy defined in the [`Topic Registry`](#topic-registry).

Some standard message headers are added such as source, correlation id, message type, publish time, message id 
### Publish a message with custom options:
```javascript
await _messageBusPublisher.PublishAsync(command, new MessagingPublisherOptions {TopicName = "MyTopic", EnvelopeCustomizer = EnvelopeCustomizer}, cancellationToken);
```

#### Publisher Options: 
- **TopicName** - Override the topic name resolved from the [`Topic Registry`](#topic-registry)
- **EvelopeCustomizer** - A delegate that can modify the outgoing message envelope, usually for customizing message headers.


## Subscribe

To subscribe messages, you should resolve/inject from the DI container one of the following:
- **IMessageBusSubscriber**
- **IMessageBus** (it also implements the IMessageBusSubscriber interface)


### Subscribe to a message type with default options
```csharp
await _messageBusSubscriber.SubscribeAsync<TMessage>(HandleMsg, cancellationToken: cancellationToken);
```
The *HandleMsg* message handler is a function that receives the message envelope and processes it.

The topic is determined from the message type using the policy defined in the [`Topic Registry`](#topic-registry).

The default subscription is durable, within a consumer group, which handles one message at a time. 

### Subscribe to a message type with custom options

```csharp
var subscription = await _messageBus.SubscribeAsync<TMessage>(HandleMsg, MessagingSubscriberOptions.Default with { Transport = SubscriptionTransportOptions.PubSub }, cancellationToken);
```
#### Subscriber options
- **TopicName** -  The name of the topic to subscribe to (optional for typed subscriptions)
- **SerDes** - options regarding the serialization/deserialization of messages
  - **UseDynamicDeserialization** (default false) - see [`Dynamic deserialization`](#dynamic-deserialization)
  - **DynamicDeserializationScannedAssemblies** - the assemblies to scan in case UseDynamicDeserialization is used
- **Transport** - options for the messaging transport
  - **IsDurable** (default true) - Specifies if a durable subscription should be made
  - **UseGroup** (default true) - Specifies if a consumer group should be used
  - **MaxConcurrentMessages** (default 1) - Specifies the maximum number of messages that can be handled concurrently
  - **DeliverNewMessagesOnly** (default true) -  If set, only new messages are delivered. Otherwise all available messages are delivered even if published before the subscription.
  - **AckWait** (default 5000) - Timeout before message redelivery (in milliseconds)

Predefined combinations of transport options:
- **SubscriptionTransportOptions.StreamProcessor** - Stream processor transport options (with durable subscription, within consumer group, )
- **SubscriptionTransportOptions.RequestReply** - Request/Reply transport options (lightweight, non-durable, without consumer group)
- **SubscriptionTransportOptions.PubSub** - Publisher/Subscriber transport options (lightweight, non-durable)

**Note**: If the message type is not known and we don't use *DynamicDeserialization*, the message payload will be deserialized into an *ExpandoObject*


## Topic resolution
The topic name for publishing and subscribing to messages is determined by the policies defined in the *TopicRegistry*.

### Topic registry

The topic registry is a service that implements interface **ITopicRegistry*. There is a default implementation that is automatically registered in the DI container when registering the MessageBus, but it can also be replaced with a custom implementation.

The default topic resolution policy (based on a message type):
* Use the full type name (including namespace) in case a *TopicNameAttribute* is not specified
* Use the value of the *TopicNameAttribute* if it is present on the type
* In both cases add a prefix with the environment (as specified in the configuration section *Messaging.Env*)

The default topic resolution policy (based on a topic name):
* Use the given topic name prefixed with the environment (as specified in the configuration section *Messaging.Env*)

## Message serialization / deserialization
Serialization and deserialization is performed by a service that implements the interface *IMessageSerDes*. There is a default implementation that is automatically registered in the DI container when registering the MessageBus, but it can also be replaced with a custom implementation.

The default implementation uses JSON serialization for messages.

### Dynamic deserialization
When the message type is not known (or object) we have the option to deserialize the message payload according to the "message type" header in the envelope. The actual type is determined by scanning assemblies according to the policy in *MessageTypeRegistry*

If we don't use dynamic deserialization in this case the message payload will be deserialized into an *ExpandoObject*


## Message envelope
All messages transported on the bus are wrapped in a message envelope. It has the following sections
- Headers: a dictionary of string keys and values representing message metadata
- Payload: the data

### Standard headers
| Code                            | Value                  | Description                                                  |
| --------------------------------| ---------------------- | ------------------------------------------------------------ |
| MessagingHeaders.MessageType    | "nbb-messageType"      | The type name of the payload (can be used in dynamic deserialization)                                                        |
| MessagingHeaders.Source         | "nbb-source"           | The source service (where the message was published)                                                      |
| MessagingHeaders.CorrelationId  | "nbb-correlationId"    | A correlation ID for tracing the message                                                         |
| MessagingHeaders.MessageId      | "nbb-messageId"        | A unique identifier of the message (can be used for idempotency checks)                                                         |
| MessagingHeaders.PublishTime    | "nbb-publishTime"      | The date and time when the message was published.                                                         |
