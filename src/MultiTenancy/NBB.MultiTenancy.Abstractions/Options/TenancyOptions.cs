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
        MultiTenant,
        MonoTenant
    }
}
