using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public abstract class MultiTenantDbContext : DbContext
    {
        protected MultiTenantDbContext()
        {
        }

        protected MultiTenantDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureMultiTenant(this);
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.SetTenantIdFromContext();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            this.SetTenantIdFromContext();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}
