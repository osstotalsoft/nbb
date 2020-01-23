using System;
using System.Threading;
using System.Threading.Tasks;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using NBB.MultiTenancy.Abstractions.Services;

namespace NBB.Messaging.MultiTenancy
{
    public class MultiTenancyMessageBusPublisherDecorator : IMessageBusPublisher
    {
        private readonly IMessageBusPublisher _inner;
        private readonly ITenantService _tenantService;

        public MultiTenancyMessageBusPublisherDecorator(IMessageBusPublisher inner, ITenantService tenantService)
        {
            _inner = inner;
            _tenantService = tenantService;
        }

        public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default,
            Action<MessagingEnvelope> envelopeCustomizer = null,
            string topicName = null)
        {
            var tenantId = await _tenantService.GetTenantIdAsync();

            void NewCustomizer(MessagingEnvelope outgoingEnvelope)
            {
                outgoingEnvelope.SetHeader(MessagingHeaders.TenantId, tenantId.ToString());
                envelopeCustomizer?.Invoke(outgoingEnvelope);
            }

            await _inner.PublishAsync(message, cancellationToken, NewCustomizer, topicName);
        }
    }
}
