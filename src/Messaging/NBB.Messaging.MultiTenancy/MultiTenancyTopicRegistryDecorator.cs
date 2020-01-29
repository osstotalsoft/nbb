using System;
using Microsoft.Extensions.Options;
using NBB.Messaging.Abstractions;
using NBB.MultiTenancy.Abstractions.Options;

namespace NBB.Messaging.MultiTenancy
{
    public class MultiTenancyTopicRegistryDecorator : ITopicRegistry
    {
        private readonly ITopicRegistry _innerTopicRegistry;
        private readonly IOptions<TenancyHostingOptions> _tenancyOptions;
        private const string SharedTopicPrefix = "Shared";
        private const string TenantTopicPrefix = "Tenant";

        public MultiTenancyTopicRegistryDecorator(ITopicRegistry innerTopicRegistry, IOptions<TenancyHostingOptions> tenancyOptions)
        {
            _innerTopicRegistry = innerTopicRegistry;
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
            switch (_tenancyOptions.Value.TenancyType)
            {
                case TenancyType.MultiTenant:
                    return $"{baseTopicPrefix}{SharedTopicPrefix}.";
                case TenancyType.MonoTenant when _tenancyOptions.Value.MonoTenantId.HasValue:
                {
                    //var tenantId = _tenantService.GetTenantIdAsync().GetAwaiter().GetResult();
                    var tenantId = _tenancyOptions.Value.MonoTenantId.Value;
                    return $"{baseTopicPrefix}{TenantTopicPrefix}.{tenantId}.";
                }
                default:
                {
                    throw new ApplicationException("Invalid multiTenant context configuration");
                }
            }
        }
    }
}
