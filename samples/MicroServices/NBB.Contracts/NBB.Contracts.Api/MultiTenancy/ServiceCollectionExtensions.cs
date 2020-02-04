using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NBB.Messaging.MultiTenancy;
using NBB.MultiTenancy.Abstractions.Hosting;
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
            var tenancyOptions = configurationSection.Get<TenancyHostingOptions>();
            if (tenancyOptions == null || tenancyOptions.TenancyType == TenancyType.None)
            {
                return;
            }

            services.AddHostingConfigValidation();
            services.AddMultiTenantMessaging();
            services.AddSingleton<ITenantHostingConfigService, TenantHostingConfigService>();
            services.Configure<TenancyHostingOptions>(configurationSection);

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTenantIdentification()
                .AddTenantIdentificationStrategy<IdTenantIdentifier>(builder => builder
                    .AddTenantTokenResolver<TenantIdHeaderHttpTokenResolver>("TenantId"));
        }
    }
}
