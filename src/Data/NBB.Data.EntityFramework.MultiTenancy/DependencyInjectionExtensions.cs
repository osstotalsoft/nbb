using Microsoft.Extensions.DependencyInjection;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddTenantDatabaseConfigService<TTenantDatabaseConfigService>(this IServiceCollection services)
            where TTenantDatabaseConfigService : class, ITenantDatabaseConfigService
        {
            services.AddSingleton<ITenantDatabaseConfigService, TTenantDatabaseConfigService>();

            return services;
        }
    }
}
