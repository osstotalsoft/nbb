using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.Messaging.DataContracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NBB.Correlation;

namespace NBB.Messaging.Abstractions
{
    public class MessageBusPublisher : IMessageBusPublisher
    {
        private readonly ITopicRegistry _topicRegistry;
        private readonly IMessageSerDes _messageSerDes;
        private readonly IConfiguration _configuration;
        private readonly IMessagingTopicPublisher _topicPublisher;
        public MessageBusPublisher(IMessagingTopicPublisher topicPublisher, ITopicRegistry topicRegistry,
            IMessageSerDes messageSerDes, IConfiguration configuration)
        {
            _topicPublisher = topicPublisher;
            _topicRegistry = topicRegistry;
            _messageSerDes = messageSerDes;
            _configuration = configuration;
        }

        public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default, Action<MessagingEnvelope> envelopeCustomizer = null, string topicName = null)
        {
            var outgoingEnvelope = PrepareMessageEnvelope(message, envelopeCustomizer);
            var key = (message as IKeyProvider)?.Key;
            var value = _messageSerDes.SerializeMessageEnvelope(outgoingEnvelope);
            var newTopicName = _topicRegistry.GetTopicForName(topicName) ??
                               _topicRegistry.GetTopicForMessageType(message.GetType());

            await _topicPublisher.PublishAsync(newTopicName, key, value, cancellationToken);

            await Task.Yield();
        }

        private  MessagingEnvelope<TMessage> PrepareMessageEnvelope<TMessage>(TMessage message, Action<MessagingEnvelope> customizer = null)
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

            outgoingEnvelope.SetHeader(MessagingHeaders.CorrelationId, (CorrelationManager.GetCorrelationId() ?? Guid.NewGuid()).ToString());

            return outgoingEnvelope;
        }

        private string GetSourceId()
        {
            var topicPrefix = _configuration.GetSection("Messaging")?["Source"];
            return topicPrefix ?? "";
        }
    }
}