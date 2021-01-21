using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NBB.Messaging.Abstractions
{
    public class MessagingTopicSubscriber : IMessagingTopicSubscriber
    {
        private readonly IMessagingTransport _messagingTransport;
        private readonly ILogger<MessagingTopicSubscriber> _logger;
        private bool _subscribedToTopic;
        private readonly object _lockObj = new();

        public MessagingTopicSubscriber(IMessagingTransport messagingTransport,
            ILogger<MessagingTopicSubscriber> logger)
        {
            _messagingTransport = messagingTransport;
            _logger = logger;
        }

        public Task SubscribeAsync(string topic, Func<string, Task> handler,
            CancellationToken cancellationToken = default, MessagingSubscriberOptions options = null)
        {
            if (!_subscribedToTopic)
            {
                lock (_lockObj)
                {
                    if (!_subscribedToTopic)
                    {
                        _subscribedToTopic = true;
                        return SubscribeToTopicAsync(topic, handler, options, cancellationToken);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task UnSubscribeAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        private async Task SubscribeToTopicAsync(string topic, Func<string, Task> handler,
            MessagingSubscriberOptions options = null, CancellationToken cancellationToken = default)
        {
            var subscriberOptions = options ?? new MessagingSubscriberOptions();
            var transportOptions = SubscriptionTransportOptions.Default;
            transportOptions.UseGroup = true;
            transportOptions.IsDurable = true;
            transportOptions.MaxParallelMessages = subscriberOptions.MaxInFlight;
            transportOptions.UseBlockingHandler = subscriberOptions.HandlerStrategy == MessagingHandlerStrategy.Serial;
            transportOptions.UseManualAck =
                subscriberOptions.AcknowledgeStrategy == MessagingAcknowledgeStrategy.Manual;

            async Task MsgHandler(byte[] messageData)
            {
                _logger.LogDebug("Messaging subscriber received message from subject {Subject}", topic);

                var json = System.Text.Encoding.UTF8.GetString(messageData);

                try
                {
                    await handler(json);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        "Messaging consumer encountered an error when handling a message from subject {Subject}.\n {Error}",
                        topic, ex);
                }
            }

            var unused =
                await _messagingTransport.SubscribeAsync(topic, MsgHandler, transportOptions, cancellationToken);
        }
    }
}