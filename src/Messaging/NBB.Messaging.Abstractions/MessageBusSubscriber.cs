// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

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

            async Task<PipelineResult> MsgHandler(TransportReceiveContext receiveContext)
            {
                _logger.LogDebug("Messaging subscriber received message from subject {Subject}", topicName);

                MessagingEnvelope<TMessage> messageEnvelope = null;

                try
                {
                    messageEnvelope = receiveContext.ReceivedData switch
                    {
                        TransportReceivedData.EnvelopeBytes(var messsageBytes)
                            => _messageSerDes.DeserializeMessageEnvelope<TMessage>(messsageBytes, options?.SerDes),
                        TransportReceivedData.PayloadBytesAndHeaders(var payloadBytes, var headers)
                            => new MessagingEnvelope<TMessage>(headers, _messageSerDes.DeserializePayload<TMessage>(payloadBytes, headers, options?.SerDes)),
                        _ => throw new Exception("Invalid received message data")
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Deserialization error on topic {Subject}.", topicName);

                    if (messageEnvelope != null)
                        _deadLetterQueue.Push(messageEnvelope, topicName, ex);
                    else
                        _deadLetterQueue.Push(receiveContext.ReceivedData, topicName, ex);

                    return new PipelineResult(false, ex.Message);
                }

                try
                {
                    await handler(messageEnvelope);
                    return PipelineResult.SuccessResult;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Messaging subscriber encountered an error when handling a message from subject {Subject}.",
                       topicName);

                    return new PipelineResult(false, ex.Message);
                }
            }

            return await _messagingTransport.SubscribeAsync(topicName, MsgHandler, options?.Transport, cancellationToken);
        }
    }
}
