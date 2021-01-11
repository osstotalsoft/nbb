using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{


    public abstract class Effect<T>
    {
        public abstract Effect<TResult> Bind<TResult>(Func<T, Effect<TResult>> continuation);
        internal abstract Effect<TResult> Bind2<TResult>(EffectFnSeq<T, TResult> continuations);
        public abstract Task<TResult> Accept<TResult>(IVisitor<TResult> v, CancellationToken cancellationToken);

        public class Pure : Effect<T>
        {
            public T Value { get; }

            public Pure(T value)
            {
                Value = value;
            }

            public override Effect<TResult> Bind<TResult>(Func<T, Effect<TResult>> continuation)
                => continuation(Value);

            internal override Effect<TResult> Bind2<TResult>(EffectFnSeq<T, TResult> continuations)
                => continuations.Apply(Value);

            public override Task<TResult> Accept<TResult>(IVisitor<TResult> v, CancellationToken cancellationToken)
                => v.Visit(this, cancellationToken);
        }

        public class Impure<TSideEffect, TSideEffectResult> : Effect<T>
            where TSideEffect : ISideEffect<TSideEffectResult>
        {
            public TSideEffect SideEffect { get; }
            public EffectFnSeq<TSideEffectResult, T> Continuations { get; }

            public Impure(TSideEffect sideEffect, EffectFnSeq<TSideEffectResult, T> continuations)
            {
                SideEffect = sideEffect;
                Continuations = continuations;
            }

            public override Effect<TResult> Bind<TResult>(Func<T, Effect<TResult>> continuation)
                => new Effect<TResult>.Impure<TSideEffect, TSideEffectResult>(SideEffect, Continuations.Append(continuation));

            internal override Effect<TResult> Bind2<TResult>(EffectFnSeq<T, TResult> continuations)
                => new Effect<TResult>.Impure<TSideEffect, TSideEffectResult>(SideEffect, Continuations.AppendMany(continuations));

            public override Task<TResult> Accept<TResult>(IVisitor<TResult> v, CancellationToken cancellationToken)
                => v.Visit(this, cancellationToken);
        }

        public interface IVisitor<TResult>
        {
            Task<TResult> Visit(Pure eff, CancellationToken cancellationToken);
            Task<TResult> Visit<TSideEffect, TSideEffectResult>(Impure<TSideEffect, TSideEffectResult> eff, CancellationToken cancellationToken) 
                where TSideEffect : ISideEffect<TSideEffectResult>;
        }
    }

    public static class Effect
    {
        public static Effect<TSideEffectResult> Of<TSideEffect, TSideEffectResult>(TSideEffect sideEffect)
            where TSideEffect : ISideEffect<TSideEffectResult>
            => new Effect<TSideEffectResult>.Impure<TSideEffect, TSideEffectResult>(sideEffect, new EffectFnSeq<TSideEffectResult, TSideEffectResult>.Leaf(Pure));

        public static Effect<T> Pure<T>(T value)
            => new Effect<T>.Pure(value);

        public static Effect<Unit> Pure()
            => new Effect<Unit>.Pure(Unit.Value);

        public static Effect<T> From<T>(Func<CancellationToken, Task<T>> impure)
            => Of<Thunk.SideEffect<T>, T>(Thunk.From(impure));

        public static Effect<Unit> From(Func<CancellationToken, Task> impure)
            => Of<Thunk.SideEffect<Unit>, Unit>(Thunk.From(impure));

        public static Effect<T> From<T>(Func<T> impure)
            => Of<Thunk.SideEffect<T>, T>(Thunk.From(impure));

        public static Effect<Unit> From(Action impure)
            => Of<Thunk.SideEffect<Unit>, Unit>(Thunk.From(impure));

        public static Effect<(T1, T2)> Parallel<T1, T2>(Effect<T1> e1, Effect<T2> e2)
            => Of<Parallel.SideEffect<T1, T2>, (T1, T2)>(Effects.Parallel.From(e1, e2));

        public static Effect<Unit> Parallel(Effect<Unit> e1, Effect<Unit> e2)
            => Parallel<Unit, Unit>(e1, e2).ToUnit();

        public static Effect<IEnumerable<T>> Sequence<T>(IEnumerable<Effect<T>> effectList)
            => Of<Sequenced.SideEffect<T>, IEnumerable<T>>(Effects.Sequenced.From(effectList));

        public static Effect<Unit> Sequence(IEnumerable<Effect<Unit>> effectList)
            => Sequence<Unit>(effectList).ToUnit();

        public static Effect<TResult> Bind<T, TResult>(Effect<T> effect, Func<T, Effect<TResult>> computation)
            => effect.Bind(computation);

        public static Effect<TResult> Map<T, TResult>(Func<T, TResult> selector, Effect<T> effect)
         => effect.Map(selector);

        public static Effect<TResult> Apply<T, TResult>(Effect<Func<T, TResult>> fn, Effect<T> effect)
            //=> effect.Bind(x => fn.Map(f => f(x)));
            => Parallel(fn, effect).Map(pair => pair.Item1(pair.Item2));

        public static Func<T1, Effect<T3>> ComposeK<T1, T2, T3>(Func<T1, Effect<T2>> f, Func<T2, Effect<T3>> g) =>
            x => f(x).Bind(g);

    }
}
