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

        public async Task<T> Interpret<T>(IEffect<T> effect, CancellationToken cancellationToken = default)
        {
            T result = default;
            var nextEffect = effect;
            while (nextEffect!=null)
            {
                if (nextEffect.GetType().IsGenericType &&
                    nextEffect.GetType().GetGenericTypeDefinition() == typeof(FreeEffect<,>))
                {
                    nextEffect = await InterpretFreeEffect(nextEffect as dynamic, cancellationToken);
                }
                else if (nextEffect.GetType().IsGenericType &&
                         nextEffect.GetType().GetGenericTypeDefinition() == typeof(ParallelEffect<,,>))
                {
                    nextEffect = await InterpretParallelEffect(nextEffect as dynamic, cancellationToken);
                }
                else if (nextEffect.GetType().IsGenericType &&
                         nextEffect.GetType().GetGenericTypeDefinition() == typeof(UnitEffect<>))
                {
                    nextEffect = InterpretUnitEffect(nextEffect as dynamic, Unit.Value);
                }
                else if (nextEffect is PureEffect<T> pureEff)
                {
                    result = InterpretPureEffect(pureEff);
                    nextEffect = null;
                }
            }

            return result;

        }

        // ReSharper disable once UnusedParameter.Local
        private T InterpretPureEffect<T>(PureEffect<T> effect)
        {
            return effect.Value;
        }

        private IEffect<T> InterpretUnitEffect<T, T1>(UnitEffect<T1> effect, T value)
        {
            return effect.InnerEffect.Map(_ => Unit.Value) as dynamic;
        }

        private async Task<IEffect<T>> InterpretParallelEffect<T1, T2, T>(ParallelEffect<T1, T2, T> parallelEffect, CancellationToken cancellationToken)
        {
            var t1 = Interpret(parallelEffect.LeftEffect, cancellationToken);
            var t2 = Interpret(parallelEffect.RightEffect, cancellationToken);
            await Task.WhenAll(t1, t2);
            var nextEffect = parallelEffect.Next(t1.Result, t2.Result);
            return nextEffect;
        }

        private async Task<IEffect<T>> InterpretFreeEffect<TOutput, T>(FreeEffect<TOutput, T> effect, CancellationToken cancellationToken)
        {
            var sideEffectResult = await _sideEffectMediator.Run(effect.SideEffect, cancellationToken);
            var innerEffect = effect.Next(sideEffectResult);
            return innerEffect;
        }
    }
}
