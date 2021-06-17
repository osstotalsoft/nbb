using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;
using System;
using System.Collections.Concurrent;

namespace NBB.Data.EntityFramework.MultiTenancy
{
    public class ConfigurationDatabaseConfigService : ITenantDatabaseConfigService
    {
        private const string MultitenantDbConfigSection = "MultiTenancy";

        private readonly IConfigurationSection _configurationSection;
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private ConcurrentDictionary<Guid, TenantDbConfig> tenantMap;

        public ConfigurationDatabaseConfigService(IConfiguration configuration, IOptions<TenancyHostingOptions> tenancyHostingOptions, ITenantContextAccessor tenantContextAccessor)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _configurationSection = configuration.GetSection(MultitenantDbConfigSection);
            if (!_configurationSection.Exists())
            {
                throw new Exception($"Tenancy not configured. Add the '{MultitenantDbConfigSection}' section to the application configuration.");
            }

            _tenantContextAccessor = tenantContextAccessor;

            if (tenancyHostingOptions.Value.TenancyType == TenancyType.MonoTenant)
            {
                LoadDefaultTenant();
            }
            else
            {
                LoadTenants();
            };
        }

        public string GetConnectionString()
        {
            var tenantId = _tenantContextAccessor.TenantContext.GetTenantId();

            return tenantMap.TryGetValue(tenantId, out var result)
                ? result.ConnectionString
                : throw new Exception($"Database configiguration not found for tenant {tenantId}");
        }

        private void LoadTenants()
        {
            var newMap = new ConcurrentDictionary<Guid, TenantDbConfig>();
            var tenants = _configurationSection.GetSection("Tenants").GetChildren();

            foreach (var tenantSection in tenants)
            {
                var newTenantConfig = _configurationSection.GetSection("Defaults").Get<TenantDbConfig>() ?? new TenantDbConfig();
                tenantSection.Bind(newTenantConfig, options => options.BindNonPublicProperties = true);
                newMap.TryAdd(newTenantConfig.TenantId, newTenantConfig);
            }

            tenantMap = newMap;
        }

        private void LoadDefaultTenant()
        {
            var newTenantConfig = _configurationSection.GetSection("Defaults").Get<TenantDbConfig>() ?? TenantDbConfig.Default;
            tenantMap = new ConcurrentDictionary<Guid, TenantDbConfig>() { [newTenantConfig.TenantId] = newTenantConfig };
        }

        private class TenantDbConfig
        {
            public Guid TenantId { get; init; }
            public string ConnectionString { get; init; }

            public static TenantDbConfig Default { get; } = new TenantDbConfig { TenantId = Tenant.Default.TenantId };
        }
    }
}
