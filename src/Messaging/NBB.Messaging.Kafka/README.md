# Kafka transport

The *NBB.Messaging.Kafka* package implements the messaging transport on top of [Confluent Kafka .NET](https://github.com/confluentinc/confluent-kafka-dotnet), the official .NET binding for Kafka.

## NuGet install
```
dotnet add package NBB.Messaging.Kafka
```

## Sample usage
```csharp
services.AddMessageBus().AddKafkaTransport(configuration);
```

## Configuration

The transport requires a *Kafka* section inside the *Messaging* configuration section in *appsettings.json*:

```json
{
    "Messaging": {
        ...
        "Kafka": {
            "bootstrap_servers": "localhost:9092",
            "group_id": "nbb-kafka"
        }
    }
}
```

Settings:
- **bootstrap_servers** - comma-separated list of `host:port` pairs of one or more Kafka servers. Required: the DI wiring fails validation with `missing bootstrap_servers` when it is empty.
- **group_id** - base identifier of the Kafka consumer group used by subscriptions. Each subscription gets its own consumer group named `<group_id>__<sanitized topic>`, so subscriptions to different topics keep independent position state.

Note that topics are sanitized to the Kafka topic alphabet (`a-z A-Z 0-9 . - _`): any other character is replaced with `_`. The same sanitization is applied to the consumer-group name, so the same topic always maps to the same Kafka topic and consumer group.

## Transport options mapping

The transport maps the generic `SubscriptionTransportOptions` to Kafka concepts as follows:

| SubscriptionTransportOptions | Kafka equivalent |
|---|---|
| **DeliverNewMessagesOnly** | `AutoOffsetReset` of the consumer, set by the consumer factory: `Latest` when the flag is set, `Earliest` when it is not. Only relevant on first consumption for a consumer group; afterwards the persisted group offsets decide where consumption starts. |
| **MaxConcurrentMessages** | Batch size of the `Consume` collect-loop (up to N messages are pulled before invoking the handlers) plus the `MaxDegreeOfParallelism` of the `Parallel.ForEachAsync` that runs the handlers. |
| **IsDurable** / **AckWait** | No Kafka equivalent. Kafka consumer-group offsets persist per `group_id` (per topic-partition), so subscription position survives restarts for a stable group name; there is no per-subscription durability switch nor an ack timeout with redelivery. |

## Handler-failure tradeoff

Kafka has no per-message negative acknowledgement (unlike the JetStream NATS transports, where a failed handler leaves the message un-acked for redelivery). In this transport the batch commit still advances the partition offset past a message whose handler threw: the failure is reported to the `OnError` transport error handler and the message is *not* redelivered. If replaying failed messages matters, handle it at the application level (for example by re-publishing to a retry topic) rather than relying on the transport.

## Native runtime requirement

`Confluent.Kafka` binds to the native `librdkafka` shared library, so the transport needs a platform where librdkafka can be loaded at runtime (Windows/Linux with the standard binary dependencies available). This is why the full `IMessagingTransport` resolution is not exercised in the unit tests: see below.

## DI test deferral

The unit tests (`NBB.Messaging.Kafka.Tests`) resolve only the `KafkaConsumerFactory` service from the DI graph, because building the producer/consumer via `ProducerBuilder.Build()` loads the native librdkafka library. Full `IMessagingTransport` resolution through `AddKafkaTransport` is deferred to the integration tests (`test/Integration/NBB.Messaging.Kafka.IntegrationTests`), which run against a live Kafka server and are excluded from CI.
