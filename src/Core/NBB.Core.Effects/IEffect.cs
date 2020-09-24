using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public interface IEffect<T>
    {
        IEffect<TResult> Map<TResult>(Func<T, TResult> selector);
        IEffect<TResult> Bind<TResult>(Func<T, IEffect<TResult>> computation);
        Task<TResult> Accept<TResult>(IEffectVisitor<T,TResult> v, CancellationToken cancellationToken);
    }

    public interface IEffect : IEffect<Unit>
    {
    }
}
