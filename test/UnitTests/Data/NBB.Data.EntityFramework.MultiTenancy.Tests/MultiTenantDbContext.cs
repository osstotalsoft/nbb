using Microsoft.EntityFrameworkCore;
using NBB.MultiTenancy.Abstractions.Context;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.EntityFramework.MultiTenancy.Tests
{
    public class MultiTenantDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; }

        public MultiTenantDbContext(DbContextOptions<MultiTenantDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyMultiTenantConfiguration(new TestEntityConfiguration(), this);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            this.SetTenantIdFromContext();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.SetTenantIdFromContext();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
    }
}