using System;
using System.Threading;
using System.Threading.Tasks;

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

        public static IEffect<Unit> Pure()
        {
            return new PureEffect<Unit>(Unit.Value);
        }

        public static IEffect<T> From<T>(Func<CancellationToken, Task<T>> impure)
            => Of(Thunk.From(impure));

        public static IEffect<Unit> From(Func<CancellationToken, Task> impure)
            => Of(Thunk.From(impure));

        public static IEffect<TOutput> From<TOutput>(Func<TOutput> impure)
            => Of(Thunk.From(impure));

        public static IEffect<Unit> From(Action impure)
            => Of(Thunk.From(impure));

        public static IEffect<(T1, T2)> Parallel<T1, T2>(IEffect<T1> e1, IEffect<T2> e2)
        {
            return ParallelEffect<T1, T2, (T1, T2)>.From(e1, e2, (t1, t2) => (t1, t2));
        }

        public static IEffect<Unit> Parallel(IEffect<Unit> e1, IEffect<Unit> e2)
        {
            return ParallelEffect<Unit, Unit, Unit>.From(e1, e2, (_, __) => Unit.Value);
        }

        public static IEffect<TResult> Bind<T, TResult>(IEffect<T> effect, Func<T, IEffect<TResult>> computation)
        {
            return effect.Bind(computation);
        }

        public static IEffect<TResult> Map<T, TResult>(Func<T, TResult> selector, IEffect<T> effect)
        {
            return effect.Map(selector);
        }

        public static IEffect<TResult> Apply<T, TResult>(IEffect<Func<T, TResult>> fn, IEffect<T> effect)
        {
            return effect.Bind(x => fn.Map(f => f(x)));
        }

    }
}
