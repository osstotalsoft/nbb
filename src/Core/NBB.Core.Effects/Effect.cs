﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public abstract class Effect<T>
    { 
        public abstract Effect<TResult> Bind<TResult>(Func<T, Effect<TResult>> continuation);
        public abstract Effect<TResult> Bind2<TResult>(EffectFnSeq<T, TResult> continuations);

        public abstract Task<TResult> Accept<TResult>(IEffectVisitor<T,TResult> v, CancellationToken cancellationToken);
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

        public override Effect<TResult> Bind2<TResult>(EffectFnSeq<T, TResult> continuations)
            => continuations.Apply(Value);

        public override Task<TResult> Accept<TResult>(IEffectVisitor<T, TResult> v, CancellationToken cancellationToken)
            => v.Visit(this, cancellationToken);

        
    }

    public class FreeEffect<TOutput, T> : Effect<T>
    {
        public ISideEffect<TOutput> SideEffect { get; }
        public EffectFnSeq<TOutput, T> Continuations { get; }

        public FreeEffect(ISideEffect<TOutput> sideEffect, EffectFnSeq<TOutput, T> continuations)
        {
            SideEffect = sideEffect;
            Continuations = continuations;
        }

        public override Effect<TResult> Bind<TResult>(Func<T, Effect<TResult>> continuation)
            => new FreeEffect<TOutput, TResult>(SideEffect, Continuations.Append(continuation));
        
        public override Effect<TResult> Bind2<TResult>(EffectFnSeq<T, TResult> continuations)
            => new FreeEffect<TOutput, TResult>(SideEffect, Continuations.AppendMany(continuations));

        public override Task<TResult> Accept<TResult>(IEffectVisitor<T, TResult> v, CancellationToken cancellationToken)
            => v.Visit(this, cancellationToken);

        
    }

    public class ParallelEffect<T1, T2, T> : Effect<T>
    {
        public Effect<T1> LeftEffect { get; }
        public Effect<T2> RightEffect { get; }
        public Func<T1, T2, Effect<T>> Next { get; }

        public ParallelEffect(Effect<T1> leftEffect, Effect<T2> rightEffect, Func<T1, T2, Effect<T>> next)
        {
            LeftEffect = leftEffect;
            RightEffect = rightEffect;
            Next = next;
        }

        public override Effect<TResult> Bind<TResult>(Func<T, Effect<TResult>> continuation)
            => new ParallelEffect<T1, T2, TResult>(LeftEffect, RightEffect, (t1, t2) => Next(t1, t2).Bind(continuation));
        
        public override Effect<TResult> Bind2<TResult>(EffectFnSeq<T, TResult> continuation)
            => new ParallelEffect<T1, T2, TResult>(LeftEffect, RightEffect, (t1, t2) => Next(t1, t2).Bind2(continuation));

        public override Task<TResult> Accept<TResult>(IEffectVisitor<T, TResult> v, CancellationToken cancellationToken)
            => v.Visit(this, cancellationToken);

        
    }

    public static class Effect
    {
        public static Effect<T> Of<T>(ISideEffect<T> sideEffect)
            => new FreeEffect<T, T>(sideEffect, new EffectFnSeq<T, T>.Leaf(Pure));

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
            => new ParallelEffect<T1, T2, (T1, T2)>(e1, e2, (t1, t2) => Pure((t1, t2)));

        public static Effect<Unit> Parallel(Effect<Unit> e1, Effect<Unit> e2)
            => new ParallelEffect<Unit, Unit, Unit>(e1, e2, (_, __) => Pure());

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
