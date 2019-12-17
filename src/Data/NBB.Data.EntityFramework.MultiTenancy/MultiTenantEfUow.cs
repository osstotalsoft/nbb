using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Services;
using NBB.MultiTenancy.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public class MultiTenantEfUow<TEntity, TContext> : IUow<TEntity>
        where TEntity : class
        where TContext : DbContext
    {
        private readonly TContext _c;
        private readonly ILogger<MultiTenantEfUow<TEntity, TContext>> _logger;
        private readonly ITenantService _tenantService;
        private readonly ITenantDatabaseConfigService _tenantDatabaseConfigService;

        public MultiTenantEfUow(TContext c, ITenantService tenantService, ITenantDatabaseConfigService tenantDatabaseConfigService)
        {
            _c = c;
            _tenantService = tenantService;
            _tenantDatabaseConfigService = tenantDatabaseConfigService;
        }

        public IEnumerable<TEntity> GetChanges()
        {
            return _c.ChangeTracker.Entries<TEntity>().Select(ee => ee.Entity);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var tenant = await _tenantService.GetCurrentTenantAsync();
            CheckContextIntegrity(_c, tenant);
            await _c.SaveChangesAsync(cancellationToken);            
        }

        public void UpdateDefaultTenantId(DbContext dbContext, Tenant tenant)
        {
            if (tenant == null)
            {
                throw new Exception("Tenant could not be identified");
            }

            var list = dbContext.ChangeTracker.Entries()
                .Where(e => e.Entity.GetType().GetCustomAttributes(typeof(MustHaveTenantAttribute), true).Length > 0)
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList();

            var listByAnnotations =
                dbContext.ChangeTracker.Entries()
                .Where(e => e.Metadata.IsMultiTenant() && e.State != EntityState.Unchanged)
                .ToList();

            list = list.Union(listByAnnotations).ToList();

            foreach (var e in list)
            {
                var tenantProp = e.Property("TenantId");
                if ((Guid)tenantProp.CurrentValue == Guid.Empty)
                {
                    tenantProp.CurrentValue = tenant.TenantId;
                }
            }
        }

        protected List<Guid> GetViolations(DbContext dbContext)
        {
            var list = GetViolationsByAnnotations(dbContext).Union(GetViolationsByAttributes(dbContext));

            return list.ToList();
        }

        private List<Guid> GetViolationsByAnnotations(DbContext dbContext)
        {
            var list = dbContext
                        .ChangeTracker
                        .Entries()
                        .Where(e => e.Metadata.IsMultiTenant() && e.State != EntityState.Unchanged)
                        .Select(e => (Guid)dbContext.Entry(e.Entity).Property("TenantId").CurrentValue)
                        .Distinct()
                        .ToList();

            return list.Distinct().ToList();
        }

        private List<Guid> GetViolationsByAttributes(DbContext dbContext)
        {
            var list = dbContext
                          .ChangeTracker
                          .Entries()
                          .Where(e => e.Entity.GetType().GetCustomAttributes(typeof(MustHaveTenantAttribute), true).Length > 0 && e.State != EntityState.Unchanged)
                          .Select(e => (Guid)dbContext.Entry(e.Entity).Property("TenantId").CurrentValue)
                          .Distinct()
                          .ToList();

            return list.Distinct().ToList();
        }

        public void ThrowIfMultipleTenants(DbContext dbContext, Tenant tenant)
        {
            if (tenant == null)
            {
                throw new Exception("Tenant could not be identified");
            }

            var toCheck = GetViolations(dbContext);

            if (toCheck.Count == 0)
            {
                return;
            }

            if (!_tenantDatabaseConfigService.IsSharedDatabase(tenant.TenantId))
            {
                return;
            }

            if (toCheck.Count > 1)
            {
                throw new CrossTenantUpdateException(toCheck);
            }

            if (!toCheck.First().Equals(tenant.TenantId))
            {
                throw new CrossTenantUpdateException(toCheck);
            }
            return;
        }

        public void CheckContextIntegrity(DbContext dbContext, Tenant tenant)
        {
            if (tenant == null)
            {
                throw new Exception("Tenant could not be identified");
            }

            if (_tenantDatabaseConfigService.IsSharedDatabase(tenant.TenantId))
            {
                UpdateDefaultTenantId(dbContext, tenant);
            }

            if (!_tenantDatabaseConfigService.IsSharedDatabase(tenant.TenantId))
            {
                ThrowIfMultipleTenants(dbContext, tenant);
            }
        }
    }
}