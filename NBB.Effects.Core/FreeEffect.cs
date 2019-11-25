using System;

namespace NBB.Effects.Core
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
        {
            return new FreeEffect<TOutput, TResult>(SideEffect, response => Next(response).Bind(computation));
        }

        public IEffect<TResult> Map<TResult>(Func<T, TResult> selector)
        {
            return new FreeEffect<TOutput, TResult>(SideEffect, response => Next(response).Map(selector));
        }
    }
}
