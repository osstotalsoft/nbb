// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.MultiTenancy.Abstractions.Configuration;
using NBB.MultiTenancy.Abstractions.Repositories;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static void AddTenantRepository<TTenantRepository>(this IServiceCollection services, bool useCaching = true)
            where TTenantRepository : class, ITenantRepository
        {
            services.AddScoped<ITenantRepository, TTenantRepository>();
            services.AddScoped<TenantInfrastructure>();

            if (!useCaching) return;

            services.AddDistributedMemoryCache();
            services.Decorate<ITenantRepository, CachedTenantRepositoryDecorator>();
        }
    }
}
