// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using Microsoft.Extensions.Configuration;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private const string MultitenancySectionName = "MultiTenancy";

        public static IServiceCollection AddMultitenancy(this IServiceCollection services, IConfiguration configuration)
        {
            var configurationSection = configuration.GetSection(MultitenancySectionName);
            var tenancyOptions = configurationSection.Get<TenancyHostingOptions>();

            if (tenancyOptions == null)
            {
                throw new Exception($"Tenancy not configured. Add the '{MultitenancySectionName}' section to the application configuration.");
            }

            services.Configure<TenancyHostingOptions>(configurationSection);
            services.AddSingleton<ITenantContextAccessor, TenantContextAccessor>();

            return services;
        }
    }
}
