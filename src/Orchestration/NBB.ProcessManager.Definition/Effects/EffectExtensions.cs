using System;
using System.Collections.Generic;
using System.Text;

namespace NBB.ProcessManager.Definition.Effects
{
    public static class EffectExtensions
    {
        public static IEffect<TEffectResult2> ContinueWith<TEffectResult1, TEffectResult2>(this IEffect<TEffectResult1> effect,
            Func<TEffectResult1, IEffect<TEffectResult2>> continuation)
        {
            return new Effect<TEffectResult2>(async runner =>
            {
                var r1 = await effect.Computation(runner);
                return await continuation(r1).Computation(runner);
            });
        }

        public static IEffect<TResult> Select<T, TResult>(this IEffect<T> effect, Func<T, TResult> selector)
        {
            return new Effect<TResult>(async runner =>
            {
                var r1 = await effect.Computation(runner);
                return selector(r1);
            });
        }

        public static IEffect<TResult> SelectMany<TSource1, TSource2, TResult>(this IEffect<TSource1> sourceEffect,
            Func<TSource1, IEffect<TSource2>> monadicFn,
            Func<TSource1, TSource2, TResult> resultSelector)
        {
            return sourceEffect.ContinueWith(source => monadicFn(source).Select(x => resultSelector(source, x)));
        }
    }
}