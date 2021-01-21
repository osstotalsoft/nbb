using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Nats.Internal;

namespace NBB.Messaging.Nats
{
    public static class DependencyInjectionExtensions
    {
        public static void AddNatsMessaging(this IServiceCollection services)
        {
            services.AddMessagingDefaults();
            services.AddSingleton<StanConnectionProvider>();
            services.AddSingleton<IMessagingTransport, StanMessagingTransport>();
        }
    }
}
