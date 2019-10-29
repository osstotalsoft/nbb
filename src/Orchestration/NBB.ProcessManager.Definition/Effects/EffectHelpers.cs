using System;
using System.Threading.Tasks;
using MediatR;

namespace NBB.ProcessManager.Definition.Effects
{
 
    public static class EffectHelpers
    {
        public static IEffect<TResult[]> WhenAll<TResult>(params IEffect<TResult>[] effects)
        {
            return new ParallelEffect<TResult>(effects);
        }

        public static IEffect WhenAll<TResult>(params IEffect[] effects)
        {
            return new ParallelEffect<TResult>(effects);
        }

        public static IEffect<TResult[]> WhenAny<TResult>(params IEffect<TResult>[] effects)
        {
            throw new NotImplementedException();
        }

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
            return Aggregate(func1, func2, (effect1, effect2) => new SequentialEffect(effect1, effect2));
        }
    }
}