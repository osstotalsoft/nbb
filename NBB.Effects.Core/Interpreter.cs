using System;
using System.Threading.Tasks;

namespace NBB.Effects.Core
{
    public class Interpreter : IInterpreter
    {
        private readonly IServiceProvider _serviceProvider;

        public Interpreter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<T> Interpret<T>(IEffect<T> effect)
        {
            return InternalInterpret(effect as dynamic);
        }

        private Task<T> InternalInterpret<T>(PureEffect<T> effect)
        {
            return Task.FromResult(effect.Value);
        }
        private async Task<T> InternalInterpret<T1, T2, T>(ParallelEffect<T1, T2, T> parallelEffect)
        {
            var t1 = Interpret(parallelEffect.LeftEffect);
            var t2 = Interpret(parallelEffect.RightEffect);
            await Task.WhenAll(t1, t2);
            var nextEffect = parallelEffect.Next(t1.Result, t2.Result);
            return await Interpret(nextEffect);
        }

        private async Task<T> InternalInterpret<TOutput, T>(FreeEffect<TOutput, T> effect)
        {
            var sideEffectType = effect.SideEffect.GetType();
            var sideEffectHandlerType = typeof(ISideEffectHandler<,>).MakeGenericType(sideEffectType, typeof(TOutput));
            var sideEffectHandler = _serviceProvider.GetService(sideEffectHandlerType) as dynamic;
            var sideEffectResult = (TOutput)(await sideEffectHandler.Handle(effect.SideEffect));
            var innerEffect = effect.Next(sideEffectResult);
            return await Interpret(innerEffect);
        }

        
    }
}
