using System;
using System.Threading;
using System.Threading.Tasks;
using NBB.Messaging.Abstractions;
using NBB.MultiTenancy.Abstractions.Context;

namespace NBB.Messaging.MultiTenancy
{
    public class MultiTenancyMessageBusPublisherDecorator : IMessageBusPublisher
    {
        private readonly IMessageBusPublisher _inner;
        private readonly ITenantContextAccessor _tenantContextAccessor;

        public MultiTenancyMessageBusPublisherDecorator(IMessageBusPublisher inner, ITenantContextAccessor tenantContextAccessor)
        {
            _inner = inner;
            _tenantContextAccessor = tenantContextAccessor;
        }

        public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default,
            Action<MessagingEnvelope> envelopeCustomizer = null,
            string topicName = null)
        {
            var tenantId = _tenantContextAccessor.TenantContext.GetTenantId();

            void NewCustomizer(MessagingEnvelope outgoingEnvelope)
            {
                outgoingEnvelope.SetHeader(MessagingHeaders.TenantId, tenantId.ToString());
                envelopeCustomizer?.Invoke(outgoingEnvelope);
            }

            await _inner.PublishAsync(message, cancellationToken, NewCustomizer, topicName);
        }
    }
}
