using System.Net.Http;
using NBB.Core.Effects;
using NBB.Http.Effects;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
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
