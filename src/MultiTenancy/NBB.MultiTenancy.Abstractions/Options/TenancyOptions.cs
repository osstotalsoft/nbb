using System;

namespace NBB.MultiTenancy.Abstractions.Options
{
    public class TenancyOptions
    {
        /// <summary>
        /// Returns the tenancy model used by the current process
        /// </summary>
        public TenancyContextType TenancyContextType { get; set; }

        /// <summary>
        /// Returns the current Tenant ID if the current tenancy context type is "MonoTenant"
        /// </summary>
        public Guid? MonoTenantId { get; set; }
    }

    public enum TenancyContextType
    {
        None = 0,

        /// <summary>
        /// The process can serve multiple tenants
        /// </summary>
        MultiTenant = 1,

        /// <summary>
        /// The process can serve a single tenant
        /// </summary>
        MonoTenant = 2
    }
}
