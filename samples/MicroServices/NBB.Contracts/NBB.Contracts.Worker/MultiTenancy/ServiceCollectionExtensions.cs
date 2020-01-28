using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.MultiTenancy;
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
            var tenancyOptions = configurationSection.Get<TenancyOptions>();
            if (tenancyOptions == null || tenancyOptions.TenancyContextType == TenancyContextType.None)
            {
                return;
            }

            services.AddMultiTenantMessaging();
            services.AddSingleton<ITenantMessagingConfigService, TenantMessagingConfigService>();
            services.Configure<TenancyOptions>(configurationSection);

            services.AddTenantIdentification();
            switch (tenancyOptions.TenancyContextType)
            {
                case TenancyContextType.MultiTenant:
                    services.AddTenantIdentificationStrategy<IdTenantIdentifier>(config => config
                        .AddTenantTokenResolver<TenantIdHeaderMessagingTokenResolver>(MessagingHeaders
                            .TenantId));
                    break;
                case TenancyContextType.MonoTenant when tenancyOptions.MonoTenantId.HasValue:
                    services.AddTenantIdentificationStrategy<IdTenantIdentifier>(config => config
                        .AddTenantTokenResolver(
                            new DefaultTenantTokenResolver(tenancyOptions.MonoTenantId.Value)));
                    break;
                case TenancyContextType.None:
                default:
                    throw new Exception("Invalid tenancy configuration");
            }
        }
    }
}
