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
        private readonly ITenantDatabaseConfigService _tenantDatabaseConfigService;
        private readonly MultitenantDbContextHelper _multitenantDbContextHelper;

        public MultiTenantDbContext(ITenantService tenantService, ITenantDatabaseConfigService tenantDatabaseConfigService, MultitenantDbContextHelper multitenantDbContextHelper)
        {
            _tenantService = tenantService;
            _tenantDatabaseConfigService = tenantDatabaseConfigService;
            _multitenantDbContextHelper = multitenantDbContextHelper;

            _tenant = _tenantService.GetCurrentTenantAsync().GetAwaiter().GetResult();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_tenantDatabaseConfigService.GetConnectionString(_tenant.TenantId));
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            BeforeModelCreating(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        protected void BeforeModelCreating(ModelBuilder modelBuilder)
        {
            _multitenantDbContextHelper.AddTenantIdProperties(modelBuilder, _tenant);
            _multitenantDbContextHelper.AddQueryFilters(modelBuilder, _tenant);
        }

        public override int SaveChanges()
        {
            Func<int> saveAction = () => base.SaveChanges();
            return _multitenantDbContextHelper.CheckContextIntegrity(this, saveAction, _tenant);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            Func<int> saveAction = () => base.SaveChanges(acceptAllChangesOnSuccess);
            return _multitenantDbContextHelper.CheckContextIntegrity(this, saveAction, _tenant);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            Func<Task<int>> saveAction = () => base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            return _multitenantDbContextHelper.CheckContextIntegrityAsync(this, saveAction, _tenant);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            Func<Task<int>> saveAction = () => base.SaveChangesAsync(cancellationToken);
            return _multitenantDbContextHelper.CheckContextIntegrityAsync(this, saveAction, _tenant);
        }
    }
}
