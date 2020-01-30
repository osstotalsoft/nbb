using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.MultiTenancy
{
    public static class DependencyInjectionExtensions
    {
        public static void AddMultiTenantMessaging(this IServiceCollection services)
        {
            services.Decorate<ITopicRegistry, MultiTenancyTopicRegistryDecorator>();
            services.Decorate<IMessageBusPublisher, MultiTenancyMessageBusPublisherDecorator>();
        }
    }
}
