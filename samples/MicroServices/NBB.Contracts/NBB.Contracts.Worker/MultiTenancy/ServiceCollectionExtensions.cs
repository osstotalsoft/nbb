using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.MultiTenancy;
using NBB.MultiTenancy.Abstractions.Hosting;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Services;
using NBB.MultiTenancy.Identification.Extensions;
using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Messaging;
using NBB.MultiTenancy.Identification.Resolvers;

namespace NBB.Contracts.Worker.MultiTenancy
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMultiTenancy(this IServiceCollection services, IConfiguration configuration)
        {
            var configurationSection = configuration.GetSection("MultiTenancy");
            var tenancyOptions = configurationSection.Get<TenancyHostingOptions>();
            if (tenancyOptions == null || tenancyOptions.TenancyType == TenancyType.None)
            {
                return;
            }

            services.AddHostingConfigValidation();
            services.AddMultiTenantMessaging();
            services.AddSingleton<ITenantHostingConfigService, TenantHostingConfigService>();
            services.Configure<TenancyHostingOptions>(configurationSection);

            services.AddTenantIdentification();
            switch (tenancyOptions.TenancyType)
            {
                case TenancyType.MultiTenant:
                    services.AddTenantIdentificationStrategy<IdTenantIdentifier>(config => config
                        .AddMessagingTokenResolver());
                    break;
                case TenancyType.MonoTenant when tenancyOptions.TenantId.HasValue:
                    services.AddTenantIdentificationStrategy<IdTenantIdentifier>(config => config
                        .AddTenantTokenResolver(
                            new DefaultTenantTokenResolver(tenancyOptions.TenantId.Value)));
                    break;
                case TenancyType.None:
                default:
                    throw new Exception("Invalid tenancy configuration");
            }
        }
    }
}
