using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class Interpreter : IInterpreter
    {
        private readonly SideEffectHandlerFactory _sideEffectHandlerFactory;

        public Interpreter(SideEffectHandlerFactory sideEffectHandlerFactory)
        {
            _sideEffectHandlerFactory = sideEffectHandlerFactory;
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
            var sideEffectHandler = _sideEffectHandlerFactory.GetSideEffectHandlerFor(effect.SideEffect);
            var sideEffectResult = await sideEffectHandler.Handle(effect.SideEffect);
            var innerEffect = effect.Next(sideEffectResult);
            return await Interpret(innerEffect);
        }
    }
}
