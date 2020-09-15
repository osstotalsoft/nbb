using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

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

        public static IServiceCollection AddSideEffectHandler<TOutput>(this IServiceCollection services, ISideEffectHandler<ISideEffect<TOutput>, TOutput> handler)
        {
            services.AddSingleton(handler);
            return services;
        }

        public static IServiceCollection AddSideEffectHandler<TSideEffect, TOutput>(this IServiceCollection services, Func<TSideEffect, CancellationToken, Task<TOutput>> handlerFn)
            where TSideEffect : ISideEffect<TOutput>
        {
            services.AddSingleton<ISideEffectHandler<TSideEffect, TOutput>>(new GenericSideEffectHandler<TSideEffect, TOutput>(handlerFn));
            return services;
        }

        public static IServiceCollection AddSideEffectHandler<TSideEffect, TOutput>(this IServiceCollection services, Func<TSideEffect, TOutput> handlerFn)
            where TSideEffect : ISideEffect<TOutput>
        {
            Task<TOutput> HandlerFnAsync(TSideEffect sideEffect, CancellationToken _cancellationToken)
            {
                return Task.FromResult(handlerFn(sideEffect));
            }

            services.AddSingleton<ISideEffectHandler<TSideEffect, TOutput>>(new GenericSideEffectHandler<TSideEffect, TOutput>(HandlerFnAsync));
            return services;
        }
    }
}
