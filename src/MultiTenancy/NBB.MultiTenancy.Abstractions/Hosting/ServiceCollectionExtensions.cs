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

            services.AddSingleton<ITenantContextAccessor, TenantContextAccessor>();

            addTenantAwareServices(tenancyOptions);
        }
    }
}
