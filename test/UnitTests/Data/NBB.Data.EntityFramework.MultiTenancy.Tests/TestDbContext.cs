using Microsoft.EntityFrameworkCore;

namespace NBB.Data.EntityFramework.MultiTenancy.Tests
{
    public class TestDbContext : DbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyMultiTenantConfiguration(new TestEntityConfiguration(), this);

        }
    }
}
