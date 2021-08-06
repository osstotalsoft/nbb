using Microsoft.Extensions.Configuration;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Nats;
using NBB.Messaging.Nats.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddNatsTransport(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<NatsOptions>(configuration.GetSection("Messaging").GetSection("Nats"));
            services.AddSingleton<StanConnectionProvider>();
            services.AddSingleton<IMessagingTransport, StanMessagingTransport>();

            return services;
        }
    }
}
