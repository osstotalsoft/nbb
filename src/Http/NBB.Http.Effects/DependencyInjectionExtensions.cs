using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Effects;

namespace NBB.Http.Effects
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddHttpEffects(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddSingleton<ISideEffectHandler<HttpGet.SideEffect, HttpResponseMessage>, HttpGet.Handler>();
            return services;
        }
    }
}
