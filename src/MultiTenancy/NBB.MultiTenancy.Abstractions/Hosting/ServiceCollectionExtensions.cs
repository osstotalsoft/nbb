using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Services;

namespace NBB.MultiTenancy.Abstractions.Hosting
{
    public static class ServiceCollectionExtensions
    {
        private const string MessagingSectionName = "MultiTenancy";

        public static void AddMultitenancy(this IServiceCollection services, IConfiguration configuration,
            Action<TenancyHostingOptions> addTenantAwareServices)
        {
            var configurationSection = configuration.GetSection(MessagingSectionName);
            var tenancyOptions = configurationSection.Get<TenancyHostingOptions>();
            if (tenancyOptions == null || tenancyOptions.TenancyType == TenancyType.None)
            {
                return;
            }

            services.AddSingleton<IHostedService, TenancyHostingValidator>();
            services.Configure<TenancyHostingOptions>(configurationSection);

            addTenantAwareServices(tenancyOptions);
        }

        public static void AddTenantHostingConfigService<TTenantHostingConfigService>(this IServiceCollection services)
            where TTenantHostingConfigService : class, ITenantHostingConfigService
        {
            services.AddSingleton<ITenantHostingConfigService, TTenantHostingConfigService>();
        }
    }
}
