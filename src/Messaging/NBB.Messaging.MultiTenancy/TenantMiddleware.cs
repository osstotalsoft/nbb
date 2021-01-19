using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Repositories;
using NBB.MultiTenancy.Identification.Services;

namespace NBB.Messaging.MultiTenancy
{
    /// <summary>
    /// A pipeline middleware that checks if the tenant received in messages is the same as the tenant 
    /// obtained from the current identification strategy and builds the tenant context.
    /// </summary>
    /// <seealso cref="IPipelineMiddleware{MessagingEnvelope}" />
    public class TenantMiddleware : IPipelineMiddleware<MessagingEnvelope>
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly ITenantIdentificationService _tenantIdentificationService;
        private readonly IOptions<TenancyHostingOptions> _tenancyOptions;
        private readonly ITenantRepository _tenantRepository;

        public TenantMiddleware(ITenantContextAccessor tenantContextAccessor, ITenantIdentificationService tenantIdentificationService, IOptions<TenancyHostingOptions> tenancyOptions, ITenantRepository tenantRepository)
        {
            _tenantContextAccessor = tenantContextAccessor;
            _tenantIdentificationService = tenantIdentificationService;
            _tenancyOptions = tenancyOptions;
            _tenantRepository = tenantRepository;
        }

        public async Task Invoke(MessagingEnvelope message, CancellationToken cancellationToken, Func<Task> next)
        {
            if (_tenancyOptions.Value.TenancyType == TenancyType.None)
            {
                await next();
                return;
            }

            var tenantId = await _tenantIdentificationService.GetTenantIdAsync();

            if (!message.Headers.TryGetValue(MessagingHeaders.TenantId, out var messageTenantIdHeader))
            {
                throw new ApplicationException($"The tenant ID message header is missing from the message envelope");
            }

            if (!Guid.TryParse(messageTenantIdHeader, out var messageTenantId))
            {
                throw new ApplicationException($"The tenant ID message header is invalid");
            }

            if (messageTenantId != tenantId)
            {
                throw new ApplicationException(
                    $"Invalid tenant ID for message {message.Payload.GetType()}. Expected {tenantId} but received {messageTenantIdHeader}");
            }

            if (_tenancyOptions.Value.TenancyType == TenancyType.MonoTenant && _tenancyOptions.Value.TenantId != messageTenantId)
            {
                throw new ApplicationException(
                    $"Invalid tenant ID for message {message.Payload.GetType()}. Expected {_tenancyOptions.Value.TenantId} but received {messageTenantIdHeader}");
            }

            var tenant = await _tenantRepository.Get(tenantId, cancellationToken)
                         ?? throw new ApplicationException($"Tenant {tenantId} not found");

            if (tenant.IsShared && _tenancyOptions.Value.TenancyType == TenancyType.MonoTenant)
            {
                throw new ApplicationException(
                    $"Received a message for shared tenant {messageTenantIdHeader} in a MonoTenant hosting");
            }

            if (!tenant.IsShared && _tenancyOptions.Value.TenancyType == TenancyType.MultiTenant)
            {
                throw new ApplicationException(
                    $"Received a message for premium tenant {messageTenantIdHeader} in a MultiTenant (shared) context");
            }

            if (_tenantContextAccessor.TenantContext != null)
            {
                throw new ApplicationException("Tenant context is already set");
            }

            _tenantContextAccessor.TenantContext = new TenantContext(tenant);

            await next();
        }
    }

    public static partial class MessagingPipelineExtensions
    {
        public static IPipelineBuilder<MessagingEnvelope> UseTenantMiddleware(this IPipelineBuilder<MessagingEnvelope> pipelineBuilder)
            => pipelineBuilder.UseMiddleware<TenantMiddleware, MessagingEnvelope>();
    }
}
