// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using  NBB.Data.EntityFramework.MultiTenancy;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection    
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
