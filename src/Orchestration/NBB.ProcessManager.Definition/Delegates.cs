using System;
using NBB.Core.Effects;

namespace NBB.ProcessManager.Definition
{
    public delegate IEffect EffectFunc<in TEvent, TData>(TEvent @event, InstanceData<TData> data) where TData : struct;

    public delegate TData SetStateFunc<in TEvent, TData>(TEvent @event, InstanceData<TData> data) where TData : struct;

    public delegate bool EventPredicate<in TEvent, TData>(TEvent @event, InstanceData<TData> data) where TData : struct;

    public static class EffectFuncs
    {
        public static EffectFunc<TEvent, TData> Aggregate<TEvent, TData>(EffectFunc<TEvent, TData> func1, EffectFunc<TEvent, TData> func2,
            Func<IEffect, IEffect, IEffect> accumulator)
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
            return Aggregate(func1, func2, (effect1, effect2) => effect1.Then(effect2).ToUnit());
        }
    }
}
