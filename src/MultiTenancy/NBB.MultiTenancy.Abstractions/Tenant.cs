using System;

namespace NBB.MultiTenancy.Abstractions
{
    public class Tenant
    {
        public Guid TenantId { get; init; }

        public string Code { get; init; }

        public Tenant() { }

        public Tenant(Guid tenantId, string code)
        {
            TenantId = tenantId;
            Code = code;
        }

        public static Tenant Default { get; } = new Tenant(default, "default");
    }
}