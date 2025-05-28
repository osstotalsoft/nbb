// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Confluent.Kafka;
using Microsoft.Extensions.Options;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Kafka.Internal;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Kafka
{
    public class KafkaMessagingTransport : IMessagingTransport, ITransportMonitor
    {
        private readonly KafkaConnectionProvider _connectionProvider;
        private readonly IOptions<KafkaOptions> _options;

        public KafkaMessagingTransport(KafkaConnectionProvider connectionProvider, IOptions<KafkaOptions> options)
        {
            _connectionProvider = connectionProvider;
            _options = options;
        }

        public event TransportErrorHandler OnError;

        public async Task PublishAsync(string topic, TransportSendContext sendContext, CancellationToken cancellationToken = default)
        {
            var producer = _connectionProvider.GetProducer();
            var envelopeData = sendContext.EnvelopeBytesAccessor.Invoke();
            await producer.ProduceAsync(topic, new Message<Null, byte[]> { Value = envelopeData }, cancellationToken);
        }

        public Task<IDisposable> SubscribeAsync(string topic, Func<TransportReceiveContext, Task> handler, SubscriptionTransportOptions options = null, CancellationToken token = default)
        {
            var consumer = _connectionProvider.GetConsumer();
            consumer.Subscribe(topic);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var t = Task.Run(async () =>
            {
                try
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var result = consumer.Consume(cts.Token);
                            if (result == null) continue;
                            var receiveContext = new TransportReceiveContext(new TransportReceivedData.EnvelopeBytes(result.Message.Value));
                            await handler(receiveContext);
                            consumer.Commit(result);
                        }
                        catch (ConsumeException ex)
                        {
                            OnError?.Invoke(ex);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex);
                }
            }, cts.Token);

            return Task.FromResult<IDisposable>(new SubscriptionDisposable(() =>
            {
                cts.Cancel();
                try { t.Wait(); } catch { }
                consumer.Close();
            }));
        }

        private record SubscriptionDisposable(Action dispose) : IDisposable
        {
            public void Dispose() => dispose();
        }
    }
}

