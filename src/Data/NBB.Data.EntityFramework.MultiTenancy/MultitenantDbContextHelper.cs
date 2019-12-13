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
        private readonly ITenantService _tenantService;

        public MultitenantDbContextHelper(ITenantDatabaseConfigService tenantDatabaseConfigService, ITenantService tenantService)
        {
            _tenantDatabaseConfigService = tenantDatabaseConfigService;
            _tenantService = tenantService;
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
                throw new Exception("Tenant could not be identified");
            }

            var mandatory = new List<IMutableEntityType>();

            var entityTypes = modelBuilder.Model.GetEntityTypes();
            var listByAttributes = modelBuilder.Model.GetEntityTypes().Where(p => p.ClrType.GetCustomAttributes(typeof(MustHaveTenantAttribute), true).Length > 0).ToList();

            var listByAnnotations = modelBuilder.Model.GetEntityTypes().Where(e => e.IsMultiTenant())            
                .Distinct()
                .ToList();

            var listOfTenantAware = listByAnnotations.Union(listByAttributes).Distinct().ToList();
            foreach (var entity in listOfTenantAware)
            {
                var e = modelBuilder.Entity(entity.ClrType);
                e.Property(typeof(Guid), "TenantId");
            }
        }

        public void AddQueryFilter<T>(ModelBuilder modelBuilder) where T : class
        {
            Expression<Func<T, bool>> filter = t => EF.Property<Guid>(t, "TenantId") == _tenantService.GetCurrentTenantAsync().GetAwaiter().GetResult().TenantId;
            modelBuilder.Entity<T>().HasQueryFilter((LambdaExpression)filter);
        }

        public MultitenantDbContextHelper AddQueryFilters(ModelBuilder modelBuilder, Tenant tenant)
        {
            if (tenant == null)
            {
                throw new Exception("Tenant could not be identified");
            }

            var entities = modelBuilder.Model.GetEntityTypes().Where(p => p.ClrType.GetCustomAttributes(typeof(MustHaveTenantAttribute), true).Length > 0).ToList();

            entities.ToList().ForEach(t =>
            {
                var method = this.GetType().GetMethod("AddQueryFilter");
                var genericMethod = method.MakeGenericMethod(t.ClrType);
                genericMethod.Invoke(this, new object[] { modelBuilder });
            });
            return this;
        }

        public MultitenantDbContextHelper UpdateDefaultTenantId(DbContext dbContext, Tenant tenant)
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
            return this;
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

        public MultitenantDbContextHelper ThrowIfMultipleTenants(DbContext dbContext, Tenant tenant)
        {
            if (tenant == null)
            {
                throw new Exception("Tenant could not be identified");
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