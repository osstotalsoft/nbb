using NBB.MultiTenancy.Identification.Extensions;
using NBB.MultiTenancy.Identification.Messaging;

namespace NBB.Messaging.MultiTenancy
{
    public static class TenantTokenResolverConfigurationExtensions
    {
        public static TenantTokenResolverConfiguration AddMessagingTokenResolver(
            this TenantTokenResolverConfiguration strategyBuilder)
        {
            return strategyBuilder
                .AddTenantTokenResolver<TenantIdHeaderMessagingTokenResolver>(MessagingHeaders.TenantId);
        }
    }
}
