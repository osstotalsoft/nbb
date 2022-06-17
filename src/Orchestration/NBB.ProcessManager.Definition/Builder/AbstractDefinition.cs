// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using NBB.Core.Effects;

namespace NBB.ProcessManager.Definition.Builder;

public abstract class AbstractDefinition<TData> : IDefinition<TData>
{
    private readonly List<IEventActivitySet<TData>> _eventActivities = new();
    private readonly Dictionary<Type, EventCorrelation<object, TData>> _eventCorrelations = new();

    public IEventActivitySetBuilder<TEvent, TData> StartWith<TEvent>(EventPredicate<TEvent, TData> predicate = null)
    {
        var ea = new EventActivitySet<TEvent, TData>(true, predicate);
        _eventActivities.Add(ea);
        return new EventActivitySetBuilder<TEvent, TData>(ea);
    }

    public IEventActivitySetBuilder<TEvent, TData> When<TEvent>(EventPredicate<TEvent, TData> predicate = null)
    {
        var ea = new EventActivitySet<TEvent, TData>(false, predicate);
        _eventActivities.Add(ea);
        return new EventActivitySetBuilder<TEvent, TData>(ea);
    }

    public void Event<TEvent>(Action<EventCorrelationBuilder<TEvent, TData>> configureEventCorrelation)
    {
        Preconditions.NotNull(configureEventCorrelation, nameof(configureEventCorrelation));

        var configurator = new EventCorrelationBuilder<TEvent, TData>();
        configureEventCorrelation(configurator);
        var correl = configurator.Build();
        _eventCorrelations.Add(typeof(TEvent), new EventCorrelation<object, TData>(@event => correl.CorrelationFilter((TEvent)@event)));
    }

    Func<TEvent, object> IDefinition<TData>.GetCorrelationFilter<TEvent>()
    {
        if (_eventCorrelations.ContainsKey(typeof(TEvent)))
        {
            var func = _eventCorrelations[typeof(TEvent)];
            return @event => func.CorrelationFilter(@event);
        }

        return null;
    }

    IEnumerable<Type> IDefinition.GetEventTypes() => _eventActivities.Select(x => x.EventType).Distinct();

    EffectFunc<TEvent, TData> IDefinition<TData>.GetEffectFunc<TEvent>()
    {
        var func = _eventActivities
            .Where(x => x.EventType == typeof(TEvent))
            .Select(x => x.EffectFunc)
            .DefaultIfEmpty()
            .Aggregate(EffectFuncs.Sequential);

        return (@event, data) => func?.Invoke(@event, data) ?? Effect.Pure();
    }

    SetStateFunc<TEvent, TData> IDefinition<TData>.GetSetStateFunc<TEvent>()
    {
        var func = _eventActivities
            .Where(x => x.EventType == typeof(TEvent))
            .Select(x => x.SetStateFunc)
            .DefaultIfEmpty()
            .Aggregate((func1, func2) => (@event, data) =>
                {
                    var newData = func1(@event, data);
                    return func2(@event, new InstanceData<TData>(data.InstanceId, newData));
                }
            );

        return (@event, data) => func == null ? data.Data : func.Invoke(@event, data);
    }

    EventPredicate<TEvent, TData> IDefinition<TData>.GetStarterPredicate<TEvent>()
    {
        var act = _eventActivities.SingleOrDefault(x => x.EventType == typeof(TEvent) && x.StartsProcess);
        if (act == null)
            return (@event, data) => false;

        return (@event, data) => act.StarterPredicate?.Invoke(@event, data) ?? true;
    }

    EventPredicate<TEvent, TData> IDefinition<TData>.GetCompletionPredicate<TEvent>()
    {
        var acts = _eventActivities
            .Where(x => x.EventType == typeof(TEvent) && x.CompletesProcess)
            .ToList();

        if (acts.Count == 0)
            return (@event, data) => false;

        var func = acts.Select(x => x.CompletionPredicate)
            .DefaultIfEmpty()
            .Aggregate((func1, func2) => (@event, data) => func1(@event, data) && func2(@event, data));

        return (@event, data) => func?.Invoke(@event, data) ?? true;
    }
}
