using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.MultiTenancy
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMultiTenantMessaging(this IServiceCollection services)
        {
            services.Decorate<IMessageBusPublisher, MultiTenancyMessageBusPublisherDecorator>();

            return services;
        }
    }
}
