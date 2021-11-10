// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.MultiTenancy.Abstractions.Context
{
    public class TenantContext
    {
        public Tenant Tenant { get; }

        public TenantContext(Tenant tenant)
        {
            Tenant = tenant;
        }

        public TenantContext Clone()
        {
            return new TenantContext(new Tenant(Tenant.TenantId, Tenant.Code));
        }
    }
}
