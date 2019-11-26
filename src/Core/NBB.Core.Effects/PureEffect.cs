using System;

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
        {
            return new PureEffect<TResult>(selector(Value));
        }

        public IEffect<TResult> Bind<TResult>(Func<T, IEffect<TResult>> computation)
        {
            return computation(Value);
        }
    }
}
