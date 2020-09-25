using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class IterativeInterpreterVisitor<T> : IEffectVisitor<T, (IEffect<T>,T)>
    {
        private readonly ISideEffectMediator _sideEffectMediator;
        private readonly IInterpreter _interpreter;

        public IterativeInterpreterVisitor(ISideEffectMediator sideEffectMediator, IInterpreter interpreter)
        {
            _sideEffectMediator = sideEffectMediator;
            _interpreter = interpreter;
        }

        public Task<(IEffect<T>, T)> Visit(PureEffect<T> eff, CancellationToken cancellationToken)
            => Task.FromResult<(IEffect<T>, T)>((null, eff.Value));

        public async Task<(IEffect<T>,T)> Visit<TOutput>(FreeEffect<TOutput, T> eff, CancellationToken cancellationToken)
        {
            var sideEffectResult = await _sideEffectMediator.Run(eff.SideEffect, cancellationToken);
            var nextEffect = eff.Next(sideEffectResult);
            return (nextEffect, default);
        }

        public async Task<(IEffect<T>, T)> Visit<T1, T2>(ParallelEffect<T1, T2, T> eff, CancellationToken cancellationToken)
        {
            var t1 = _interpreter.Interpret(eff.LeftEffect, cancellationToken);
            var t2 = _interpreter.Interpret(eff.RightEffect, cancellationToken);
            await Task.WhenAll(t1, t2);
            var nextEffect = eff.Next(t1.Result, t2.Result);
            return (nextEffect, default);
        }
    }
}
