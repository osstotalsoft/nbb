using System;

namespace NBB.MultiTenancy.Abstractions.Options
{
    public class TenancyHostingOptions
    {
        /// <summary>
        /// Returns the tenancy model used by the current process
        /// </summary>
        public TenancyType TenancyType { get; set; }

        /// <summary>
        /// Returns the current Tenant ID if the current tenancy context type is "MonoTenant"
        /// </summary>
        public Guid? TenantId { get; set; }
    }

    public enum TenancyType
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
