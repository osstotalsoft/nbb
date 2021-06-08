using Microsoft.Extensions.DependencyInjection;
using NBB.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;
using NBB.Todo.Data;
using NBB.Todo.Data.Entities;
using NBB.Data.EntityFramework.MultiTenancy;
using NBB.MultiTenancy.Abstractions.Context;

namespace NBB.Todos.Data
{
    public static class DependencyInjectionExtensions
    {
        public static void AddTodoDataAccess(this IServiceCollection services)
        {
            services.AddTenantDatabaseConfigService<TenantDatabaseConfigService>();

            services.AddEfCrudRepository<TodoTask, TodoDbContext>();
            services.AddEfQuery<TodoTask, TodoDbContext>();

            services.AddEntityFrameworkDataAccess();
            services.AddEntityFrameworkSqlServer();

            services.AddDbContext<TodoDbContext>(
                (serviceProvider, options) =>
                {
                    var databaseService = serviceProvider.GetService<ITenantDatabaseConfigService>();
                    var tenantContextAccessor = serviceProvider.GetRequiredService<ITenantContextAccessor>();
                    var tenantId = tenantContextAccessor.TenantContext?.GetTenantId();
                    var connectionString = databaseService.GetConnectionString(tenantId ?? default);

                    options
                        .UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Todo.Migrations"))
                        .UseInternalServiceProvider(serviceProvider);
                });
        }
    }
}
