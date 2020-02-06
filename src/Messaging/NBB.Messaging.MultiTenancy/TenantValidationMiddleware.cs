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
        private readonly ITenantHostingConfigService _tenantHostingConfigService;
        private readonly IOptions<TenancyHostingOptions> _tenancyOptions;

        public TenantValidationMiddleware(ITenantService tenantService, ITenantHostingConfigService tenantHostingConfigService, IOptions<TenancyHostingOptions> tenancyOptions)
        {
            _tenantService = tenantService;
            _tenantHostingConfigService = tenantHostingConfigService;
            _tenancyOptions = tenancyOptions;
        }

        public async Task Invoke(MessagingEnvelope message, CancellationToken cancellationToken, Func<Task> next)
        {
            var contextTenantId = await _tenantService.GetTenantIdAsync();
            if (!message.Headers.TryGetValue(MessagingHeaders.TenantId, out var messageTenantIdHeader))
            {
                throw new ApplicationException($"The tenant ID message header is missing from the message envelope");
            }

            if (!Guid.TryParse(messageTenantIdHeader, out var messageTenantId))
            {
                throw new ApplicationException($"The tenant ID message header is invalid");
            }

            if (messageTenantId != contextTenantId)
            {
                throw new ApplicationException(
                    $"Invalid tenant ID for message {message.Payload.GetType()}. Expected {contextTenantId} but received {messageTenantIdHeader}");
            }

            if (_tenancyOptions.Value.TenancyType == TenancyType.MonoTenant && _tenancyOptions.Value.TenantId != messageTenantId)
            {
                throw new ApplicationException(
                    $"Invalid tenant ID for message {message.Payload.GetType()}. Expected {_tenancyOptions.Value.TenantId} but received {messageTenantIdHeader}");
            }

            if (_tenantHostingConfigService.IsShared(messageTenantId) &&
                _tenancyOptions.Value.TenancyType == TenancyType.MonoTenant)
            {
                throw new ApplicationException(
                    $"Received a message for shared tenant {messageTenantIdHeader} in a MonoTenant hosting");
            }

            if (!_tenantHostingConfigService.IsShared(messageTenantId) &&
                _tenancyOptions.Value.TenancyType == TenancyType.MultiTenant)
            {
                throw new ApplicationException(
                    $"Received a message for premium tenant {messageTenantIdHeader} in a MultiTenant (shared) context");
            }

            await next();
        }

    }

    public static class MessagingPipelineExtensions
    {
        public static IPipelineBuilder<MessagingEnvelope> UseTenantValidationMiddleware(this IPipelineBuilder<MessagingEnvelope> pipelineBuilder)
            => pipelineBuilder.UseMiddleware<TenantValidationMiddleware, MessagingEnvelope>();
    }
}
