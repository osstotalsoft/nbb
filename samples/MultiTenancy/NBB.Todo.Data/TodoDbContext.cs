using Microsoft.EntityFrameworkCore;
using NBB.Data.EntityFramework.MultiTenancy;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.Todo.Data.Entities;
using NBB.Todo.Data.EntityConfigurations;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Todo.Data
{
    public class TodoDbContext : DbContext
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;

        public DbSet<TodoTask> TodoTasks { get; set; }

        public TodoDbContext(DbContextOptions<TodoDbContext> options, ITenantContextAccessor tenantContextAccessor)
            : base(options)
        {
            _tenantContextAccessor = tenantContextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyMultiTenantConfiguration(new TodoTaskConfiguration(), this);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            this.SetTenantId(_tenantContextAccessor.TenantContext.GetTenantId());
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.SetTenantId(_tenantContextAccessor.TenantContext.GetTenantId());
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
    }
}
