using System;

namespace NBB.Effects.Core
{
    public interface IEffect<out T>
    {
        IEffect<TResult> Map<TResult>(Func<T, TResult> selector);
    }
}
