// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NBB.Messaging.Abstractions;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;

namespace NBB.Messaging.MultiTenancy
{
    public class MultiTenancyMessageBusPublisherDecorator : IMessageBusPublisher
    {
        private readonly IMessageBusPublisher _inner;
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly IOptions<TenancyHostingOptions> _tenancyOptions;

        public MultiTenancyMessageBusPublisherDecorator(IMessageBusPublisher inner,
            ITenantContextAccessor tenantContextAccessor, IOptions<TenancyHostingOptions> tenancyOptions)
        {
            _inner = inner;
            _tenantContextAccessor = tenantContextAccessor;
            _tenancyOptions = tenancyOptions;
        }

        public async Task PublishAsync<T>(T message, MessagingPublisherOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var skip = _tenancyOptions.Value.TenancyType == TenancyType.MonoTenant ||
                options?.TopicName == DefaultDeadLetterQueue.ErrorTopicName;

            if (skip)
            {
                await _inner.PublishAsync(message, options, cancellationToken);
                return;
            }

            var tenantId = _tenantContextAccessor.TenantContext.GetTenantId();
            options ??= MessagingPublisherOptions.Default;

            void NewCustomizer(MessagingEnvelope outgoingEnvelope)
            {
                outgoingEnvelope.SetHeader(MessagingHeaders.TenantId, tenantId.ToString());
                options.EnvelopeCustomizer?.Invoke(outgoingEnvelope);
            }

            await _inner.PublishAsync(message, options with { EnvelopeCustomizer = NewCustomizer }, cancellationToken);
        }
    }
}
