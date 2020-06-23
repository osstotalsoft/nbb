using System;

namespace NBB.MultiTenancy.Abstractions.Context
{
    public static class TenantContextExtensions
    {
        public static Guid GetTenantId(this TenantContext tenantContext) => tenantContext.TenantInfo?.Id ?? throw new TenantNotFoundException();
        public static Guid? TryGetTenantId(this TenantContext tenantContext) => tenantContext.TenantInfo?.Id;

        public static TenantContextFlow ChangeTenantContext(this ITenantContextAccessor tenantContextAccessor, Guid tenantId)
            => tenantContextAccessor.ChangeTenantContext(new TenantContext(new TenantInfo(tenantId, null)));
    }
}
