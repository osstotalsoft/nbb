using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Services;
using NBB.MultiTenancy.Data.Abstractions;
using NBB.MultiTenancy.Data.EntityFramework.Exceptions;
using NBB.MultiTenancy.Data.EntityFramework.Extensions;
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

        public Expression<Func<T, bool>> GetMandatoryFilter<T>(T tenantId) where T : class, IMustHaveTenant
        {
            Expression<Func<T, bool>> filter = t => t.TenantId.Equals(tenantId);
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

        protected virtual void AddQueryFilters(ModelBuilder modelBuilder, List<IMutableEntityType> mandatory)
        {
            if (_tenant == null)
            {
                return;
            }

            var tenantId = _tenant.TenantId;            

            mandatory.ToList().ForEach(t =>
            {
                var filter = GetMandatoryFilter(tenantId as dynamic);
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
                .Where(e => e.Entity is IMustHaveTenant && ((IMustHaveTenant)e.Entity).TenantId.IsNullOrDefault())
                .Select(e => ((IMustHaveTenant)e.Entity));
            foreach (var e in list)
            {
                e.TenantId = _tenant.TenantId;
            }
        }

        private Guid GetTenantId(object src)
        {
            return Guid.Parse(src.GetType().GetProperty("TenantId").GetValue(src, null).ToString());
        }

        protected List<Guid> GetViolations()
        {
            var list = (from e in ChangeTracker.Entries()
                        //where e.Entity is IMustHaveTenant
                        //select ((IMustHaveTenant)e.Entity).TenantId)
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnTenantModelCreating(modelBuilder);
            if (_tenant == null)
            {
                return;
            }

            if (_tenantDatabaseConfigService.IsSharedDatabase(_tenant.TenantId))
            {
                UpdateDefaultTenantId();
            }

            if (!_tenantDatabaseConfigService.IsSharedDatabase(_tenant.TenantId))
            {
                ThrowIfMultipleTenants();
            }

            var mandatory = new List<IMutableEntityType>();
            
            mandatory.AddRange(modelBuilder.Model.GetEntityTypes().Where(p => typeof(IMustHaveTenant).IsAssignableFrom(p.ClrType)).ToList());

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