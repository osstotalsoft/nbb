using System;

namespace NBB.Core.Effects
{
    public class UnitEffect<T> : IEffect
    {
        public IEffect<T> InnerEffect { get; }

        public UnitEffect(IEffect<T> innerEffect)
        {
            InnerEffect = innerEffect;
        }

        public IEffect<TResult> Map<TResult>(Func<Unit, TResult> selector)
        {
            return InnerEffect.Map(_ => selector(Unit.Value));
        }

        public IEffect<TResult> Bind<TResult>(Func<Unit, IEffect<TResult>> computation)
        {
            return InnerEffect.Bind(_ => computation(Unit.Value));
        }
    }
}
