using Microsoft.Extensions.DependencyInjection;

namespace NBB.Core.Effects
{
    public static class DependencyInjectionExtensions
    {
        public static void AddEffects(this IServiceCollection services)
        {
            services.AddScoped<ISideEffectHandlerFactory, SideEffectHandlerFactory>();
            services.AddScoped<IInterpreter, Interpreter>();
        }

    }
}
