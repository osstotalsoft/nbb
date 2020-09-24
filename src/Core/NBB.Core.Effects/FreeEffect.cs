using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class FreeEffect<TOutput, T> : IEffect<T>
    {
        public ISideEffect<TOutput> SideEffect { get; }
        public Func<TOutput, IEffect<T>> Next { get; }

        public FreeEffect(ISideEffect<TOutput> sideEffect, Func<TOutput, IEffect<T>> next)
        {
            SideEffect = sideEffect;
            Next = next;
        }

        public IEffect<TResult> Bind<TResult>(Func<T, IEffect<TResult>> computation)
            => new FreeEffect<TOutput, TResult>(SideEffect, response => Next(response).Bind(computation));


        public IEffect<TResult> Map<TResult>(Func<T, TResult> selector)
            => new FreeEffect<TOutput, TResult>(SideEffect, response => Next(response).Map(selector));

        public Task<TResult> Accept<TResult>(IEffectVisitor<T, TResult> v, CancellationToken cancellationToken)
            => v.Visit(this, cancellationToken);
    }
}
