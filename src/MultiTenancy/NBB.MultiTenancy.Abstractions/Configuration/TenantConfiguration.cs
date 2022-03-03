// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    public string this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

    private IConfiguration GetTenantConfiguration()
    {
        if (_tenancyHostingOptions.Value.TenancyType == TenancyType.MonoTenant)
        {
            return _globalConfiguration;
        }

        var defaultSection = _tenancyConfigurationSection.GetSection("Defaults");
        var tenantId = _tenantContextAccessor.TenantContext.GetTenantId();
        if(_tenantMap.TryGetValue(tenantId, out var tenentSectionPath))
        {
            var tenantSection = _tenancyConfigurationSection.GetSection(tenentSectionPath);
            var mergedSection = new MergedConfigurationSection(tenantSection, defaultSection);
            return mergedSection;
        }

        return defaultSection;
    }

    public IEnumerable<IConfigurationSection> GetChildren()
        => GetTenantConfiguration().GetChildren();

    public IChangeToken GetReloadToken()
        => GetTenantConfiguration().GetReloadToken();

    public IConfigurationSection GetSection(string key)
        => GetTenantConfiguration().GetSection(key);
}
