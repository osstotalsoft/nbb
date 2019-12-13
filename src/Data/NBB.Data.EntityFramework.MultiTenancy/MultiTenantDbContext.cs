using Microsoft.EntityFrameworkCore;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public class MultiTenantDbContext : DbContext
    {
        private readonly Tenant _tenant;
        private readonly ITenantService _tenantService;
        private readonly MultitenantDbContextHelper _multitenantDbContextHelper;
        private readonly ITenantDatabaseConfigService _tenantDatabaseConfigService;

        public MultiTenantDbContext(ITenantService tenantService, MultitenantDbContextHelper multitenantDbContextHelper, ITenantDatabaseConfigService tenantDatabaseConfigService)
        {
            _tenantService = tenantService;
            _multitenantDbContextHelper = multitenantDbContextHelper;
            _tenantDatabaseConfigService = tenantDatabaseConfigService;

            _tenant = _tenantService.GetCurrentTenantAsync().GetAwaiter().GetResult();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            BeforeModelCreating(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        protected void BeforeModelCreating(ModelBuilder modelBuilder)
        {
            if (_tenant == null)
            {
                throw new Exception("Tenant could not be identified");
            }

            if (_tenantDatabaseConfigService.IsSharedDatabase(_tenant.TenantId))
            {
                _multitenantDbContextHelper.AddTenantIdProperties(modelBuilder, _tenant);
                _multitenantDbContextHelper.AddQueryFilters(modelBuilder, _tenant);
            }
        }

        public override int SaveChanges()
        {            
            _multitenantDbContextHelper.CheckContextIntegrity(this, _tenant);
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            _multitenantDbContextHelper.CheckContextIntegrity(this, _tenant);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            _multitenantDbContextHelper.CheckContextIntegrity(this, _tenant);
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _multitenantDbContextHelper.CheckContextIntegrity(this, _tenant);
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
