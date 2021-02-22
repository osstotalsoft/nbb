using NBB.Correlation;
using NBB.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host
{
    public class MessagingContextBusPublisherDecorator : IMessageBusPublisher
    {
        private readonly IMessageBusPublisher _inner;
        private readonly MessagingContextAccessor _messagingContextAccessor;

        public MessagingContextBusPublisherDecorator(IMessageBusPublisher inner,
            MessagingContextAccessor messagingContextAccessor)
        {
            _inner = inner;
            _messagingContextAccessor = messagingContextAccessor;
        }

        public Task PublishAsync<T>(T message, MessagingPublisherOptions options = null,
            CancellationToken cancellationToken = default)
        {
            options ??= MessagingPublisherOptions.Default;

            void NewEnvelopeCustomizer(MessagingEnvelope outgoingEnvelope)
            {
                var correlationId = CorrelationManager.GetCorrelationId();
                if (correlationId.HasValue)
                    outgoingEnvelope.SetHeader(MessagingHeaders.CorrelationId, correlationId.ToString());

                var messagingEnvelope = _messagingContextAccessor.MessagingContext?.MessagingEnvelope;
                if (messagingEnvelope != null)
                {
                    messagingEnvelope.TransferHeaderTo(outgoingEnvelope, MessagingHeaders.CorrelationId);
                    messagingEnvelope.TransferCustomHeadersTo(outgoingEnvelope);
                }

                options?.EnvelopeCustomizer?.Invoke(outgoingEnvelope);
            }

            return _inner.PublishAsync(message, options with {EnvelopeCustomizer = NewEnvelopeCustomizer},
                cancellationToken);
        }
    }
}