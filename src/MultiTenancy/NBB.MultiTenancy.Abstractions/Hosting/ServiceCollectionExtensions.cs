using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;

namespace NBB.MultiTenancy.Abstractions.Hosting
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
