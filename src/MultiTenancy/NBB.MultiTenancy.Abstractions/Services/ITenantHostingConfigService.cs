using System;

namespace NBB.MultiTenancy.Abstractions.Services
{
    public interface ITenantHostingConfigService
    {
        bool IsShared(Guid tenantId);
    }
}