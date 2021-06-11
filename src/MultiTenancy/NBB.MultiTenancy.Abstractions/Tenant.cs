using System;

namespace NBB.MultiTenancy.Abstractions
{
    public class Tenant
    {
        public Guid TenantId { get; }

        public string Code { get; }

        public Tenant(Guid tenantId, string code)
        {
            TenantId = tenantId;
            Code = code;
        }
    }
}