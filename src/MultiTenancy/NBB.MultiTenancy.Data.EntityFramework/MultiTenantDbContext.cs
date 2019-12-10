using Microsoft.EntityFrameworkCore;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Services;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;
using NBB.MultiTenancy.Data.Abstractions;
using NBB.MultiTenancy.Data.EntityFramework.Extensions;
using NBB.MultiTenancy.Data.EntityFramework.Exceptions;

namespace NBB.MultiTenancy.Data.EntityFramework
{
    public abstract class MultiTenantDbContext : DbContext
    {
        private readonly Tenant _tenant;
        private readonly ITenantService _tenantService;
        private readonly TenantDatabaseConfiguration _tenantDatabaseConfiguration;

        public MultiTenantDbContext(ITenantService tenantService, TenantDatabaseConfiguration tenantDatabaseConfiguration)
        {
            _tenantService = tenantService;
            _tenantDatabaseConfiguration = tenantDatabaseConfiguration;

            _tenant = _tenantService.GetCurrentTenantAsync().GetAwaiter().GetResult();

            if (_tenantDatabaseConfiguration.IsReadOnly)
            {
                ChangeTracker.AutoDetectChangesEnabled = false;
                ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
        }

        #region base

        public Expression<Func<T, bool>> GetMandatoryFilter<T>(ModelBuilder modelBuilder, T tenantId) where T : class, IMustHaveTenant
        {
            Expression<Func<T, bool>> filter = t => t.TenantId.Equals(tenantId);
            return filter;
        }

        public Expression<Func<T, bool>> GetOptionalFilter<T>(ModelBuilder modelBuilder, Guid tenantId) where T : class, IMayHaveTenant
        {
            Expression<Func<T, bool>> filter = t => !t.TenantId.IsNullOrDefault();
            return filter;
        }

        public void SetDefaultValue<T>(ModelBuilder modelBuilder, T tenantId) where T : class, IMustHaveTenant
        {
            modelBuilder.Entity<T>().Property(nameof(IMustHaveTenant.TenantId)).HasDefaultValue(tenantId);
        }

        protected void ApplyDefaultValues(ModelBuilder modelBuilder)
        {
            if (_tenant == null)
            {
                return;
            }
            var mandatory = new List<IMutableEntityType>();

            mandatory.AddRange(modelBuilder.Model.GetEntityTypes().Where(p => typeof(IMustHaveTenant).IsAssignableFrom(p.ClrType)).ToList());

            mandatory = mandatory.Distinct().ToList();

            var tenantId = _tenant.TenantId;

            mandatory.ToList().ForEach(t =>
            {
                SetDefaultValue(modelBuilder, tenantId as dynamic);
            });
        }

        protected virtual void AddQueryFilters(ModelBuilder modelBuilder, List<IMutableEntityType> optional, List<IMutableEntityType> mandatory)
        {
            if (_tenant == null)
            {
                return;
            }

            var tenantId = _tenant.TenantId;

            optional.ForEach(t =>
            {
                var filter = GetOptionalFilter(modelBuilder, tenantId as dynamic);
                modelBuilder.Entity(t.ClrType).HasQueryFilter((LambdaExpression)filter);
            });

            mandatory.ToList().ForEach(t =>
            {
                var filter = GetMandatoryFilter(modelBuilder, tenantId as dynamic);
                modelBuilder.Entity(t.ClrType).HasQueryFilter((LambdaExpression)filter);
            });
        }

        protected List<Guid> GetViolationsByInheritance()
        {
            var optionalIds = (from e in ChangeTracker.Entries()
                               where e.Entity is IMayHaveTenant && !((IMayHaveTenant)e.Entity).TenantId.IsNullOrDefault()
                               select ((IMayHaveTenant)e.Entity).TenantId)
                       .Distinct()
                       .ToList();

            var mandatoryIds = (from e in ChangeTracker.Entries()
                                where e.Entity is IMustHaveTenant
                                select ((IMustHaveTenant)e.Entity).TenantId)
                       .Distinct()
                       .ToList();

            var toCheck = optionalIds.Union(mandatoryIds).ToList();
            return toCheck;
        }

        protected void UpdateDefaultTenantId()
        {
            if (_tenant == null)
            {
                return;
            }

            var list = ChangeTracker.Entries()
                .Where(e => e.Entity is IMustHaveTenant && ((IMustHaveTenant)e.Entity).TenantId.IsNullOrDefault())
                .Select(e => ((IMustHaveTenant)e.Entity));
            foreach (var e in list)
            {
                e.TenantId = _tenant.TenantId;
            }
        }

        protected List<Guid> GetViolations()
        {
            var list = GetViolationsByInheritance();

            return list.Distinct().ToList();
        }

        protected void ThrowIfMultipleTenants()
        {
            if (_tenant == null)
            {
                return;
            }

            var toCheck = GetViolations();

            if (toCheck.Count >= 1 && _tenantDatabaseConfiguration.IsReadOnly)
            {
                throw new Exception("Read only Db context");
            }

            if (toCheck.Count == 0)
            {
                return;
            }

            if (!_tenantDatabaseConfiguration.RestrictCrossTenantAccess)
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
            _tenantDatabaseConfiguration.ConfigureConnection(_tenant);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnTenantModelCreating(modelBuilder);
            if (_tenant == null)
            {
                return;
            }
            if (_tenantDatabaseConfiguration.UseDefaultValueOnSave)
            {
                ApplyDefaultValues(modelBuilder);
            }

            if (!_tenantDatabaseConfiguration.RestrictCrossTenantAccess)
            {
                base.OnModelCreating(modelBuilder);
                return;
            }
            var optional = new List<IMutableEntityType>();
            var mandatory = new List<IMutableEntityType>();
            optional.AddRange(modelBuilder.Model.GetEntityTypes().Where(p => typeof(IMayHaveTenant).IsAssignableFrom(p.GetType())).ToList());
            mandatory.AddRange(modelBuilder.Model.GetEntityTypes().Where(p => typeof(IMustHaveTenant).IsAssignableFrom(p.ClrType)).ToList());

            optional = optional.Distinct().ToList();
            mandatory = mandatory.Distinct().ToList();

            AddQueryFilters(modelBuilder, optional, mandatory);


            base.OnModelCreating(modelBuilder);
        }

        protected abstract void OnTenantModelCreating(ModelBuilder modelBuilder);

        private int CheckContextIntegrity(Func<int> saveAction)
        {
            if (_tenant == null)
            {
                return saveAction();
            }

            if (_tenantDatabaseConfiguration.IsReadOnly)
            {
                throw new Exception("Readonly");
            }

            if (_tenantDatabaseConfiguration.UseDefaultValueOnSave)
            {
                UpdateDefaultTenantId();
            }

            if (_tenantDatabaseConfiguration.RestrictCrossTenantAccess)
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

            if (_tenantDatabaseConfiguration.IsReadOnly)
            {
                throw new Exception("Readonly");
            }

            if (_tenantDatabaseConfiguration.UseDefaultValueOnSave)
            {
                UpdateDefaultTenantId();
            }

            if (_tenantDatabaseConfiguration.RestrictCrossTenantAccess)
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

            if (_tenantDatabaseConfiguration.IsReadOnly)
            {
                throw new Exception("Readonly");
            }

            if (_tenantDatabaseConfiguration.UseDefaultValueOnSave)
            {
                UpdateDefaultTenantId();
            }

            if (_tenantDatabaseConfiguration.RestrictCrossTenantAccess)
            {
                ThrowIfMultipleTenants();
            }

            return base.SaveChangesAsync(cancellationToken);
        }
        #endregion

    }
}