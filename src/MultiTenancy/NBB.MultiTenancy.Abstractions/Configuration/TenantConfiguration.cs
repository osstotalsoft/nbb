// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;

namespace NBB.MultiTenancy.Abstractions.Configuration;

public class TenantConfiguration : ITenantConfiguration
{
    private const string MultitenantDbConfigSection = "MultiTenancy";

    private readonly IConfigurationSection _tenancyConfigurationSection;
    private readonly IConfiguration _globalConfiguration;
    private readonly IOptions<TenancyHostingOptions> _tenancyHostingOptions;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private ConcurrentDictionary<Guid, string> _tenantMap;

    public TenantConfiguration(IConfiguration configuration, IOptions<TenancyHostingOptions> tenancyHostingOptions,
        ITenantContextAccessor tenantContextAccessor)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        _tenancyConfigurationSection = configuration.GetSection(MultitenantDbConfigSection);
        if (!_tenancyConfigurationSection.Exists())
        {
            throw new Exception(
                $"Tenancy not configured. Add the '{MultitenantDbConfigSection}' section to the application configuration.");
        }

        _globalConfiguration = configuration;
        _tenancyHostingOptions = tenancyHostingOptions;
        _tenantContextAccessor = tenantContextAccessor;

        if (tenancyHostingOptions.Value.TenancyType == TenancyType.MultiTenant)
        {
            LoadTenantsMap();
        }
    }

    private void LoadTenantsMap()
    {
        var newMap = new ConcurrentDictionary<Guid, string>();
        var tenants = _tenancyConfigurationSection.GetSection("Tenants").GetChildren().ToList();

        for (int i = 0; i < tenants.Count; i++)
        {
            var tid = tenants[i].GetValue<Guid>("TenantId");
            newMap.TryAdd(tid, $"Tenants:{i}");
        }

        _tenantMap = newMap;
    }

    public T GetValue<T>(string key)
    {
        if (_tenancyHostingOptions.Value.TenancyType == TenancyType.MonoTenant)
        {
            return getValueOrComplexObject<T>(_globalConfiguration, key);
        }

        var tenantId = _tenantContextAccessor.TenantContext.GetTenantId();
        var defaultSection = _tenancyConfigurationSection.GetSection("Defaults");
        var sectionPath = _tenantMap.TryGetValue(tenantId, out var result)
            ? result
            : throw new Exception($"Configuration not found for tenant {tenantId}");


        return getValueOrComplexObject<T>(_tenancyConfigurationSection.GetSection(sectionPath), key, defaultSection);
    }

    private static T getValueOrComplexObject<T>(IConfiguration config, string key, IConfigurationSection defaultSection = null)
    {
        //section.GetSection is never null
        if (config.GetSection(key).GetChildren().Any())
        {
            //complex type is present
            return config.GetSection(key).Get<T>();
        }

        if (config.GetSection(key).Value != null)
            return config.GetValue<T>(key);

        return defaultSection == null ? default : getValueOrComplexObject<T>(defaultSection, key);
    }
}
