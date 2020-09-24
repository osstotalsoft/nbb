using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class UnitEffect : IEffect
    {
        public IEffect<Unit> InnerEffect { get; }

        public UnitEffect(IEffect<Unit> innerEffect)
        {
            InnerEffect = innerEffect;
        }

        public IEffect<TResult> Map<TResult>(Func<Unit, TResult> selector)
            => InnerEffect.Map(_ => selector(Unit.Value));

        public IEffect<TResult> Bind<TResult>(Func<Unit, IEffect<TResult>> computation)
            => InnerEffect.Bind(_ => computation(Unit.Value));

        public Task<TResult> Accept<TResult>(IEffectVisitor<Unit, TResult> v, CancellationToken cancellationToken)
            => v.Visit(this, cancellationToken);
    }
}
