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
            return (Guid) tenantProp.CurrentValue;
        }

        public static bool IsMultiTenant(this EntityEntry e)
        {
            var annotation = e.Metadata.FindAnnotation(MultiTenancy.MultiTenantAnnotation);
            if (annotation != null)
            {
                return (bool)annotation.Value;
            }

            return false;
        }
    }
}
