using System;

namespace NBB.MultiTenancy.Abstractions.Context
{
    public class TenantContext
    {
        public TenantInfo TenantInfo { get; }

        public TenantContext(TenantInfo tenantInfo)
        {
            TenantInfo = tenantInfo;
        }

        public TenantContext Clone()
        {
            return new TenantContext(new TenantInfo(TenantInfo.Id, TenantInfo.Code));
        }
    }
    
    public class TenantInfo
    {
        public Guid Id { get; }
        public string Code { get; }

        public TenantInfo(Guid id, string code)
        {
            Id = id;
            Code = code;
        }
    }
}
