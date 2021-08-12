// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public static class EntityEntryExtensions
    {
        public static void SetTenantId(this EntityEntry e, Guid tenantId)
        {
            var tenantProp = e.Property(MultiTenancy.TenantIdProp);
            tenantProp.CurrentValue = tenantId;
        }

        public static Guid GetTenantId(this EntityEntry e)
        {
            var tenantProp = e.Property(MultiTenancy.TenantIdProp);
            return (Guid)tenantProp.CurrentValue;
        }

        public static bool IsMultiTenant(this EntityEntry e)
            => e.Metadata.IsMultiTenant();
    }
}
