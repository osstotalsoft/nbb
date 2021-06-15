using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Abstractions.Repositories
{
    public class ConfigurationTenantRepository : ITenantRepository
    {
        private const string MultiTenantConfigurationRepo = "MultiTenancy";

        private readonly IConfigurationSection _configurationSection;
        private ConcurrentDictionary<Guid, Tenant> tenantMap;

        public ConfigurationTenantRepository(IConfiguration configuration, IOptions<TenancyHostingOptions> tenancyHostingOptions)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _configurationSection = configuration.GetSection(MultiTenantConfigurationRepo);
            if (!_configurationSection.Exists())
            {
                throw new Exception($"Tenancy not configured. Add the '{MultiTenantConfigurationRepo}' section to the application configuration.");
            }

            if (tenancyHostingOptions.Value.TenancyType == TenancyType.MonoTenant)
            {
                LoadDefaultTenant();
            }
            else
            {
                LoadTenants();
            };
        }

        public Task<Tenant> Get(Guid id, CancellationToken token = default)
        {

            return Task.FromResult(tenantMap.TryGetValue(id, out var result) ? result : throw new Exception($"Database configiguration not found for tenant {id}"));
        }

        public Task<Tenant> GetByHost(string host, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        private void LoadTenants()
        {
            var newMap = new ConcurrentDictionary<Guid, Tenant>();
            var tenants = _configurationSection.GetSection("Tenants").GetChildren();

            foreach (var tenantSection in tenants)
            {
                var newTenant = _configurationSection.GetSection("Defaults").Get<Tenant>(options => options.BindNonPublicProperties = true) ?? Tenant.Default;
                tenantSection.Bind(newTenant, options => options.BindNonPublicProperties = true);
                newMap.TryAdd(newTenant.TenantId, newTenant);
            }

            tenantMap = newMap;
        }

        private void LoadDefaultTenant()
        {
            var newTenant = _configurationSection.GetSection("Defaults").Get<Tenant>() ?? Tenant.Default;
            tenantMap = new ConcurrentDictionary<Guid, Tenant>() { [newTenant.TenantId] = newTenant };
        }
    }
}
