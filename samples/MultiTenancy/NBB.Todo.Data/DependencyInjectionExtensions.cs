// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using NBB.Todo.Data;
using NBB.Todo.Data.Entities;
using NBB.Data.EntityFramework.MultiTenancy;
using NBB.MultiTenancy.Abstractions.Configuration;

namespace NBB.Todos.Data
{
    public static class DependencyInjectionExtensions
    {
        public static void AddTodoDataAccess(this IServiceCollection services)
        {
            services.AddDefaultTenantConfiguration();

            services.AddEfCrudRepository<TodoTask, TodoDbContext>();
            services.AddEfQuery<TodoTask, TodoDbContext>();

            services.AddEntityFrameworkDataAccess();

            services.AddDbContext<TodoDbContext>(
                (serviceProvider, options) =>
                {
                    var databaseService = serviceProvider.GetRequiredService<ITenantConfiguration>();
                    var connectionString = databaseService.GetConnectionString("DefaultConnection");

                    options
                        .UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Todo.Migrations"))
                        .UseMultitenancy(serviceProvider);
                });
        }
    }
}
