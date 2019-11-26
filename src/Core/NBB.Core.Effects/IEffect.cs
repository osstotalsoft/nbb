using System;

namespace NBB.Core.Effects
{
    public interface IEffect<out T>
    {
        IEffect<TResult> Map<TResult>(Func<T, TResult> selector);
        IEffect<TResult> Bind<TResult>(Func<T, IEffect<TResult>> computation);
    }
}
