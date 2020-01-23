using System;

namespace NBB.MultiTenancy.Abstractions.Services
{
    public interface ITenantMessagingConfigService
    {
        bool IsShared(Guid tenantId);
    }
}