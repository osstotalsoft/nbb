// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;
using System;
using System.Collections.Concurrent;

namespace NBB.MultiTenancy.Abstractions.Configuration
{
    public class TenantConfiguration : ITenantConfiguration
    {
        private const string MultitenantDbConfigSection = "MultiTenancy";

        private readonly IConfigurationSection _tenancyConfigurationSection;
        private readonly IConfiguration _globalConfiguration;
        private readonly IOptions<TenancyHostingOptions> _tenancyHostingOptions;
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private ConcurrentDictionary<Guid, IConfiguration> _tenantMap;

        public TenantConfiguration(IConfiguration configuration, IOptions<TenancyHostingOptions> tenancyHostingOptions, ITenantContextAccessor tenantContextAccessor)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _tenancyConfigurationSection = configuration.GetSection(MultitenantDbConfigSection);
            if (!_tenancyConfigurationSection.Exists())
            {
                throw new Exception($"Tenancy not configured. Add the '{MultitenantDbConfigSection}' section to the application configuration.");
            }

            _globalConfiguration = configuration;
            _tenancyHostingOptions = tenancyHostingOptions;
            _tenantContextAccessor = tenantContextAccessor;

            if (tenancyHostingOptions.Value.TenancyType == TenancyType.MultiTenant)
            {
                LoadTenantsMap();
            };
        }
 
        private void LoadTenantsMap()
        {
            var newMap = new ConcurrentDictionary<Guid, IConfiguration>();
            var tenants = _tenancyConfigurationSection.GetSection("Tenants").GetChildren();

            foreach (var tenantSection in tenants)
            {
                var tid = tenantSection.GetValue<Guid>("TenantId");
                newMap.TryAdd(tid, tenantSection);
            }

            _tenantMap = newMap;
        }

        public T GetValue<T>(string key)
        {
            var tenantId = _tenantContextAccessor.TenantContext.GetTenantId();
            if (_tenancyHostingOptions.Value.TenancyType == TenancyType.MonoTenant)
            {
                return _globalConfiguration.GetValue<T>(key);
            }
            else
            {
                var defaultSection = _tenancyConfigurationSection.GetSection("Defaults");
                var section = _tenantMap.TryGetValue(tenantId, out var result)
                    ? result
                    : throw new Exception($"Database configiguration not found for tenant {tenantId}");

                return section.GetValue<T>(key, defaultSection.GetValue<T>(key));
            }
        }
    }
}
