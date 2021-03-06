﻿using Microsoft.Extensions.DependencyInjection;
using NBB.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;
using NBB.Todo.Data;
using NBB.Todo.Data.Entities;
using NBB.Data.EntityFramework.MultiTenancy;
namespace NBB.Todos.Data
{
    public static class DependencyInjectionExtensions
    {
        public static void AddTodoDataAccess(this IServiceCollection services)
        {
            services.AddTenantDatabaseConfigService<ConfigurationDatabaseConfigService>();

            services.AddEfCrudRepository<TodoTask, TodoDbContext>();
            services.AddEfQuery<TodoTask, TodoDbContext>();

            services.AddEntityFrameworkDataAccess();

            services.AddDbContext<TodoDbContext>(
                (serviceProvider, options) =>
                {
                    var databaseService = serviceProvider.GetRequiredService<ITenantDatabaseConfigService>();
                    var connectionString = databaseService.GetConnectionString();

                    options
                        .UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Todo.Migrations"))
                        .UseMultitenancy(serviceProvider);
                });
        }
    }
}
