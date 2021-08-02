using NBB.Application.MediatR.Effects;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
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
