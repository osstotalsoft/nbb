// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NBB.Correlation;

namespace NBB.Messaging.Abstractions
{
    public class MessageBusPublisher : IMessageBusPublisher
    {
        private readonly ITopicRegistry _topicRegistry;
        private readonly IMessageSerDes _messageSerDes;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MessageBusPublisher> _logger;
        private readonly IMessagingTransport _messagingTransport;

        public MessageBusPublisher(IMessagingTransport messagingTransport, ITopicRegistry topicRegistry,
            IMessageSerDes messageSerDes, IConfiguration configuration, ILogger<MessageBusPublisher> logger)
        {
            _messagingTransport = messagingTransport;
            _topicRegistry = topicRegistry;
            _messageSerDes = messageSerDes;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task PublishAsync<T>(T message, MessagingPublisherOptions publisherOptions = null,
            CancellationToken cancellationToken = default)
        {
            var outgoingEnvelope = PrepareMessageEnvelope(message, publisherOptions?.EnvelopeCustomizer);
            var sendContext = new TransportSendContext(
                PayloadBytesAccessor: () => _messageSerDes.SerializePayload(outgoingEnvelope.Payload),
                EnvelopeBytesAccessor: () => _messageSerDes.SerializeMessageEnvelope(outgoingEnvelope),
                HeadersAccessor: () => outgoingEnvelope.Headers
            );

            var newTopicName = _topicRegistry.GetTopicForName(publisherOptions?.TopicName) ??
                               _topicRegistry.GetTopicForMessageType(message.GetType());

            await _messagingTransport.PublishAsync(newTopicName, sendContext, cancellationToken);

            _logger.LogDebug("Messaging publisher sent a message for subject {Subject}", newTopicName);
            await Task.Yield();
        }

        private MessagingEnvelope<TMessage> PrepareMessageEnvelope<TMessage>(TMessage message,
            Action<MessagingEnvelope> customizer = null)
        {
            var outgoingEnvelope = new MessagingEnvelope<TMessage>(new Dictionary<string, string>
            {
                [MessagingHeaders.MessageId] = Guid.NewGuid().ToString(),
                [MessagingHeaders.PublishTime] = DateTime.Now.ToString(CultureInfo.InvariantCulture)
            }, message);

            if (message is IKeyProvider messageKeyProvider)
            {
                outgoingEnvelope.Headers[MessagingHeaders.StreamId] = messageKeyProvider.Key;
            }

            var sourceId = GetSourceId();
            if (!string.IsNullOrWhiteSpace(sourceId))
            {
                outgoingEnvelope.Headers[MessagingHeaders.Source] = sourceId;
            }

            customizer?.Invoke(outgoingEnvelope);

            outgoingEnvelope.SetHeader(MessagingHeaders.CorrelationId,
                (CorrelationManager.GetCorrelationId() ?? Guid.NewGuid()).ToString());

            return outgoingEnvelope;
        }

        private string GetSourceId()
        {
            var topicPrefix = _configuration.GetSection("Messaging")?["Source"];
            return topicPrefix ?? "";
        }
    }
}
