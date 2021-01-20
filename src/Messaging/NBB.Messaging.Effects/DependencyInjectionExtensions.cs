using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Effects;

namespace NBB.Messaging.Effects
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
