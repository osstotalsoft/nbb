using System;

namespace NBB.Core.Effects
{
    public static class Effect
    {
        public static IEffect<T> Of<T>(ISideEffect<T> sideEffect)
        {
            return new FreeEffect<T, T>(sideEffect, Pure);
        }

        public static IEffect<T> Pure<T>(T value)
        {
            return new PureEffect<T>(value);
        }

        public static IEffect<(T1, T2)> Parallel<T1, T2>(IEffect<T1> e1, IEffect<T2> e2)
        {
            return ParallelEffect<T1, T2, (T1, T2)>.From(e1, e2, (t1, t2) => (t1, t2));
        }

        public static IEffect<TResult> Then<T, TResult>(this IEffect<T> effect, Func<T, IEffect<TResult>> computation)
        {
            return effect.Bind(computation);
        }

        public static IEffect<TResult> Then<T, TResult>(this IEffect<T> effect, Func<T, TResult> selector)
        {
            return effect.Map(selector);
        }

        public static IEffect<TResult> Then<TResult>(this IEffect effect, IEffect<TResult> next)
        {
            return effect.Bind(_ => next);
        }

        public static IEffect ToUnit<T>(this IEffect<T> effect)
        {
            return new UnitEffect<T>(effect);
        }
    }
}
