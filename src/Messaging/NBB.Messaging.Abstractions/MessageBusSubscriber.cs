using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public class MessageBusSubscriber : IMessageBusSubscriber
    {
        private readonly IMessagingTransport _messagingTransport;
        private readonly ITopicRegistry _topicRegistry;
        private readonly IMessageSerDes _messageSerDes;
        private readonly ILogger<MessageBusSubscriber> _logger;

        public MessageBusSubscriber(IMessagingTransport messagingTransport, ITopicRegistry topicRegistry,
            IMessageSerDes messageSerDes,
            ILogger<MessageBusSubscriber> logger)
        {
            _messagingTransport = messagingTransport;
            _topicRegistry = topicRegistry;
            _messageSerDes = messageSerDes;
            _logger = logger;
        }

        public async Task<IDisposable> SubscribeAsync<TMessage>(Func<MessagingEnvelope<TMessage>, Task> handler,
            MessagingSubscriberOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var topicName = _topicRegistry.GetTopicForName(options?.TopicName) ??
                        _topicRegistry.GetTopicForMessageType(typeof(TMessage));

            async Task MsgHandler(byte[] messageData)
            {
                _logger.LogDebug("Messaging subscriber received message from subject {Subject}", topicName);

                try
                {
                    var messageEnvelope = _messageSerDes.DeserializeMessageEnvelope<TMessage>(messageData);

                    await handler(messageEnvelope);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        "Messaging consumer encountered an error when handling a message from subject {Subject}.\n {Error}",
                        topicName, ex);

                    //TODO: push to DLQ
                }
            }

            return await _messagingTransport.SubscribeAsync(topicName, MsgHandler, options?.Transport, cancellationToken);
        }
    }
}