using System;

namespace NBB.MultiTenancy.Abstractions.Services
{
    public interface ITenantMessagingConfigService
    {
        string GetTopicPrefix(Guid tenantId);
        bool IsShared(Guid tenantId);
    }
}