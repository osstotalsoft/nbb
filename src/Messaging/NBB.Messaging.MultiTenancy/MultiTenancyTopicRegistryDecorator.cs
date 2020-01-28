using System;
using Microsoft.Extensions.Options;
using NBB.Messaging.Abstractions;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Services;

namespace NBB.Messaging.MultiTenancy
{
    public class MultiTenancyTopicRegistryDecorator : ITopicRegistry
    {
        private readonly ITopicRegistry _innerTopicRegistry;
        private readonly ITenantService _tenantService;
        private readonly ITenantMessagingConfigService _tenantMessagingConfigService;
        private readonly IOptions<TenancyOptions> _tenancyOptions;
        private const string SharedTopicPrefix = "Shared";

        public MultiTenancyTopicRegistryDecorator(ITopicRegistry innerTopicRegistry, ITenantService tenantService, ITenantMessagingConfigService tenantMessagingConfigService, IOptions<TenancyOptions> tenancyOptions)
        {
            _innerTopicRegistry = innerTopicRegistry;
            _tenantService = tenantService;
            _tenantMessagingConfigService = tenantMessagingConfigService;
            _tenancyOptions = tenancyOptions;
        }

        public string GetTopicForMessageType(Type messageType, bool includePrefix = true)
        {
            var topicName = _innerTopicRegistry.GetTopicForMessageType(messageType, false);

            if (string.IsNullOrEmpty(topicName))
            {
                return topicName;
            }

            return includePrefix ? $"{GetTopicPrefix()}{topicName}" : topicName;
        }

        public string GetTopicForName(string topicName, bool includePrefix = true)
        {
            topicName = _innerTopicRegistry.GetTopicForName(topicName, false);

            if (string.IsNullOrEmpty(topicName))
            {
                return topicName;
            }

            return includePrefix ? $"{GetTopicPrefix()}{topicName}" : topicName;
        }

        public string GetTopicPrefix()
        {
            var baseTopicPrefix = _innerTopicRegistry.GetTopicPrefix();
            switch (_tenancyOptions.Value.TenancyContextType)
            {
                case TenancyContextType.MultiTenant:
                    return $"{baseTopicPrefix}{SharedTopicPrefix}.";
                case TenancyContextType.MonoTenant:
                {
                    var tenantId = _tenantService.GetTenantIdAsync().GetAwaiter().GetResult();
                    return $"{baseTopicPrefix}Tenant.{tenantId}.";
                }
                case TenancyContextType.None:
                default:
                {
                    throw new ApplicationException("Invalid multiTenant context configuration");
                }
            }
        }
    }
}
