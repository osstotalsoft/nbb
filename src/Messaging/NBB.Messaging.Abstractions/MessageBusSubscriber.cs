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
        private readonly IDeadLetterQueue _deadLetterQueue;
        private readonly ILogger<MessageBusSubscriber> _logger;

        public MessageBusSubscriber(IMessagingTransport messagingTransport, ITopicRegistry topicRegistry,
            IMessageSerDes messageSerDes, IDeadLetterQueue deadLetterQueue,
            ILogger<MessageBusSubscriber> logger)
        {
            _messagingTransport = messagingTransport;
            _topicRegistry = topicRegistry;
            _messageSerDes = messageSerDes;
            _deadLetterQueue = deadLetterQueue;
            _logger = logger;
        }

        public async Task<IDisposable> SubscribeAsync<TMessage>(Func<MessagingEnvelope<TMessage>, Task> handler,
            MessagingSubscriberOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var topicNameWithoutPrefix = _topicRegistry.GetTopicForName(options?.TopicName, false) ??
                        _topicRegistry.GetTopicForMessageType(typeof(TMessage), false);

            var topicName = _topicRegistry.GetTopicForName(topicNameWithoutPrefix);

            void logSubscriberError(Exception exception) =>
                _logger.LogError("Messaging consumer encountered an error when handling a message from subject {Subject}.\n {Error}",
                        topicName, exception);

            async Task MsgHandler(TransportReceiveContext messageData)
            {
                _logger.LogDebug("Messaging subscriber received message from subject {Subject}", topicName);
                var messageEnvelope = messageData.EnvelopeAccessor.Invoke() as MessagingEnvelope<TMessage>;

                try
                {
                    await handler(messageEnvelope);
                }
                catch (Exception ex)
                {
                    logSubscriberError(ex);

                    if (messageEnvelope != null)
                        _deadLetterQueue.Push(messageEnvelope, topicNameWithoutPrefix, ex);
                }
            }

            TransportReceiveContext EnvelopeFactory(byte[] envelopeData)
            {
                MessagingEnvelope<TMessage> envelope = null;
                try
                {
                    envelope = _messageSerDes.DeserializeMessageEnvelope<TMessage>(envelopeData, options?.SerDes);
                }
                catch (Exception ex)
                {
                    logSubscriberError(ex);
                    _deadLetterQueue.Push(envelopeData, topicNameWithoutPrefix, ex);
                }
                return new TransportReceiveContext(EnvelopeAccessor: () => envelope);
            }

            TransportReceiveContext PayloadFactory(byte[] payloadData, IDictionary<string, string> headers)
            {
                TMessage payload = default;
                try
                {
                    payload = _messageSerDes.DeserializePayload<TMessage>(payloadData, headers, options?.SerDes);
                }
                catch (Exception ex)
                {
                    logSubscriberError(ex);
                    _deadLetterQueue.Push(payloadData, headers, topicNameWithoutPrefix, ex);
                }
                return new TransportReceiveContext(EnvelopeAccessor: () => new MessagingEnvelope<TMessage>(headers, payload));
            }

            var receiveContextFactory = new TransportReceiveContextFactory(FromEnvelopeBytes: EnvelopeFactory, FromPayloadBytesAndHeaders: PayloadFactory);
            return await _messagingTransport.SubscribeAsync(topicName, MsgHandler, receiveContextFactory, options?.Transport, cancellationToken);
        }
    }
}
