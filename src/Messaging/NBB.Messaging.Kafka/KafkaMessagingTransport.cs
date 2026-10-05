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
                GroupId = GroupName(topic, options),
                AutoOffsetReset = options.DeliverNewMessagesOnly ? AutoOffsetReset.Latest : AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false,
            }).Build();

    internal string GroupName(string topic, SubscriptionTransportOptions options) =>
        KafkaMessagingTransport.SanitizeTopic(
            options.UseGroup ? $"{kafkaOptions.Value.GroupId}__{topic}"
                             : $"{kafkaOptions.Value.GroupId}__{topic}__{Guid.NewGuid()}");
}

public class KafkaMessagingTransport(IProducer<byte[], byte[]> producer, KafkaConsumerFactory consumerFactory) : IMessagingTransport, ITransportMonitor
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
        var maxConcurrentMessages = Math.Max(1, subscriberOptions.MaxConcurrentMessages);
        var consumer = consumerFactory.Create(sanitizedTopic, subscriberOptions);
        // subscribe synchronously, before the poll loop starts: the consumer is positioned when SubscribeAsync returns
        consumer.Subscribe(sanitizedTopic);
        var cts = new CancellationTokenSource();
        //link the internal token to the caller token: caller cancellation cancels the poll loop and running handlers
        if (cancellationToken.CanBeCanceled)
            cancellationToken.Register(_ => cts.Cancel(), null);

        var t = Task.Run(async () =>
        {
            try
            {
                while (!cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                {
                    // collected outside the inner try: results already popped from the librdkafka buffer
                    // must be handled and committed even when a later Consume in the same batch throws
                    var results = new List<ConsumeResult<byte[], byte[]>>();
                    try
                    {
                        for (var i = 0; i < maxConcurrentMessages && !cancellationToken.IsCancellationRequested; i++)
                        {
                            var result = consumer.Consume(TimeSpan.FromMilliseconds(100));
                            if (result is null || result.IsPartitionEOF) break;
                            results.Add(result);
                        }
                    }
                    catch (ConsumeException e)
                    {
                        _ = Task.Run(() => OnError?.Invoke(e));
                        if (e.Error.IsFatal) break;
                    }

                    if (results.Count > 0)
                    {
                        // handle+commit is its own guard: a transient Commit throw or unexpected ForEachAsync
                        // fault is non-fatal (report and keep polling), unlike a fatal ConsumeException
                        try
                        {
                            await Parallel.ForEachAsync(results,
                                new ParallelOptions { MaxDegreeOfParallelism = maxConcurrentMessages, CancellationToken = cts.Token },
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

                            if (subscriberOptions.UseGroup)
                            {
                                var lastByPartition = new Dictionary<TopicPartition, ConsumeResult<byte[], byte[]>>();
                                foreach (var r in results)
                                    lastByPartition[r.TopicPartition] = r;
                                foreach (var last in lastByPartition.Values)
                                    consumer.Commit(last);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // cancellation is not a batch fault: let it reach the outer handling and end the poll task
                            throw;
                        }
                        catch (Exception e)
                        {
                            _ = Task.Run(() => OnError?.Invoke(e));
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                _ = Task.Run(() => OnError?.Invoke(e));
            }
        });

        var closed = false;
        return new SubscriptionDisposable(() =>
        {
            if (closed) return;
            closed = true;
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
