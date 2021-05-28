using Microsoft.EntityFrameworkCore;
using NBB.Todo.Data.Entities;
using NBB.Todo.Data.EntityConfigurations;

namespace NBB.Todo.Data
{
    public class NoTenantTodoDbContext : DbContext
    {
        public DbSet<TodoTask> TodoTasks { get; set; }

        public NoTenantTodoDbContext(DbContextOptions<NoTenantTodoDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new TodoTaskConfiguration());
        }
    }
}
