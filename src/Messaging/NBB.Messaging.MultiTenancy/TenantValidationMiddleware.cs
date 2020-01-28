using NBB.Core.Pipeline;
using NBB.Messaging.DataContracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Services;

namespace NBB.Messaging.MultiTenancy
{
    /// <summary>
    /// A pipeline middleware that checks if the tenant received in messages is the same as the tenant
    /// expected in the current context.
    /// </summary>
    /// <seealso cref="IPipelineMiddleware{MessagingEnvelope}" />
    public class TenantValidationMiddleware : IPipelineMiddleware<MessagingEnvelope>
    {
        private readonly ITenantService _tenantService;
        private readonly ITenantMessagingConfigService _tenantMessagingConfigService;
        private readonly IOptions<TenancyOptions> _tenancyOptions;

        public TenantValidationMiddleware(ITenantService tenantService, ITenantMessagingConfigService tenantMessagingConfigService, IOptions<TenancyOptions> tenancyOptions)
        {
            _tenantService = tenantService;
            _tenantMessagingConfigService = tenantMessagingConfigService;
            _tenancyOptions = tenancyOptions;

            CheckMonoTenant();
        }

        public async Task Invoke(MessagingEnvelope message, CancellationToken cancellationToken, Func<Task> next)
        {
            var contextTenantId = await _tenantService.GetTenantIdAsync();
            if (message.Headers.TryGetValue(MessagingHeaders.TenantId, out var messageTenantIdHeader))
            {
                if (!Guid.TryParse(messageTenantIdHeader, out var messageTenantId))
                {
                    if (messageTenantId != contextTenantId)
                    {
                        throw new ApplicationException(
                            $"Invalid tenant ID for message {message.Payload.GetType()}. Expected {contextTenantId} but received {messageTenantIdHeader}");
                    }

                    if (_tenantMessagingConfigService.IsShared(messageTenantId) &&
                        _tenancyOptions.Value.TenancyContextType == TenancyContextType.MonoTenant)
                    {
                        throw new ApplicationException($"Received a message for shared tenant {messageTenantIdHeader} in a MonoTenant hosting");

                    }

                    if (!_tenantMessagingConfigService.IsShared(messageTenantId) &&
                        _tenancyOptions.Value.TenancyContextType == TenancyContextType.MultiTenant)
                    {
                        throw new ApplicationException($"Received a message for premium tenant {messageTenantIdHeader} in a MultiTenant (shared) context");

                    }
                }
            }

            await next();
        }

        private void CheckMonoTenant()
        {
            if (_tenancyOptions.Value.TenancyContextType != TenancyContextType.MonoTenant) return;
            var tenantId = _tenancyOptions.Value.MonoTenantId ?? throw new ApplicationException("MonoTenant Id is not configured");

            if (_tenantMessagingConfigService.IsShared(tenantId))
            {
                throw  new ApplicationException($"Starting message host for premium tenant {tenantId} in a MultiTenant (shared) context");
            }
        }
    }

    public static class MessagingPipelineExtensions
    {
        public static IPipelineBuilder<MessagingEnvelope> UseTenantValidationMiddleware(this IPipelineBuilder<MessagingEnvelope> pipelineBuilder)
            => pipelineBuilder.UseMiddleware<TenantValidationMiddleware, MessagingEnvelope>();
    }
}
