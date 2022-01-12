// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

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

        public MultiTenancyMessageBusPublisherDecorator(IMessageBusPublisher inner,
            ITenantContextAccessor tenantContextAccessor)
        {
            _inner = inner;
            _tenantContextAccessor = tenantContextAccessor;
        }

        public async Task PublishAsync<T>(T message, MessagingPublisherOptions options = null,
            CancellationToken cancellationToken = default)
        {
            if (options?.TopicName == DefaultDeadLetterQueue.ErrorTopicName)
            {
                await _inner.PublishAsync(message, options , cancellationToken);
                return;
            }

            var tenantId = _tenantContextAccessor.TenantContext.GetTenantId();
            options ??= MessagingPublisherOptions.Default;

            void NewCustomizer(MessagingEnvelope outgoingEnvelope)
            {
                outgoingEnvelope.SetHeader(MessagingHeaders.TenantId, tenantId.ToString());
                options.EnvelopeCustomizer?.Invoke(outgoingEnvelope);
            }

            await _inner.PublishAsync(message, options with {EnvelopeCustomizer = NewCustomizer}, cancellationToken);
        }
    }
}
