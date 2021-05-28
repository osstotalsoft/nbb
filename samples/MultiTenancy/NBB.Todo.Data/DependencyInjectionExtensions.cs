using Microsoft.Extensions.Configuration;
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
            services.AddEntityFrameworkDataAccess();

            services.AddEfCrudRepository<TodoTask, NoTenantTodoDbContext>();
            services.AddEfQuery<TodoTask, NoTenantTodoDbContext>();

            services.AddDbContext<NoTenantTodoDbContext>(
                (serviceProvider, options) =>
                {
                    var configuration = serviceProvider.GetService<IConfiguration>();
                    var connectionString = configuration.GetConnectionString("DefaultConnection");

                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Todo.Migrations"));
                });
        }

        public static void AddMultiTenantTodoDataAccess(this IServiceCollection services)
        {
            services.AddTenantDatabaseConfigService<TenantDatabaseConfigService>();

            services.AddEntityFrameworkDataAccess();

            services.AddMultiTenantEfCrudRepository<TodoTask, TodoDbContext>();
            services.AddEfQuery<TodoTask, TodoDbContext>();

            services.AddEntityFrameworkSqlServer();
            services.AddDbContext<TodoDbContext>(
                (serviceProvider, options) =>
                {

                    var databaseService = serviceProvider.GetService<ITenantDatabaseConfigService>();
                    var tenantContextAccessor = serviceProvider.GetRequiredService<ITenantContextAccessor>();
                    var tenantId = tenantContextAccessor.TenantContext.GetTenantId();
                    var connectionString = databaseService.GetConnectionString(tenantId);


                    options
                        .UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Todo.Migrations"))
                        .UseInternalServiceProvider(serviceProvider);
                });
        }
    }
}
