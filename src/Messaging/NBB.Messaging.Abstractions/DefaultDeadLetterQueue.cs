// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public class DefaultDeadLetterQueue : IDeadLetterQueue
    {
        public const string ErrorTopicName = "_error";

        private readonly IMessageBusPublisher _messageBusPublisher;
        private readonly IMessageSerDes _messageSerDes;
        private readonly ILogger<DefaultDeadLetterQueue> _logger;

        public DefaultDeadLetterQueue(IMessageBusPublisher messageBusPublisher, IMessageSerDes messageSerDes, ILogger<DefaultDeadLetterQueue> logger)
        {
            _messageBusPublisher = messageBusPublisher;
            _messageSerDes = messageSerDes;
            _logger = logger;
        }

        public void Push(MessagingEnvelope messageEnvelope, string topicName, Exception ex)
        {
            var payload = new
            {
                ExceptionType = ex.GetType(),
                ErrorMessage = ex.Message,
                ex.StackTrace,
                ex.Source,
                Data = messageEnvelope,
                OriginalTopic = topicName,
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

            // Fire and forget
            _ = _messageBusPublisher
                    .PublishAsync(payload, MessagingPublisherOptions.Default with { TopicName = ErrorTopicName }, default)
                    .ContinueWith(t => _logger.LogError(t.Exception, "Error publishing to dead letter queue"), TaskContinuationOptions.OnlyOnFaulted);

        }

        public void Push(TransportReceivedData messageData, string topicName, Exception ex)
        {
            switch (messageData)
            {
                case TransportReceivedData.EnvelopeBytes(var envelopeBytes):
                    Push(envelopeBytes, topicName, ex);
                    break;
                case TransportReceivedData.PayloadBytesAndHeaders(var payloadBytes, var headers):
                    Push(payloadBytes, headers, topicName, ex);
                    break;
                default:
                    throw new ArgumentException("Invalid transport received data", nameof(messageData));
            }
        }

        private void Push(byte[] messageData, string topicName, Exception ex)
        {
            try
            {
                // Headers only deserialization; payload will be deserialized to ExpandoObject
                var partialEnvelope = _messageSerDes.DeserializeMessageEnvelope(messageData);
                Push(partialEnvelope, topicName, ex);
                return;
            }
            catch { } // ignore partial deserialization exception

            var envelopeString = Encoding.UTF8.GetString(messageData);
            var payload = new
            {
                ExceptionType = ex.GetType(),
                Data = envelopeString,
                ErrorMessage = ex.Message,
                ex.StackTrace,
                ex.Source,
                OriginalTopic = topicName,
                OriginalSystem = string.Empty,
                PublishTime = DateTime.Now,
                MessageId = string.Empty
            };

            // Fire and forget
            _ = _messageBusPublisher
                    .PublishAsync(payload, MessagingPublisherOptions.Default with { TopicName = ErrorTopicName }, default)
                    .ContinueWith(t => _logger.LogError(t.Exception, "Error publishing to dead letter queue"), TaskContinuationOptions.OnlyOnFaulted);
        }

        private void Push(byte[] payloadData, IDictionary<string, string> headers, string topicName, Exception ex)
        {
            try
            {
                // Try untyped deserialization
                var untypedPayload = _messageSerDes.DeserializePayload<object>(payloadData, headers);
                Push(new MessagingEnvelope(headers, untypedPayload), topicName, ex);
                return;
            }
            catch { } // ignore untyped deserialization exception

            var payloadString = Encoding.UTF8.GetString(payloadData);

            Push(new MessagingEnvelope(headers, payloadString), topicName, ex);
        }
    }
}
