// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;

namespace NBB.MultiTenancy.Abstractions.Context
{
    public class TenantContext
    {
        public Tenant Tenant { get; }
        public Dictionary<string, object> Properties { get; set; } = [];
        public TenantContext(Tenant tenant)
        {
            Tenant = tenant;
        }

        public TenantContext(Tenant tenant, Dictionary<string, object> properties)
        {
            Tenant = tenant;
            Properties = properties;
        }

        public TenantContext Clone()
        {
            return new TenantContext(new Tenant(Tenant.TenantId, Tenant.Code), new Dictionary<string, object>(Properties));
        }
    }
}
