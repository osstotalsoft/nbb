using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Effects;

namespace NBB.Messaging.Effects
{
    public static class DependencyInjectionExtensions
    {
        public static void AddMessagingEffects(this IServiceCollection services)
        {
            services.AddSingleton<ISideEffectHandler<PublishMessage.SideEffect>, PublishMessage.Handler>();
        }
    }
}
