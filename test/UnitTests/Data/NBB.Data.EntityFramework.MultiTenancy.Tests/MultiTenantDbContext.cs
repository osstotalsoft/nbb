using Microsoft.EntityFrameworkCore;
using NBB.MultiTenancy.Abstractions.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.EntityFramework.MultiTenancy.Tests
{
    public class MultiTenantDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; }
        private readonly Guid _tenantId;


        public MultiTenantDbContext(DbContextOptions<MultiTenantDbContext> options, ITenantService tenantService) : base(options)
        {
            _tenantId = tenantService.GetTenantIdAsync().GetAwaiter().GetResult();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyMultiTenantConfiguration(new TestEntityConfiguration(), this);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            this.SetTenantId(_tenantId);
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.SetTenantId(_tenantId);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
    }
}