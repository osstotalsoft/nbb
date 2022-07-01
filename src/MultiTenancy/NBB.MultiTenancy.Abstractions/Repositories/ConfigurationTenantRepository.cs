﻿// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
            if (!tenantMap.TryGetValue(id, out var result))
            {
                throw new Exception($"Tenant configuration not found for tenant {id}");
            }


            return Task.FromResult(result.Enabled ? result : throw new Exception($"Tenant {result.Code} is disabled "));
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
                var tenant = _configurationSection.GetSection("Defaults").Get<Tenant>(options => options.BindNonPublicProperties = true) ?? new Tenant();
                tenantSection.Bind(tenant, options => options.BindNonPublicProperties = true);
                tenant = tenant with { Code = tenantSection.Key };

                if (tenant.IsValid())
                {
                    newMap.TryAdd(tenant.TenantId, tenant);
                }
            }

            tenantMap = newMap;
        }

        private void LoadDefaultTenant()
        {
            var newTenant = Tenant.Default;
            tenantMap = new ConcurrentDictionary<Guid, Tenant>() { [newTenant.TenantId] = newTenant };
        }

        public Task<List<Tenant>> GetAll(CancellationToken token = default)
        {
            return Task.FromResult(tenantMap.Values.Where(t => t.Enabled).ToList());
        }
    }
}
