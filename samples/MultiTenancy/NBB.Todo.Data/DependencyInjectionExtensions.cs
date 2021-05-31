using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;
using NBB.Todo.Data;
using NBB.Todo.Data.Entities;
using NBB.Data.EntityFramework.MultiTenancy;
using NBB.MultiTenancy.Abstractions.Context;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;

namespace NBB.Todos.Data
{
    public static class DependencyInjectionExtensions
    {
        public static void AddTodoDataAccess(this IServiceCollection services)
        {
            services.AddTenantDatabaseConfigService<TenantDatabaseConfigService>();

            services.AddMultiTenantEfCrudRepository<TodoTask, TodoDbContext>();
            services.AddEfQuery<TodoTask, TodoDbContext>();

            services.AddEntityFrameworkDataAccess();
            services.AddEntityFrameworkSqlServer();

            services.AddDbContext<TodoDbContext>(
                (serviceProvider, options) =>
                {
                    var tenancyOptions = serviceProvider.GetRequiredService<IOptions<TenancyHostingOptions>>();
                    var isMultiTenant = tenancyOptions?.Value?.TenancyType != TenancyType.None;
                    string connectionString = null;

                    if (isMultiTenant)
                    {
                        var databaseService = serviceProvider.GetService<ITenantDatabaseConfigService>();
                        var tenantContextAccessor = serviceProvider.GetRequiredService<ITenantContextAccessor>();
                        var tenantId = tenantContextAccessor.TenantContext.GetTenantId();
                        connectionString = databaseService.GetConnectionString(tenantId);
                    }
                    else
                    {
                        var configuration = serviceProvider.GetService<IConfiguration>();
                        connectionString = configuration.GetConnectionString("DefaultConnection");
                    }

                    options
                        .UseSqlServer(connectionString, b => b.MigrationsAssembly("NBB.Todo.Migrations"))
                        .UseInternalServiceProvider(serviceProvider);
                });
        }
    }
}
