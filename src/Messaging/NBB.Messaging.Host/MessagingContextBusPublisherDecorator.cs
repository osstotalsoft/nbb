using NBB.Correlation;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host
{
    public class MessagingContextBusPublisherDecorator : IMessageBusPublisher
    {
        private readonly IMessageBusPublisher _inner;
        private readonly MessagingContextAccessor _messagingContextAccessor;

        public MessagingContextBusPublisherDecorator(IMessageBusPublisher inner, MessagingContextAccessor messagingContextAccessor)
        {
            _inner = inner;
            _messagingContextAccessor = messagingContextAccessor;
        }

        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default, Action<MessagingEnvelope> envelopeCustomizer = null, string topicName = null)
        {
            void NewEnvelopeCustomizer(MessagingEnvelope outgoingEnvelope)
            {
                var correlationId = CorrelationManager.GetCorrelationId();
                if (correlationId.HasValue)
                    outgoingEnvelope.SetHeader(MessagingHeaders.CorrelationId, correlationId.ToString(), false);

                var receivedMessageEnvelope = _messagingContextAccessor.MessagingContext?.ReceivedMessageEnvelope;
                if (receivedMessageEnvelope != null)
                {
                    receivedMessageEnvelope.TransferHeaderTo(outgoingEnvelope, MessagingHeaders.CorrelationId, false);
                    receivedMessageEnvelope.TransferCustomHeadersTo(outgoingEnvelope, false);
                }

                envelopeCustomizer?.Invoke(outgoingEnvelope);
            }

            return _inner.PublishAsync(message, cancellationToken, NewEnvelopeCustomizer, topicName);
        }
    }
}
