// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.InProcessMessaging.Internal
{
    public class InProcessMessagingTransport : IMessagingTransport, ITransportMonitor
    {
        private readonly IStorage _storage;
        private readonly ILogger<InProcessMessagingTransport> _logger;

        public InProcessMessagingTransport(IStorage storage, ILogger<InProcessMessagingTransport> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public event TransportErrorHandler OnError;

        public async Task<IDisposable> SubscribeAsync(string topic, Func<TransportReceiveContext, Task<PipelineResult>> handler,
            SubscriptionTransportOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var sub = await _storage.AddSubscription(topic, async msg =>
            {
                try
                {
                    var receiveConetxt = new TransportReceiveContext(new TransportReceivedData.EnvelopeBytes(msg));

                    await handler(receiveConetxt);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "InProcessMessagingTopicSubscriber encountered an error when handling a message from topic {TopicName}.", topic);

                    OnError?.Invoke(ex);

                    //TODO: push to DLQ
                }
            }, cancellationToken);

            //_logger.LogInformation("InProcessMessagingTopicSubscriber has subscribed to topic {Topic}", topic);

            return sub;
        }

        public Task PublishAsync(string topic, TransportSendContext sendContext, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var message = sendContext.EnvelopeBytesAccessor();
            _storage.Enqueue(message, topic);
            stopWatch.Stop();

            _logger.LogDebug("InProcess message published to topic {Topic} in {ElapsedMilliseconds} ms", topic,
                stopWatch.ElapsedMilliseconds);

            return Task.CompletedTask;
        }
    }
}
