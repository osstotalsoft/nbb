// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Confluent.Kafka;
using Microsoft.Extensions.Options;
using NBB.Messaging.Abstractions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Kafka;

public interface KafkaConsumerFactory
{
    IConsumer<byte[], byte[]> Create(string topic, SubscriptionTransportOptions options);
}

internal class KafkaConsumerFactoryImpl(IOptions<KafkaOptions> kafkaOptions) : KafkaConsumerFactory
{
    public IConsumer<byte[], byte[]> Create(string topic, SubscriptionTransportOptions options) =>
        new ConsumerBuilder<byte[], byte[]>(
            new ConsumerConfig
            {
                BootstrapServers = kafkaOptions.Value.BootstrapServers,
                GroupId = KafkaMessagingTransport.SanitizeTopic(kafkaOptions.Value.GroupId + "__" + topic),
                AutoOffsetReset = options.DeliverNewMessagesOnly ? AutoOffsetReset.Latest : AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false,
            }).Build();
}

public class KafkaMessagingTransport(IProducer<byte[], byte[]> producer, KafkaConsumerFactory consumerFactory,
    IOptions<KafkaOptions> kafkaOptions) : IMessagingTransport, ITransportMonitor
{
    public event TransportErrorHandler OnError;

    public async Task PublishAsync(string topic, TransportSendContext sendContext,
        CancellationToken cancellationToken = default)
    {
        var envelopeBytes = sendContext.EnvelopeBytesAccessor.Invoke();
        var headers = sendContext.HeadersAccessor.Invoke();
        var streamId = headers is null || !headers.TryGetValue(MessagingHeaders.StreamId, out var streamIdValue)
            ? null : streamIdValue;
        var keyBytes = streamId is null ? null : System.Text.Encoding.UTF8.GetBytes(streamId);

        await producer.ProduceAsync(SanitizeTopic(topic),
            new Message<byte[], byte[]> { Key = keyBytes, Value = envelopeBytes }, cancellationToken);
    }

    public Task<IDisposable> SubscribeAsync(string topic, Func<TransportReceiveContext, Task> handler,
        SubscriptionTransportOptions options = null, CancellationToken cancellationToken = default) =>
        throw new Exception("Kafka SubscribeAsync is implemented in Task 3");

    internal static string SanitizeTopic(string topic) =>
        string.Concat(topic.Select(c => IsKafkaTopicChar(c) ? c : '_'));

    internal static bool IsKafkaTopicChar(char c) =>
        c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '.' or '-' or '_';
}
