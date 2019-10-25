using NBB.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using MediatR;
using NBB.ProcessManager.Definition.Effects;

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
                return @event => func.CorrelationFilter((IEvent) @event);
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
                .Aggregate(EffectHelpers.Sequential);

            return (@event, data) => func?.Invoke((IEvent) @event, data) ?? NoEffect.Instance;
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

            return (@event, data) => func?.Invoke((IEvent) @event, data) ?? data.Data;
        }

        EventPredicate<TEvent, TData> IDefinition<TData>.GetStarterPredicate<TEvent>()
        {
            var act = _eventActivities.SingleOrDefault(x => x.EventType == typeof(TEvent) && x.StartsProcess);
            if (act == null)
                return (@event, data) => false;

            return (@event, data) => act.StarterPredicate?.Invoke((IEvent) @event, data) ?? true;
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
                .Aggregate((func1, func2) => (@event, data) => func2(@event, data) && func2(@event, data));

            return (@event, data) => func?.Invoke((IEvent) @event, data) ?? true;
        }
    }
}