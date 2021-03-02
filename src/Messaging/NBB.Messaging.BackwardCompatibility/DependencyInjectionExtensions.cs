using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.BackwardCompatibility
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection UseLegacyTopicRegistry(this IServiceCollection services)
        {
            services.Decorate<ITopicRegistry, LegacyTopicRegistryDecorator>();
            return services;
        }
    }
}
