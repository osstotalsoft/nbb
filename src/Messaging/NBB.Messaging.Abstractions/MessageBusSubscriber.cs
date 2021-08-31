// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        private readonly IMessageBusPublisher _messageBusPublisher;

        public MessageBusSubscriber(IMessagingTransport messagingTransport, ITopicRegistry topicRegistry,
            IMessageSerDes messageSerDes,
            ILogger<MessageBusSubscriber> logger, IMessageBusPublisher messageBusPublisher)
        {
            _messagingTransport = messagingTransport;
            _topicRegistry = topicRegistry;
            _messageSerDes = messageSerDes;
            _logger = logger;
            _messageBusPublisher = messageBusPublisher;
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

                    var messageEnvelope = _messageSerDes.DeserializeMessageEnvelope(messageData);
                    var payload = new
                    {
                        ErrorMessage = ex.Message,
                        ex.StackTrace,
                        ex.Source,
                        Data = messageEnvelope,
                        OriginalTopic = _topicRegistry.GetTopicForName(options?.TopicName, false) ?? _topicRegistry.GetTopicForMessageType(typeof(TMessage), false),
                        OriginalSystem = messageEnvelope.Headers.TryGetValue(MessagingHeaders.Source, out var source) ? source : string.Empty,
                        CorrelationId = messageEnvelope.GetCorrelationId(),
                        MessageType = messageEnvelope.GetMessageTypeId(),
                        PublishTime = messageEnvelope.Headers.TryGetValue(MessagingHeaders.PublishTime, out var value)
                                                                         ? DateTime.TryParse(value, out var publishTime)
                                                                        ? publishTime
                                                                        : default
                                                                    : default,
                        MessageId = messageEnvelope.Headers.TryGetValue(MessagingHeaders.MessageId, out var messageId) ? messageId : string.Empty
                    };

                    //var envelope = new MessagingEnvelope(messageEnvelope.Headers, payload);
                    //var errorMessage = _messageSerDes.SerializeMessageEnvelope(envelope);

                    //var errorTopicName = _topicRegistry.GetTopicForName("_error");
                    await _messageBusPublisher.PublishAsync(payload, MessagingPublisherOptions.Default with { TopicName = "_error" }, cancellationToken);
                }
            }

            return await _messagingTransport.SubscribeAsync(topicName, MsgHandler, options?.Transport, cancellationToken);
        }
    }
}
