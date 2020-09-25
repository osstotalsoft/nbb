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

        public static IEffect<Unit> Then<T>(this IEffect<T> effect, Func<T, IEffect<Unit>> computation)
        {
            return effect.Bind(computation);
        }

        public static IEffect<TResult> Then<TResult>(this IEffect<Unit> effect, IEffect<TResult> next)
        {
            return effect.Bind(_ => next);
        }

        public static IEffect<Unit> Then(this IEffect<Unit> effect, IEffect<Unit> next)
        {
            return effect.Bind(_ => next);
        }

        public static IEffect<Unit> ToUnit<T>(this IEffect<T> effect)
        {
            return effect.Map(_ => Unit.Value);
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
