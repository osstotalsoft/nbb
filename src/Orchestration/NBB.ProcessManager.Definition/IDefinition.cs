using System;
using System.Collections.Generic;
using NBB.ProcessManager.Definition.Effects;

namespace NBB.ProcessManager.Definition
{
    public interface IDefinition<TData> : IDefinition
        where TData : struct
    {
        EffectFunc<TEvent, TData> GetEffectFunc<TEvent>();
        SetStateFunc<TEvent, TData> GetSetStateFunc<TEvent>();
        Func<TEvent, object> GetCorrelationFilter<TEvent>();
        EventPredicate<TEvent, TData> GetStarterPredicate<TEvent>();
        EventPredicate<TEvent, TData> GetCompletionPredicate<TEvent>();
    }

    public interface IDefinition
    {
        IEnumerable<Type> GetEventTypes();
    }
}