using System;
using System.Collections.Generic;
using System.Text;

namespace NBB.MultiTenancy.Abstractions.Options
{
    public class TenancyOptions
    {
        public TenancyContextType TenancyContextType { get; set; }

        public Guid? MonoTenantId { get; set; }
    }

    public enum TenancyContextType
    {
        None = 0,
        MultiTenant = 1,
        MonoTenant = 2
    }
}
