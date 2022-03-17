// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Abstractions.Configuration
{
    public class TenantConnectionsService<TTenantRepository> where TTenantRepository : class, ITenantRepository
    {
        public Func<IConfiguration, IServiceProvider> BuildServiceProvider = (configuration) => BuildDefaultServiceProvider(configuration);
        private static IServiceProvider BuildDefaultServiceProvider(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            services.AddSingleton(configuration);

            services.AddMultitenancy(configuration)
                .AddDefaultTenantConfiguration()
                .AddTenantRepository<TTenantRepository>();

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        public async Task<List<string>> GetConnectionStrings(IConfiguration configuration, List<string> configKeys)
        {
            var serviceProvider = BuildServiceProvider(configuration);
            var tenantContextAccessor = serviceProvider.GetRequiredService<ITenantContextAccessor>();
            tenantContextAccessor.TenantContext ??= new TenantContext(Tenant.Default);

            var tenantRepository = serviceProvider.GetRequiredService<ITenantRepository>();
            var tenantConfiguration = serviceProvider.GetService<ITenantConfiguration>();

            var tenants = await tenantRepository.GetAll();
            
            var result = tenants.SelectMany(t =>
            {
                tenantContextAccessor.ChangeTenantContext(t);
                var list = configKeys.Select(configKey =>
                {
                    var connectionString = tenantConfiguration.GetConnectionString(configKey);
                    return connectionString;
                });
                return list;
            })
            .Distinct(StringComparer.InvariantCultureIgnoreCase)
            .ToList();
            return result;
        }

        public Task<List<string>> GetConnectionStrings(IConfiguration configuration, string configKey)
        {
            var list = new List<string> { configKey };
            return GetConnectionStrings(configuration, list);
        }
    }
}
