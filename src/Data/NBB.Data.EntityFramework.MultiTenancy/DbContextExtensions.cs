using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public static class DbContextExtensions
    {
        public static void SetTenantId(this DbContext context, Guid tenantId)
        {
            var multiTenantEntities =
                context.ChangeTracker.Entries()
                    .Where(e => e.IsMultiTenant() && e.State != EntityState.Unchanged);

            foreach (var e in multiTenantEntities)
            {
                var attemptedTenantId = e.GetTenantId();
                if (attemptedTenantId.HasValue && attemptedTenantId != tenantId)
                {
                    throw new Exception(
                        $"Attempted to save entities for TenantId {attemptedTenantId} in the context of TenantId {tenantId}");
                }
                e.SetTenantId(tenantId);
            }
        }
    }
}