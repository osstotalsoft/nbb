using System;

namespace NBB.MultiTenancy.Abstractions
{
    public class Tenant
    {
        public Guid TenantId { get; }
        public string Name { get; }

        public Tenant(Guid tenantId, string name)
        {
            TenantId = tenantId;
            Name = name;
        }
    }
}