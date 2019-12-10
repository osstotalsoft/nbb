using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Services;
using NBB.MultiTenancy.Data.Abstractions;
using NBB.MultiTenancy.Data.EntityFramework.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Data.EntityFramework
{
    public abstract class MultiTenantDbContext : DbContext
    {
        private readonly Tenant _tenant;
        private readonly ITenantService _tenantService;
        private readonly ITenantDatabaseConfigService _tenantDatabaseConfigService;

        public MultiTenantDbContext(ITenantService tenantService, ITenantDatabaseConfigService tenantDatabaseConfigService)
        {
            _tenantService = tenantService;
            _tenantDatabaseConfigService = tenantDatabaseConfigService;

            _tenant = _tenantService.GetCurrentTenantAsync().GetAwaiter().GetResult();
        }

        #region base

        protected virtual void AddQueryFilters(ModelBuilder modelBuilder, List<IMutableEntityType> mandatory)
        {
            if (_tenant == null)
            {
                return;
            }

            var tenantId = _tenant.TenantId;

            mandatory.ToList().ForEach(t =>
            {
                Expression<Func<Guid, bool>> filter = t => EF.Property<Guid>(t, "TenantId") == tenantId;
                modelBuilder.Entity(t.ClrType).HasQueryFilter((LambdaExpression)filter);
            });
        }

        protected void UpdateDefaultTenantId()
        {
            if (_tenant == null)
            {
                return;
            }

            var list = ChangeTracker.Entries()
                .Where(e => e.Entity.GetType().GetCustomAttributes(typeof(MustHaveTenantAttribute), true).Length > 0)
                .ToList();

            foreach (var e in list)
            {
                if (e.State == EntityState.Added || e.State == EntityState.Modified)
                {
                    e.Property("TenantId").CurrentValue = _tenant.TenantId;
                }
            }
        }

        private Guid GetTenantId(object src)
        {
            return Guid.Parse(src.GetType().GetProperty("TenantId").GetValue(src, null).ToString());
        }

        protected List<Guid> GetViolations()
        {
            var list = (from e in ChangeTracker.Entries()
                        where e.Entity.GetType().GetCustomAttributes(typeof(MustHaveTenantAttribute), true).Length > 0
                        select GetTenantId(e.Entity))
                        .Distinct()
                        .ToList();

            return list.Distinct().ToList();
        }

        protected void ThrowIfMultipleTenants()
        {
            if (_tenant == null)
            {
                return;
            }

            var toCheck = GetViolations();

            if (toCheck.Count == 0)
            {
                return;
            }

            if (!_tenantDatabaseConfigService.IsSharedDatabase(_tenant.TenantId))
            {
                return;
            }

            if (toCheck.Count > 1)
            {
                throw new CrossTenantUpdateException(toCheck);
            }

            if (!toCheck.First().Equals(_tenant.TenantId))
            {
                throw new CrossTenantUpdateException(toCheck);
            }
        }

        #endregion       

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_tenantDatabaseConfigService.GetConnectionString(_tenant.TenantId));
            base.OnConfiguring(optionsBuilder);
        }

        private static void AddTenantProperty<T>(ModelBuilder modelBuilder, T entity) where T : class
        {
            modelBuilder.Entity<T>()
                .Property<Guid>("TenantId");

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnTenantModelCreating(modelBuilder);
            if (_tenant == null)
            {
                return;
            }

            var mandatory = new List<IMutableEntityType>();

            //modelBuilder.Entity<Contact>()
            //    .Property<DateTime>("LastUpdated");

            var listOfTenantAware = modelBuilder.Model.GetEntityTypes().Where(p => p.GetType().GetCustomAttributes(typeof(MustHaveTenantAttribute), true).Length > 0).ToList();

            foreach (var entity in listOfTenantAware)
            {
                AddTenantProperty(modelBuilder, entity as dynamic);
            }

            mandatory.AddRange(listOfTenantAware);

            mandatory = mandatory.Distinct().ToList();

            AddQueryFilters(modelBuilder, mandatory);

            base.OnModelCreating(modelBuilder);
        }

        protected abstract void OnTenantModelCreating(ModelBuilder modelBuilder);

        private int CheckContextIntegrity(Func<int> saveAction)
        {
            if (_tenant == null)
            {
                return saveAction();
            }

            if (_tenantDatabaseConfigService.IsSharedDatabase(_tenant.TenantId))
            {
                UpdateDefaultTenantId();
            }

            if (!_tenantDatabaseConfigService.IsSharedDatabase(_tenant.TenantId))
            {
                ThrowIfMultipleTenants();
            }

            return base.SaveChanges();
        }

        #region Write side
        public override int SaveChanges()
        {
            Func<int> saveAction = () => base.SaveChanges();
            return CheckContextIntegrity(saveAction);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            Func<int> saveAction = () => base.SaveChanges(acceptAllChangesOnSuccess);
            return CheckContextIntegrity(saveAction);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            if (_tenant == null)
            {
                return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }

            if (_tenantDatabaseConfigService.IsSharedDatabase(_tenant.TenantId))
            {
                UpdateDefaultTenantId();
            }

            if (!_tenantDatabaseConfigService.IsSharedDatabase(_tenant.TenantId))
            {
                ThrowIfMultipleTenants();
            }

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_tenant == null)
            {
                return base.SaveChangesAsync(cancellationToken);
            }

            if (_tenantDatabaseConfigService.IsSharedDatabase(_tenant.TenantId))
            {
                UpdateDefaultTenantId();
            }

            if (!_tenantDatabaseConfigService.IsSharedDatabase(_tenant.TenantId))
            {
                ThrowIfMultipleTenants();
            }

            return base.SaveChangesAsync(cancellationToken);
        }
        #endregion

    }
}