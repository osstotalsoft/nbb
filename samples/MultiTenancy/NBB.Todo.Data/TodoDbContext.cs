// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using NBB.Data.EntityFramework.MultiTenancy;
using NBB.Todo.Data.Entities;
using NBB.Todo.Data.EntityConfigurations;

namespace NBB.Todo.Data
{
    public class TodoDbContext : MultiTenantDbContext
    {
        public DbSet<TodoTask> TodoTasks { get; set; }
        public TodoDbContext(DbContextOptions<TodoDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new TodoTaskConfiguration());
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
