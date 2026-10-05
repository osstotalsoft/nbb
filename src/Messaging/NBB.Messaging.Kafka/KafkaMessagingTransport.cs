// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Confluent.Kafka;
using Microsoft.Extensions.Options;
using NBB.Messaging.Abstractions;
using System;
using System.Collections.Generic;
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

    public async Task<IDisposable> SubscribeAsync(string topic, Func<TransportReceiveContext, Task> handler,
        SubscriptionTransportOptions options = null, CancellationToken cancellationToken = default)
    {
        var sanitizedTopic = SanitizeTopic(topic);
        var subscriberOptions = options ?? SubscriptionTransportOptions.Default;
        var consumer = consumerFactory.Create(sanitizedTopic, subscriberOptions);
        var cts = new CancellationTokenSource();

        var t = Task.Run(async () =>
        {
            try
            {
                consumer.Subscribe(sanitizedTopic);
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        var results = new List<ConsumeResult<byte[], byte[]>>();
                        for (var i = 0; i < subscriberOptions.MaxConcurrentMessages; i++)
                        {
                            var result = consumer.Consume(TimeSpan.FromMilliseconds(100));
                            if (result is null || result.IsPartitionEOF) break;
                            results.Add(result);
                        }

                        await Parallel.ForEachAsync(results,
                            new ParallelOptions { MaxDegreeOfParallelism = subscriberOptions.MaxConcurrentMessages, CancellationToken = cts.Token },
                            async (result, _) =>
                            {
                                try
                                {
                                    await handler(new TransportReceiveContext(new TransportReceivedData.EnvelopeBytes(result.Message.Value)));
                                }
                                catch (Exception e)
                                {
                                    // Kafka has no per-message NACK: the offset still advances (unlike JetStream explicit-ack)
                                    OnError?.Invoke(e);
                                }
                            });

                        var lastByPartition = new Dictionary<TopicPartition, ConsumeResult<byte[], byte[]>>();
                        foreach (var r in results)
                            lastByPartition[r.TopicPartition] = r;
                        foreach (var last in lastByPartition.Values)
                            consumer.Commit(last);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (ConsumeException e)
                    {
                        OnError?.Invoke(e);
                        if (e.Error.IsFatal) break;
                    }
                    catch (Exception e)
                    {
                        OnError?.Invoke(e);
                    }
                }
            }
            catch (OperationCanceledException) { }
        });

        return new SubscriptionDisposable(() =>
        {
            cts.Cancel();
            t.Wait();
            consumer.Close();
        });
    }

    internal static string SanitizeTopic(string topic) =>
        string.Concat(topic.Select(c => IsKafkaTopicChar(c) ? c : '_'));

    internal static bool IsKafkaTopicChar(char c) =>
        c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '.' or '-' or '_';
}

internal record SubscriptionDisposable(Action dispose) : IDisposable
{
    public void Dispose() => dispose();
}
