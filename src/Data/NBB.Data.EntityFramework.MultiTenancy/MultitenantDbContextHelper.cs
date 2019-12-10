using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Services;
using NBB.MultiTenancy.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public class MultitenantDbContextHelper
    {
        private readonly ITenantDatabaseConfigService _tenantDatabaseConfigService;
        public MultitenantDbContextHelper(ITenantDatabaseConfigService tenantDatabaseConfigService)
        {
            _tenantDatabaseConfigService = tenantDatabaseConfigService;
        }

        private static void AddTenantProperty<T>(ModelBuilder modelBuilder, T entity) where T : class
        {
            modelBuilder.Entity<T>()
                .Property<Guid>("TenantId");

        }

        public void AddTenantIdProperties(ModelBuilder modelBuilder, Tenant tenant)
        {
            if (tenant == null)
            {
                return;
            }
            var mandatory = new List<IMutableEntityType>();

            var listOfTenantAware = modelBuilder.Model.GetEntityTypes().Where(p => p.GetType().GetCustomAttributes(typeof(MustHaveTenantAttribute), true).Length > 0).ToList();

            foreach (var entity in listOfTenantAware)
            {
                AddTenantProperty(modelBuilder, entity as dynamic);
            }
        }

        public MultitenantDbContextHelper AddQueryFilters(ModelBuilder modelBuilder, Tenant tenant)
        {
            if (tenant == null)
            {
                return this;
            }

            var entities = modelBuilder.Model.GetEntityTypes().Where(p => p.GetType().GetCustomAttributes(typeof(MustHaveTenantAttribute), true).Length > 0).ToList();

            var tenantId = tenant.TenantId;

            entities.ToList().ForEach(t =>
            {
                Expression<Func<Guid, bool>> filter = t => EF.Property<Guid>(t, "TenantId") == tenantId;
                modelBuilder.Entity(t.ClrType).HasQueryFilter((LambdaExpression)filter);
            });
            return this;
        }


        public MultitenantDbContextHelper UpdateDefaultTenantId(DbContext dbContext, Tenant tenant)
        {
            if (tenant == null)
            {
                return this;
            }

            var list = dbContext.ChangeTracker.Entries()
                .Where(e => e.Entity.GetType().GetCustomAttributes(typeof(MustHaveTenantAttribute), true).Length > 0)
                .ToList();

            foreach (var e in list)
            {
                if (e.State == EntityState.Added || e.State == EntityState.Modified)
                {
                    e.Property("TenantId").CurrentValue = tenant.TenantId;
                }
            }
            return this;
        }

        private Guid GetTenantId(object src)
        {
            return Guid.Parse(src.GetType().GetProperty("TenantId").GetValue(src, null).ToString());
        }

        protected List<Guid> GetViolations(DbContext dbContext)
        {
            var list = (from e in dbContext.ChangeTracker.Entries()
                        where e.Entity.GetType().GetCustomAttributes(typeof(MustHaveTenantAttribute), true).Length > 0
                        select GetTenantId(e.Entity))
                        .Distinct()
                        .ToList();

            return list.Distinct().ToList();
        }
        public MultitenantDbContextHelper ThrowIfMultipleTenants(DbContext dbContext, Tenant tenant)
        {
            if (tenant == null)
            {
                return this;
            }

            var toCheck = GetViolations(dbContext);

            if (toCheck.Count == 0)
            {
                return this;
            }

            if (!_tenantDatabaseConfigService.IsSharedDatabase(tenant.TenantId))
            {
                return this;
            }

            if (toCheck.Count > 1)
            {
                throw new CrossTenantUpdateException(toCheck);
            }

            if (!toCheck.First().Equals(tenant.TenantId))
            {
                throw new CrossTenantUpdateException(toCheck);
            }
            return this;
        }

        public int CheckContextIntegrity(DbContext dbContext, Func<int> saveAction, Tenant tenant)
        {
            if (tenant == null)
            {
                return saveAction();
            }

            if (_tenantDatabaseConfigService.IsSharedDatabase(tenant.TenantId))
            {
                UpdateDefaultTenantId(dbContext, tenant);
            }

            if (!_tenantDatabaseConfigService.IsSharedDatabase(tenant.TenantId))
            {
                ThrowIfMultipleTenants(dbContext, tenant);
            }

            return saveAction();
        }

        public Task<int> CheckContextIntegrityAsync(DbContext dbContext, Func<Task<int>> saveAction, Tenant tenant)
        {
            if (tenant == null)
            {
                return saveAction();
            }

            if (_tenantDatabaseConfigService.IsSharedDatabase(tenant.TenantId))
            {
                UpdateDefaultTenantId(dbContext, tenant);
            }

            if (!_tenantDatabaseConfigService.IsSharedDatabase(tenant.TenantId))
            {
                ThrowIfMultipleTenants(dbContext, tenant);
            }

            return saveAction();
        }
    }
}
