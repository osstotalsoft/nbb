using MediatR;
using System;

namespace NBB.ProcessManager.Definition.Effects
{

    public static class EffectFuncs
    {
        public static EffectFunc<TEvent, TData> Aggregate<TEvent, TData>(EffectFunc<TEvent, TData> func1, EffectFunc<TEvent, TData> func2,
            Func<IEffect<Unit>, IEffect<Unit>, IEffect<Unit>> accumulator)
            where TData : struct
        {
            return (@event, data) =>
            {
                var ef1 = func1(@event, data);
                var ef2 = func2(@event, data);

                return accumulator(ef1, ef2);
            };
        }

        public static EffectFunc<TEvent, TData> Sequential<TEvent, TData>(EffectFunc<TEvent, TData> func1, EffectFunc<TEvent, TData> func2)
            where TData : struct
        {
            return Aggregate(func1, func2, (effect1, effect2) => new Effect<Unit>(async runner =>
            {
                await effect1.Computation(runner);
                await effect2.Computation(runner);
                return Unit.Value;
            }));
        }
    }

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

        public static IEffect<TEffectResult2> SelectMany<TEffectResult1, TEffectResult2>(this IEffect<TEffectResult1> effect,
            Func<TEffectResult1, IEffect<TEffectResult2>> continuation)
        {
            return effect.ContinueWith(continuation);
        }

        public static IEffect<TResult> SelectMany<TSource, TCollection, TResult>(this IEffect<TSource> sourceEffect,
            Func<TSource, IEffect<TCollection>> collectionSelector,
            Func<TSource, TCollection, TResult> resultSelector)
        {
            return sourceEffect.ContinueWith(source => collectionSelector(source).Select(collection => resultSelector(source, collection)));
        }
    }
}