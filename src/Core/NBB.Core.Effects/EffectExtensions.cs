using System;

namespace NBB.Core.Effects
{
    public static class EffectExtensions
    {
        public static IEffect<TResult> Then<T, TResult>(this IEffect<T> effect, Func<T, TResult> selector)
        {
            return effect.Map(selector);
        }

        public static IEffect<TResult> Then<T, TResult>(this IEffect<T> effect, Func<T, IEffect<TResult>> computation)
        {
            return effect.Bind(computation);
        }

        public static IEffect Then<T>(this IEffect<T> effect, Func<T, IEffect> computation)
        {
            return effect.Bind(computation).ToUnit();
        }

        public static IEffect<TResult> Then<TResult>(this IEffect effect, IEffect<TResult> next)
        {
            return effect.Bind(_ => next);
        }

        public static IEffect ToUnit<T>(this IEffect<T> effect)
        {
            return new UnitEffect<T>(effect);
        }

        public static IEffect<TResult> Select<T, TResult>(this IEffect<T> effect, Func<T, TResult> selector)
        {
            return effect.Map(selector);
        }

        public static IEffect<TResult> SelectMany<TSource1, TSource2, TResult>(this IEffect<TSource1> sourceEffect,
            Func<TSource1, IEffect<TSource2>> monadicFn,
            Func<TSource1, TSource2, TResult> resultSelector)
        {
            return sourceEffect.Bind(source => monadicFn(source).Map(x => resultSelector(source, x)));
        }
    }
}
