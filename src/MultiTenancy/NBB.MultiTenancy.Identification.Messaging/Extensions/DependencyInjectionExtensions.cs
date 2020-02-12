using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.MultiTenancy;
using NBB.MultiTenancy.Identification.Extensions;
using NBB.MultiTenancy.Identification.Identifiers;

namespace NBB.MultiTenancy.Identification.Messaging.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDefaultMessagingTenantIdentification(this IServiceCollection services)
        {
            
            services.AddTenantIdentification()
                .AddTenantIdentificationStrategy<IdTenantIdentifier>(builder => builder
                    .AddTenantTokenResolver<TenantIdHeaderMessagingTokenResolver>(MessagingHeaders.TenantId));

            return services;
        }
    }
}
