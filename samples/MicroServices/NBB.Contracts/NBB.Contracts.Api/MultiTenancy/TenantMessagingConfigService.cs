using System;
using NBB.MultiTenancy.Abstractions.Services;

namespace NBB.Contracts.Api.MultiTenancy
{
    public class TenantMessagingConfigService : ITenantMessagingConfigService
    {
        public string GetTopicPrefix(Guid tenantId)
            => IsShared(tenantId) ? string.Empty : $"Tenant_{tenantId.ToString()}_";

        public bool IsShared(Guid tenantId) => false;
    }
}
