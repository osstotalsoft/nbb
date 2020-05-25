using Microsoft.Extensions.DependencyInjection;

namespace NBB.EventStore.Host
{
    public static class ServiceCollectionExtensions
    {
        public static EventStoreHostBuilder AddEventStoreHost(this IServiceCollection serviceCollection)
        {
            return new EventStoreHostBuilder(serviceCollection);
        }
    }
}
