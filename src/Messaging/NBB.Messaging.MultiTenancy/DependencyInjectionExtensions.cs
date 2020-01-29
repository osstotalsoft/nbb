using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NBB.Messaging.Abstractions;
using NBB.MultiTenancy.Abstractions.Hosting;

namespace NBB.Messaging.MultiTenancy
{
    public static class DependencyInjectionExtensions
    {
        public static void AddMultiTenantMessaging(this IServiceCollection services)
        {
            services.Decorate<ITopicRegistry, MultiTenancyTopicRegistryDecorator>();
            services.Decorate<IMessageBusPublisher, MultiTenancyMessageBusPublisherDecorator>();

            services.TryAddSingleton<TenancyHostingValidator>();
        }
    }
}
