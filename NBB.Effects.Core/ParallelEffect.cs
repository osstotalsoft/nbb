using System;

namespace NBB.Effects.Core
{
    public class ParallelEffect<T1, T2, T> : IEffect<T>
    {
        public IEffect<T1> LeftEffect { get; }
        public IEffect<T2> RightEffect { get; }
        public Func<T1, T2, IEffect<T>> Next { get; }

        private ParallelEffect(IEffect<T1> leftEffect, IEffect<T2> rightEffect, Func<T1, T2, IEffect<T>> next)
        {
            LeftEffect = leftEffect;
            RightEffect = rightEffect;
            Next = next;
        }

        public static ParallelEffect<T1, T2, T> From(IEffect<T1> leftEffect, IEffect<T2> rightEffect, Func<T1, T2, T> selector)
        {
            return new ParallelEffect<T1, T2, T>(leftEffect, rightEffect, (t1, t2) => new PureEffect<T>(selector(t1, t2)));
        }


        public IEffect<TResult> Map<TResult>(Func<T, TResult> selector)
        {
            return new ParallelEffect<T1, T2, TResult>(LeftEffect, RightEffect, (t1, t2) => Next(t1, t2).Map(selector));
        }

        public IEffect<TResult> Bind<TResult>(Func<T, IEffect<TResult>> computation)
        {
            return new ParallelEffect<T1, T2, TResult>(LeftEffect, RightEffect, (t1, t2) => Next(t1, t2).Bind(computation));
        }
    }
}
