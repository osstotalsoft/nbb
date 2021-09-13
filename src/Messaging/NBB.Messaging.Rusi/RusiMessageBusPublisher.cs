// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBB.Core.Abstractions;
using NBB.Correlation;
using NBB.Messaging.Abstractions;
using Proto.V1;

namespace NBB.Messaging.Rusi
{
    public class RusiMessageBusPublisher : IMessageBusPublisher
    {
        private readonly Proto.V1.Rusi.RusiClient _client;
        private readonly IOptions<RusiOptions> _options;
        private readonly ITopicRegistry _topicRegistry;
        private readonly ILogger<RusiMessageBusPublisher> _logger;
        private readonly IMessageSerDes _messageSerDes;
        private readonly IConfiguration _configuration;

        public RusiMessageBusPublisher(Proto.V1.Rusi.RusiClient client,
            IOptions<RusiOptions> options,
            IMessageSerDes messageSerDes,
            ITopicRegistry topicRegistry,
            ILogger<RusiMessageBusPublisher> logger, IConfiguration configuration)
        {
            _client = client;
            _options = options;
            _messageSerDes = messageSerDes;
            _topicRegistry = topicRegistry;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task PublishAsync<T>(T message, MessagingPublisherOptions publisherOptions = null,
            CancellationToken cancellationToken = default)
        {
            var request = PreparePublishRequest(message, publisherOptions);
            await _client.PublishAsync(request);
            _logger.LogDebug("Messaging publisher sent a message for subject {Subject}", request.Topic);
        }

        private PublishRequest PreparePublishRequest<T>(T message, MessagingPublisherOptions publisherOptions)
        {
            var outgoingEnvelope = PrepareMessageEnvelope(message, publisherOptions?.EnvelopeCustomizer);
            var (payload, extraHeaders) = _messageSerDes.SerializePayload(outgoingEnvelope.Payload);

            if (extraHeaders != null)
                foreach (var (key, value) in extraHeaders)
                    outgoingEnvelope.SetHeader(key, value, true);

            var newTopicName = _topicRegistry.GetTopicForName(publisherOptions?.TopicName) ??
                               _topicRegistry.GetTopicForMessageType(message.GetType());

            return new PublishRequest()
            {
                PubsubName = _options.Value.PubsubName,
                Topic = newTopicName,
                Data = ByteString.CopyFrom(payload),
                Metadata = { outgoingEnvelope.Headers }
            };
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
