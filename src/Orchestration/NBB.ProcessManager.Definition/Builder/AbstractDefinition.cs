﻿using NBB.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using NBB.ProcessManager.Definition.Effects.Handlers;

namespace NBB.ProcessManager.Definition.Builder
{
    public abstract class AbstractDefinition<TData> : IDefinition<TData>
        where TData : struct
    {
        private readonly List<IEventActivitySet<TData>> _eventActivities = new List<IEventActivitySet<TData>>();
        private readonly Dictionary<Type, EventCorrelation<IEvent, TData>> _eventCorrelations = new Dictionary<Type, EventCorrelation<IEvent, TData>>();


        public EventActivitySetBuilder<TEvent, TData> StartWith<TEvent>(EventPredicate<TEvent, TData> predicate = null)
            where TEvent : IEvent
        {
            var ea = new EventActivitySet<TEvent, TData>(true, predicate);
            _eventActivities.Add(ea);
            return new EventActivitySetBuilder<TEvent, TData>(ea);
        }

        public EventActivitySetBuilder<TEvent, TData> When<TEvent>(EventPredicate<TEvent, TData> predicate = null)
            where TEvent : IEvent
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
            _eventCorrelations.Add(typeof(TEvent), new EventCorrelation<IEvent, TData>(@event => correl.CorrelationFilter((TEvent) @event)));
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

        IEnumerable<IEffectHandler<IEvent, TData>> IDefinition<TData>.GetEffectHandlers(Type eventType)
        {
            return _eventActivities
                .Where(x => x.EventType == eventType)
                .Select(x => x.EffectHandler)
                .Where(x => x != null);
        }

        IEnumerable<ValueTuple<EventPredicate<IEvent, TData>, SetStateFunc<IEvent, TData>>> IDefinition<TData>.GetSetStateFuncs(Type eventType)
        {
            return _eventActivities
                .Where(x => x.EventType == eventType)
                .Select(x => (x.WhenPredicate, x.SetStateFunc));
        }

        EventPredicate<TEvent, TData> IDefinition<TData>.GetStarterPredicate<TEvent>()
        {
            var act = _eventActivities
                .SingleOrDefault(x => x.EventType == typeof(TEvent) && x.StartsProcess);
            return (@event, data) =>
            {
                if (act == null)
                    return false;
                return act.WhenPredicate?.Invoke(@event, data) ?? true;
            };
        }

        IEnumerable<EventPredicate<TEvent, TData>> IDefinition<TData>.GetCompletionPredicates<TEvent>()
        {
            foreach (var x in _eventActivities)
            {
                if (x.EventType == typeof(TEvent) && x.CompletesProcess)
                    yield return (@event, data) =>
                    {
                        if (x.CompletionPredicate != null && x.WhenPredicate != null)
                            return x.CompletionPredicate(@event, data) && x.WhenPredicate(@event, data);
                        if (x.CompletionPredicate == null && x.WhenPredicate != null)
                            return x.WhenPredicate(@event, data);
                        if (x.CompletionPredicate != null && x.WhenPredicate == null)
                            return x.CompletionPredicate(@event, data);

                        return true;
                    };
            }
        }
    }
}