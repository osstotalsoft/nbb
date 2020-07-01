using Microsoft.Extensions.DependencyInjection;

namespace NBB.MultiTenancy.Abstractions.Repositories
{
    public static class DependencyInjectionExtensions
    {
        public static void AddTenantRepository<TTenantRepository>(this IServiceCollection services, bool useCaching = true)
            where TTenantRepository : class, ITenantRepository
        {
            services.AddScoped<ITenantRepository, TTenantRepository>();

            if (!useCaching) return;

            services.AddDistributedMemoryCache();
            services.Decorate<ITenantRepository, CachedTenantRepositoryDecorator>();
        }
    }
}
