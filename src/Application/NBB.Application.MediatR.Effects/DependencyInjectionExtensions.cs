using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Effects;

namespace NBB.Application.MediatR.Effects
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMediatorEffects(this IServiceCollection services)
        {
            services.AddSingleton(typeof(MediatorEffects.Send.Handler<>));
            services.AddSingleton<ISideEffectHandler<MediatorEffects.Publish.SideEffect, Unit>, MediatorEffects.Publish.Handler>();
            return services;
        }
    }
}
