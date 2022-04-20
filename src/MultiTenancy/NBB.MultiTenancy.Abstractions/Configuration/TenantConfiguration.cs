// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;
using System;
using System.Collections.Generic;

namespace NBB.MultiTenancy.Abstractions.Configuration;

public class TenantConfiguration : ITenantConfiguration
{
    private const string MultitenantDbConfigSection = "MultiTenancy";

    private readonly IConfigurationSection _tenancyConfigurationSection;
    private readonly IConfiguration _globalConfiguration;
    private readonly IOptions<TenancyHostingOptions> _tenancyHostingOptions;
    private readonly ITenantContextAccessor _tenantContextAccessor;

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
    }

    private IConfiguration GetTenantConfiguration()
    {
        if (_tenancyHostingOptions.Value.TenancyType == TenancyType.MonoTenant)
        {
            return _globalConfiguration;
        }

        var defaultSection = _tenancyConfigurationSection.GetSection("Defaults");
        var tenantCode = _tenantContextAccessor.TenantContext.GetTenantCode();

        var tenantSection = _tenancyConfigurationSection.GetSection($"Tenants:{tenantCode}");
        if (tenantSection.Exists())
        {
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
