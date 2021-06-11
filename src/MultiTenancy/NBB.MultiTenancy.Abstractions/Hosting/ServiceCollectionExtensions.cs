using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.MultiTenancy.Abstractions.Context;
using NBB.MultiTenancy.Abstractions.Options;

namespace NBB.MultiTenancy.Abstractions.Hosting
{
    public static class ServiceCollectionExtensions
    {
        private const string MultitenancySectionName = "MultiTenancy";

        public static void AddMultitenancy(this IServiceCollection services, IConfiguration configuration,
            Action<TenancyHostingOptions> addTenantAwareServices)
        {
            var configurationSection = configuration.GetSection(MultitenancySectionName);
            var tenancyOptions = configurationSection.Get<TenancyHostingOptions>();
            services.Configure<TenancyHostingOptions>(configurationSection);
            services.AddSingleton<ITenantContextAccessor, TenantContextAccessor>();

            if (tenancyOptions == null)
            {
                throw new Exception($"Tenancy not configured. Add the '{MultitenancySectionName}' section to the application configuration.");
            }

            services.AddSingleton<IHostedService, TenancyHostingValidator>();
            addTenantAwareServices(tenancyOptions);
        }
    }
}
