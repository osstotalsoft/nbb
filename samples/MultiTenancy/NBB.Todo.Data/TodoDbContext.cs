using Microsoft.EntityFrameworkCore;
using NBB.Data.EntityFramework.MultiTenancy;
using NBB.Todo.Data.Entities;
using NBB.Todo.Data.EntityConfigurations;

namespace NBB.Todo.Data
{
    public class TodoDbContext : DbContext
    {
        public DbSet<TodoTask> TodoTasks { get; set; }
        public TodoDbContext(DbContextOptions<TodoDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyMultiTenantConfiguration(new TodoTaskConfiguration(), this);
        }

        //public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        //{
        //    this.SetTenantIdFromContext();
        //    return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        //}

        //public override int SaveChanges(bool acceptAllChangesOnSuccess)
        //{
        //    this.SetTenantIdFromContext();
        //    return base.SaveChanges(acceptAllChangesOnSuccess);
        //}
    }
}
