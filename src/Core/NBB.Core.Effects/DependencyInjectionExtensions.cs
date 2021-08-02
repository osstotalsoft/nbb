using System;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Effects;
using Parallel = NBB.Core.Effects.Parallel;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddEffects(this IServiceCollection services)
        {
            services.AddSingleton(typeof(Thunk.Handler<>));
            services.AddScoped(typeof(Parallel.Handler<,>));
            services.AddScoped(typeof(Sequenced.Handler<>));
            services.AddScoped(typeof(TryWith.Handler<>));
            services.AddScoped(typeof(TryFinally.Handler<>));
            services.AddScoped<ISideEffectBroker, SideEffectBroker>();
            services.AddScoped<IInterpreter, Interpreter>();
            return services;
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
            Task<TOutput> HandlerFnAsync(TSideEffect sideEffect, CancellationToken cancellationToken)
            {
                return Task.FromResult(handlerFn(sideEffect));
            }

            services.AddSingleton<ISideEffectHandler<TSideEffect, TOutput>>(new GenericSideEffectHandler<TSideEffect, TOutput>(HandlerFnAsync));
            return services;
        }
    }
}
