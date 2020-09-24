using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class PureEffect<T> : IEffect<T>
    {
        public T Value { get; }

        public PureEffect(T value)
        {
            Value = value;
        }

        public IEffect<TResult> Map<TResult>(Func<T, TResult> selector)
            => new PureEffect<TResult>(selector(Value));

        public IEffect<TResult> Bind<TResult>(Func<T, IEffect<TResult>> computation)
            => computation(Value);

        public Task<TResult> Accept<TResult>(IEffectVisitor<T, TResult> v, CancellationToken cancellationToken)
            => v.Visit(this, cancellationToken);
    }
}
