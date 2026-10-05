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
- **group_id** - base identifier of the Kafka consumer group used by subscriptions. Each subscription gets its own consumer group named `<group_id>__<sanitized topic>` (stable per topic, so subscriptions to different topics keep independent position state). With `UseGroup = false` the group name gets an extra per-subscription nonce suffix (`<group_id>__<sanitized topic>__<uuid>`), so each subscription starts fresh and its offsets are never persisted.

Note that topics are sanitized to the Kafka topic alphabet (`a-z A-Z 0-9 . - _`): any other character is replaced with `_`. The same sanitization is applied to the consumer-group name, so the same topic always maps to the same Kafka topic and consumer group.

## Transport options mapping

The transport maps the generic `SubscriptionTransportOptions` to Kafka concepts as follows:

| SubscriptionTransportOptions | Kafka equivalent |
|---|---|
| **DeliverNewMessagesOnly** | `AutoOffsetReset` of the consumer, set by the consumer factory: `Latest` when the flag is set, `Earliest` when it is not. Only relevant on first consumption for a consumer group; afterwards the persisted group offsets decide where consumption starts. |
| **MaxConcurrentMessages** | Batch size of the `Consume` collect-loop (up to N messages are pulled before invoking the handlers) plus the `MaxDegreeOfParallelism` of the `Parallel.ForEachAsync` that runs the handlers. Clamped to at least 1. A partition EOF (or a null result) breaks the collect-loop early, so a batch can be smaller than the budget. |
| **UseGroup** | With `true` (default) the subscription uses the stable `<group_id>__<topic>` consumer group and commits partition offsets after each batch, so position survives restarts. With `false` (`SubscriptionTransportOptions.RequestReply`) the consumer group gets a per-subscription nonce and no `Commit` is ever issued: ephemeral subscription semantics, nothing persists. |
| **IsDurable** / **AckWait** | No Kafka equivalent. Kafka consumer-group offsets persist per `group_id` (per topic-partition), so subscription position survives restarts for a stable group name; there is no per-subscription durability switch nor an ack timeout with redelivery. |

## Handler-failure tradeoff

Kafka has no per-message negative acknowledgement (unlike the JetStream NATS transports, where a failed handler leaves the message un-acked for redelivery). In this transport the batch commit still advances the partition offset past a message whose handler threw: the failure is reported to the `OnError` transport error handler and the message is *not* redelivered. If replaying failed messages matters, handle it at the application level (for example by re-publishing to a retry topic) rather than relying on the transport.

Two delivery-guarantee notes:
- **At-least-once within a batch**: the batch commit happens only after all handlers of the batch have run, so a crash between the handlers and the commit redelivers up to `MaxConcurrentMessages` already-handled messages.
- **Shared group offsets**: with `UseGroup = true` there is one consumer group per topic, so two *live* subscriptions to the same topic share the group's offsets and each commit moves the other's position. Keep at most one live subscription per topic per `group_id` (a restart of a stopped subscription then overlaps with, and may skip past, messages consumed by the live one). Use `UseGroup = false` for concurrent ephemeral subscriptions.

The consumer is subscribed synchronously inside `SubscribeAsync` (before the poll loop starts), so a message published right after `SubscribeAsync` returns is visible to the subscription even with `AutoOffsetReset.Latest`; the residual window is only the asynchronous assignment of the returned disposable.

## Native runtime requirement

`Confluent.Kafka` binds to the native `librdkafka` shared library, so the transport needs a platform where librdkafka can be loaded at runtime (Windows/Linux with the standard binary dependencies available). This is why the full `IMessagingTransport` resolution is not exercised in the unit tests: see below.

## DI test deferral

The unit tests (`NBB.Messaging.Kafka.Tests`) resolve only the `KafkaConsumerFactory` service from the DI graph, because building the producer/consumer via `ProducerBuilder.Build()` loads the native librdkafka library. Full `IMessagingTransport` resolution through `AddKafkaTransport` is deferred to the integration tests (`test/Integration/NBB.Messaging.Kafka.IntegrationTests`), which run against a live Kafka server and are excluded from CI.

## Running the integration tests locally

Verified procedure (Windows):

1. **Kafka server**: any local server works; the Apache image listens on `9092` by default, matching the test `appsettings.json`:
   ```
   docker pull apache/kafka:latest
   docker run -d --name nbb-kafka --publish 9092:9092 --publish 9093:9093 --env LISTEN_PORT=9092 --env EXTERNAL_HOST=localhost apache/kafka:latest
   ```
2. **librdkafka binaries**: `Confluent.Kafka` loads the native library via a `kafka-windows-x64-*` folder on `PATH`. Install once from the NuGet redist package (`librdkafka.redist`, version matching the pinned `Confluent.Kafka`): extract its `runtimes/win-x64/native` DLLs into `%LOCALAPPDATA%\librdkafka\kafka-windows-x64-v<version>\native` and prepend `%LOCALAPPDATA%\librdkafka` to `PATH` for the test run.
3. **Enable the facts**: the integration test `[Fact]` attributes are commented out by convention (CI never runs `test/Integration`); uncomment them for a local run.
4. **Run**:
   ```
   dotnet test test/Integration/NBB.Messaging.Kafka.IntegrationTests -c Debug
   ```

Test-design notes for the round-trip test: the message payload must be an object type (the Newtonsoft serdes deserializes envelope payloads as `JObject`, so scalar `String` payloads cannot round-trip); the topic must be unique per run (a stable consumer group persists committed offsets across runs, so a reused topic resumes past the new message); and `Dispose` must not run synchronously on the poll task's thread (completing a `TaskCompletionSource` from inside the handler resumes the awaiting test continuation on that thread, and `Dispose`'s join would deadlock it — `await Task.Yield()` before `Dispose` breaks the chain).
