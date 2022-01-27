// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.


// ReSharper disable once CheckNamespace

using Microsoft.Extensions.DependencyInjection;

namespace NBB.MultiTenancy.Abstractions.Configuration
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDefaultTenantConfiguration(this IServiceCollection services)
        {
            services.AddSingleton<ITenantConfiguration, TenantConfiguration>();

            return services;
        }
    }
}
