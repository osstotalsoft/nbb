using Microsoft.Extensions.DependencyInjection;

namespace NBB.Application.MediatR.Effects
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMediatorEffects(this IServiceCollection services)
        {
            services.AddSingleton(typeof(MediatorSendQuery.Handler<>));
            return services;
        }
    }
}
