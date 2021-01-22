using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Nats.Internal;

namespace NBB.Messaging.Nats
{
    public static class DependencyInjectionExtensions
    {
        public static void AddNatsMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessagingDefaults();
            services.Configure<NatsOptions>(configuration.GetSection("Messaging").GetSection("Nats"));
            services.AddSingleton<StanConnectionProvider>();
            services.AddSingleton<IMessagingTransport, StanMessagingTransport>();
        }
    }
}
