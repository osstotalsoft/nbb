using Microsoft.Extensions.DependencyInjection;

namespace NBB.Core.Effects
{
    public static class DependencyInjectionExtensions
    {
        public static void AddEffects(this IServiceCollection services)
        {
            services.AddSingleton(typeof(Thunk.Handler<>));
            services.AddScoped<ISideEffectMediator, SideEffectMediator>();
            services.AddScoped<IInterpreter, Interpreter>();
        }

    }
}
