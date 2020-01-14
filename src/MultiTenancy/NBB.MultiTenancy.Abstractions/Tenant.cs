using System;

namespace NBB.MultiTenancy.Abstractions
{
    public class Tenant
    {
        public Guid TenantId { get; }

        public Tenant(Guid tenantId)
        {
            TenantId = tenantId;
        }
    }
}