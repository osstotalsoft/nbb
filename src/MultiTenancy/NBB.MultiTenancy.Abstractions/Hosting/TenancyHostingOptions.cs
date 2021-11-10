// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.MultiTenancy.Abstractions.Options
{
    public class TenancyHostingOptions
    {
        /// <summary>
        /// Returns the tenancy model used by the current process
        /// </summary>
        public TenancyType TenancyType { get; set; }
    }

    public enum TenancyType
    {
        /// <summary>
        /// The process can serve multiple tenants
        /// </summary>
        MultiTenant = 0,

        /// <summary>
        /// The process can serve a single tenant
        /// </summary>
        MonoTenant = 1
    }
}
