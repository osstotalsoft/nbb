using System;
using NBB.Messaging.Abstractions;
using NBB.MultiTenancy.Abstractions.Services;

namespace NBB.Messaging.MultiTenancy
{
    public class MultiTenancyTopicRegistryDecorator : ITopicRegistry
    {
        private readonly ITopicRegistry _innerTopicRegistry;
        private readonly ITenantService _tenantService;
        private readonly ITenantMessagingConfigService _tenantMessagingConfigService;

        public MultiTenancyTopicRegistryDecorator(ITopicRegistry innerTopicRegistry, ITenantService tenantService, ITenantMessagingConfigService tenantMessagingConfigService)
        {
            _innerTopicRegistry = innerTopicRegistry;
            _tenantService = tenantService;
            _tenantMessagingConfigService = tenantMessagingConfigService;
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

        private string GetTopicPrefix()
        {
            var tenantId = _tenantService.GetTenantIdAsync().GetAwaiter().GetResult();
            var topicPrefix = _tenantMessagingConfigService.GetTopicPrefix(tenantId);

            return topicPrefix;
        }
    }
}
