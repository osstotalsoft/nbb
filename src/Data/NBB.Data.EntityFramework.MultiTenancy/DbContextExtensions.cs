using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;
using System;
using System.Linq;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public static class DbContextExtensions
    {
        public static void SetTenantIdFromContext(this DbContext context)
        {
            if (!context.IsSharedDatabase())
                return;

            var tenantId = context.GetTenantIdFromContext();

            var multiTenantEntities =
                context.ChangeTracker.Entries()
                    .Where(e => e.IsMultiTenant() && e.State != EntityState.Unchanged);

            foreach (var e in multiTenantEntities)
            {
                var attemptedTenantId = e.GetTenantId();
                if (attemptedTenantId != default && attemptedTenantId != tenantId)
                {
                    throw new Exception(
                        $"Attempted to save entities for TenantId {attemptedTenantId} in the context of TenantId {tenantId}");
                }
                e.SetTenantId(tenantId);
            }
        }

        public static Guid GetTenantIdFromContext(this DbContext dbContext)
          => dbContext.GetInfrastructure().GetRequiredService<ITenantContextAccessor>().TenantContext.GetTenantId();

        public static bool IsSharedDatabase(this DbContext dbContext)
        {
            var sp = dbContext.GetInfrastructure();

            var tenancyOptions = sp.GetRequiredService<IOptions<TenancyHostingOptions>>();
            var isMultiTenant = tenancyOptions?.Value?.TenancyType != TenancyType.None;
            if (!isMultiTenant)
                return false;

            var tenantDatabaseConfigService = sp.GetRequiredService<ITenantDatabaseConfigService>();
            var tenantId = dbContext.GetTenantIdFromContext();
            var isSharedDB = tenantDatabaseConfigService.IsSharedDatabase(tenantId);

            return isSharedDB;
        }
    }
}