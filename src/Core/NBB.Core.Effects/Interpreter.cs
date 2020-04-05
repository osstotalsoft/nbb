using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class Interpreter : IInterpreter
    {
        private readonly ISideEffectMediator _sideEffectMediator;

        public Interpreter(ISideEffectMediator sideEffectMediator)
        {
            _sideEffectMediator = sideEffectMediator;
        }

        public Task<T> Interpret<T>(IEffect<T> effect, CancellationToken cancellationToken = default)
        {
            return InternalInterpret(effect as dynamic, cancellationToken);
        }

        // ReSharper disable once UnusedParameter.Local
        private Task<T> InternalInterpret<T>(PureEffect<T> effect, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(effect.Value);
        }

        private async Task<Unit> InternalInterpret<T>(UnitEffect<T> effect, CancellationToken cancellationToken = default)
        {
            await Interpret(effect.InnerEffect, cancellationToken);
            return Unit.Value;
        }

        private async Task<T> InternalInterpret<T1, T2, T>(ParallelEffect<T1, T2, T> parallelEffect, CancellationToken cancellationToken = default)
        {
            var t1 = Interpret(parallelEffect.LeftEffect, cancellationToken);
            var t2 = Interpret(parallelEffect.RightEffect, cancellationToken);
            await Task.WhenAll(t1, t2);
            var nextEffect = parallelEffect.Next(t1.Result, t2.Result);
            return await Interpret(nextEffect, cancellationToken);
        }

        private async Task<T> InternalInterpret<TOutput, T>(FreeEffect<TOutput, T> effect, CancellationToken cancellationToken = default)
        {
            var sideEffectResult = await _sideEffectMediator.Run(effect.SideEffect, cancellationToken);
            var innerEffect = effect.Next(sideEffectResult);
            return await Interpret(innerEffect, cancellationToken);
        }
    }
}
