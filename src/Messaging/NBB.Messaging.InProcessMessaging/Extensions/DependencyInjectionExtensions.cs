using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using NBB.Messaging.InProcessMessaging.Internal;

namespace NBB.Messaging.InProcessMessaging.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void AddInProcessTransport(this IServiceCollection services)
        {
            services.AddSingleton<IStorage, Storage>();
            services.AddSingleton<IMessagingTransport, InProcessMessagingTransport>();
        }
    }
}
