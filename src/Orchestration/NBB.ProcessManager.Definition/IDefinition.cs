using System;
using System.Collections.Generic;
using NBB.Core.Abstractions;

namespace NBB.ProcessManager.Definition
{
    public interface IDefinition<TData> : IDefinition
        where TData : struct
    {
        IEnumerable<ValueTuple<EventPredicate<IEvent, TData>, IEnumerable<EffectHandler<IEvent, TData>>>> GetEffectHandlers(Type eventType);
        IEnumerable<ValueTuple<EventPredicate<IEvent, TData>, IEnumerable<StateHandler<IEvent, TData>>>> GetStateHandlers(Type eventType);
        Func<TEvent, object> GetCorrelationFilter<TEvent>() where TEvent : IEvent;
        EventPredicate<TEvent, TData> GetStarterPredicate<TEvent>() where TEvent : IEvent;
        IEnumerable<EventPredicate<TEvent, TData>> GetCompletionPredicates<TEvent>() where TEvent : IEvent;
    }

    public interface IDefinition
    {
        IEnumerable<Type> GetEventTypes();
    }
}