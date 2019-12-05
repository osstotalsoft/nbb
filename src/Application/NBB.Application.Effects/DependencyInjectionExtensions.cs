using Microsoft.Extensions.DependencyInjection;

namespace NBB.Application.Effects
{
    public static class DependencyInjectionExtensions
    {
        public static void AddMediatorEffects(this IServiceCollection services)
        {
            services.AddSingleton(typeof(MediatorSendQuery.Handler<>));
        }
    }
}
