using Microsoft.EntityFrameworkCore;
using NBB.Todo.Data.Entities;

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

            modelBuilder.Entity<TodoTask>(builder =>
            {
                builder.ToTable("TodoTasks")
                    .HasKey(c => c.TodoTaskId);
            });
        }
    }
}
