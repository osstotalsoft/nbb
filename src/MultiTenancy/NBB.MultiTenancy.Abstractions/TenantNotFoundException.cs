// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.MultiTenancy.Abstractions
{
    public class TenantNotFoundException : Exception
    {
        public TenantNotFoundException()
        {
        }

        public TenantNotFoundException(Guid tenantId)
            : base($"Tenant {tenantId} not found.")
        {

        }
    }
}
