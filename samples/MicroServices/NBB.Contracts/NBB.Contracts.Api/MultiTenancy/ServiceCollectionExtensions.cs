using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NBB.Messaging.MultiTenancy;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Services;
using NBB.MultiTenancy.Identification.Extensions;
using NBB.MultiTenancy.Identification.Http;
using NBB.MultiTenancy.Identification.Identifiers;

namespace NBB.Contracts.Api.MultiTenancy
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
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ITenantMessagingConfigService, TenantMessagingConfigService>();
            services.Configure<TenancyOptions>(configurationSection);

            services.AddTenantIdentification()
                .AddTenantIdentificationStrategy<IdTenantIdentifier>(builder => builder
                    .AddTenantTokenResolver<TenantIdHeaderHttpTokenResolver>("TenantId"));
        }
    }
}
