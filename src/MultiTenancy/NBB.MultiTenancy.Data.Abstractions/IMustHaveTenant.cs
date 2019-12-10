using System;

namespace NBB.MultiTenancy.Data.Abstractions
{
    public interface IMustHaveTenant
    {
        Guid TenantId { get; set; }
    }
}