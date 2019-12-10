using System;

namespace NBB.MultiTenancy.Data.Abstractions
{
    public interface IMayHaveTenant
    {
        Guid TenantId { get; set; }
    }
}