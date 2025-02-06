// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NBB.MultiTenancy.Abstractions.Context;
using System;
using System.Linq;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public static class DbContextExtensions
    {
        public static void SetTenantIdFromContext(this DbContext context)
        {
            var multiTenantEntities =
                context.ChangeTracker.Entries()
                    .Where(e => e.IsMultiTenant() && e.State != EntityState.Unchanged);

            if (!multiTenantEntities.Any())
            {
                return;
            }

            var tenantId = context.GetTenantIdFromContext();
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

        public static void UseMultitenancy(this DbContextOptionsBuilder options, IServiceProvider serviceProvider)
        {
            var extension = new MultiTenantOptionsExtension(serviceProvider);
            ((IDbContextOptionsBuilderInfrastructure)options).AddOrUpdateExtension(extension);
        }
    }
}
