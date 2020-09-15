using System;

namespace NBB.MultiTenancy.Abstractions
{
    public class Tenant
    {
        public Guid TenantId { get; }

        public string Code { get; }

        public bool IsShared { get; }

        public Tenant(Guid tenantId, string code, bool isShared)
        {
            TenantId = tenantId;
            Code = code;
            IsShared = isShared;
        }
    }
}