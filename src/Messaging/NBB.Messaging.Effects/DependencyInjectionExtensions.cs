using NBB.Core.Effects;
using NBB.Messaging.Effects;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMessagingEffects(this IServiceCollection services)
        {
            services.AddSingleton<ISideEffectHandler<PublishMessage.SideEffect, Unit>, PublishMessage.Handler>();
            return services;
        }
    }
}
