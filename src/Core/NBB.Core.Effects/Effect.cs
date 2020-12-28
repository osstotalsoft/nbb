using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public abstract class Effect<T>
    {
        public abstract Effect<TResult> Bind<TResult>(Func<T, Effect<TResult>> continuation);
        internal abstract Effect<TResult> Bind2<TResult>(EffectFnSeq<T, TResult> continuations);
        internal abstract Task<(Effect<T>, T)> Run(ISideEffectBroker sideEffectBroker, CancellationToken cancellationToken);
    }

    public class PureEffect<T> : Effect<T>
    {
        public T Value { get; }

        public PureEffect(T value)
        {
            Value = value;
        }

        public override Effect<TResult> Bind<TResult>(Func<T, Effect<TResult>> continuation)
            => continuation(Value);

        internal override Effect<TResult> Bind2<TResult>(EffectFnSeq<T, TResult> continuations)
            => continuations.Apply(Value);

        internal override Task<(Effect<T>, T)> Run(ISideEffectBroker sideEffectBroker, CancellationToken cancellationToken)
            => Task.FromResult<(Effect<T>, T)>((null, Value));
    }

    public class ImpureEffect<TOutput, T> : Effect<T>
    {
        public ISideEffect<TOutput> SideEffect { get; }
        public EffectFnSeq<TOutput, T> Continuations { get; }

        public ImpureEffect(ISideEffect<TOutput> sideEffect, EffectFnSeq<TOutput, T> continuations)
        {
            SideEffect = sideEffect;
            Continuations = continuations;
        }

        public override Effect<TResult> Bind<TResult>(Func<T, Effect<TResult>> continuation)
            => new ImpureEffect<TOutput, TResult>(SideEffect, Continuations.Append(continuation));

        internal override Effect<TResult> Bind2<TResult>(EffectFnSeq<T, TResult> continuations)
            => new ImpureEffect<TOutput, TResult>(SideEffect, Continuations.AppendMany(continuations));

        internal override async Task<(Effect<T>, T)> Run(ISideEffectBroker sideEffectBroker, CancellationToken cancellationToken)
        {
            var sideEffectResult = await sideEffectBroker.Run(this.SideEffect, cancellationToken);
            var nextEffect = this.Continuations.Apply(sideEffectResult);
            return (nextEffect, default);
        }
    }

    public static class Effect
    {
        public static Effect<T> Of<T>(ISideEffect<T> sideEffect)
            => new ImpureEffect<T, T>(sideEffect, new EffectFnSeq<T, T>.Leaf(Pure));

        public static Effect<T> Pure<T>(T value)
            => new PureEffect<T>(value);

        public static Effect<Unit> Pure()
            => new PureEffect<Unit>(Unit.Value);

        public static Effect<T> From<T>(Func<CancellationToken, Task<T>> impure)
            => Of(Thunk.From(impure));

        public static Effect<Unit> From(Func<CancellationToken, Task> impure)
            => Of(Thunk.From(impure));

        public static Effect<TOutput> From<TOutput>(Func<TOutput> impure)
            => Of(Thunk.From(impure));

        public static Effect<Unit> From(Action impure)
            => Of(Thunk.From(impure));

        public static Effect<(T1, T2)> Parallel<T1, T2>(Effect<T1> e1, Effect<T2> e2)
            => Of(Effects.Parallel.From(e1, e2));

        public static Effect<Unit> Parallel(Effect<Unit> e1, Effect<Unit> e2)
            => Of(Effects.Parallel.From(e1, e2)).Map(_ => Unit.Value);

        public static Effect<TResult> Bind<T, TResult>(Effect<T> effect, Func<T, Effect<TResult>> computation)
            => effect.Bind(computation);

        public static Effect<TResult> Map<T, TResult>(Func<T, TResult> selector, Effect<T> effect)
         => effect.Map(selector);

        public static Effect<TResult> Apply<T, TResult>(Effect<Func<T, TResult>> fn, Effect<T> effect)
            => effect.Bind(x => fn.Map(f => f(x)));

        public static Func<T1, Effect<T3>> ComposeK<T1, T2, T3>(Func<T1, Effect<T2>> f, Func<T2, Effect<T3>> g) =>
            x => f(x).Bind(g);

    }
}
