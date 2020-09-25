using System;

namespace NBB.Core.Effects
{
    public static class EffectExtensions
    {
        public static Effect<TResult> Map<T, TResult>(this Effect<T> effect, Func<T, TResult> selector)
            => effect.Bind(Fun.Compose(Effect.Pure, selector));

        public static Effect<TResult> Then<T, TResult>(this Effect<T> effect, Func<T, TResult> selector)
            => effect.Map(selector);

        public static Effect<TResult> Then<T, TResult>(this Effect<T> effect, Func<T, Effect<TResult>> computation)
            => effect.Bind(computation);

        public static Effect<Unit> Then<T>(this Effect<T> effect, Func<T, Effect<Unit>> computation)
            => effect.Bind(computation);

        public static Effect<TResult> Then<TResult>(this Effect<Unit> effect, Effect<TResult> next)
            => effect.Bind(_ => next);

        public static Effect<Unit> Then(this Effect<Unit> effect, Effect<Unit> next)
            => effect.Bind(_ => next);

        public static Effect<Unit> ToUnit<T>(this Effect<T> effect)
            => effect.Map(_ => Unit.Value);

        public static Effect<TResult> Select<T, TResult>(this Effect<T> effect, Func<T, TResult> selector)
            => effect.Map(selector);

        public static Effect<TResult> SelectMany<TSource1, TSource2, TResult>(this Effect<TSource1> sourceEffect,
            Func<TSource1, Effect<TSource2>> monadicFn,
            Func<TSource1, TSource2, TResult> resultSelector)
            => sourceEffect.Bind(source => monadicFn(source).Map(x => resultSelector(source, x)));
    }
}
