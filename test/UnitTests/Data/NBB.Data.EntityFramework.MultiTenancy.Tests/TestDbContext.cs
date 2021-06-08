using Microsoft.EntityFrameworkCore;

namespace NBB.Data.EntityFramework.MultiTenancy.Tests
{
    public class TestDbContext : MultiTenantDbContext
    {
        public DbSet<TestEntity> TestEntities { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new TestEntityConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}
