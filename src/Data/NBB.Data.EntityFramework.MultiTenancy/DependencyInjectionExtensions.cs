using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Abstractions;
using NBB.Data.Abstractions;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public static class DependencyInjectionExtensions
    {
        public static void AddMultiTenantEfUow<TEntity, TContext>(this IServiceCollection services)
            where TEntity : class
            where TContext : DbContext
        {
            services.AddScoped<IUow<TEntity>, MultiTenantEfUow<TEntity, TContext>>();
        }

        public static void AddMultiTenantEfCrudRepository<TEntity, TContext>(this IServiceCollection services)
            where TEntity : class
            where TContext : DbContext
        {
            services.AddScoped<ICrudRepository<TEntity>, EfCrudRepository<TEntity, TContext>>();
            services.AddMultiTenantEfUow<TEntity, TContext>();
        }

        public static IServiceCollection AddTenantDatabaseConfigService<TTenantDatabaseConfigService>(this IServiceCollection services)
            where TTenantDatabaseConfigService : class, ITenantDatabaseConfigService
        {
            services.AddSingleton<ITenantDatabaseConfigService, TTenantDatabaseConfigService>();

            return services;
        }
    }
}
