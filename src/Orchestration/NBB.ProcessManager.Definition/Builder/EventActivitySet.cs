using NBB.Core.Abstractions;
using System;
using System.Collections.Generic;

namespace NBB.ProcessManager.Definition.Builder
{

    public class EventActivitySet<TEvent, TData> : IEventActivitySet<TData>
        where TEvent : IEvent
        where TData : struct
    {
        private readonly List<EffectHandler<IEvent, TData>> _handlers = new List<EffectHandler<IEvent, TData>>();
        private readonly List<StateHandler<IEvent, TData>> _setStateHandlers = new List<StateHandler<IEvent, TData>>();
        private readonly EventPredicate<TEvent, TData> _whenPredicate;
        private EventPredicate<TEvent, TData> _completionPredicate;
        public bool StartsProcess { get; set; }
        public bool CompletesProcess { get; set; }

        public EventActivitySet(bool startsProcess, EventPredicate<TEvent, TData> whenPredicate = null)
        {
            StartsProcess = startsProcess;
            _whenPredicate = whenPredicate;
        }

        public Type EventType => typeof(TEvent);

        public IEnumerable<EffectHandler<IEvent, TData>> GetEffectHandlers()
        {
            return _handlers;
        }

        public IEnumerable<StateHandler<IEvent, TData>> GetStateHandlers()
        {
            return _setStateHandlers;
        }

        public void AddEffectHandler(EffectHandler<TEvent, TData> handler)
        {
            _handlers.Add((@event, data) => handler((TEvent) @event, data));
        }

        public void AddSetStateHandler(StateHandler<TEvent, TData> handler)
        {
            _setStateHandlers.Add((@event, data) => handler((TEvent) @event, data));
        }

        public EventPredicate<IEvent, TData> WhenPredicate
        {
            get
            {
                if (_whenPredicate == null)
                    return null;
                return (@event, data) => _whenPredicate((TEvent) @event, data);
            }
        }

        public EventPredicate<IEvent, TData> CompletionPredicate
        {
            get
            {
                if (_completionPredicate == null)
                    return null;
                return (@event, data) => _completionPredicate((TEvent) @event, data);
            }
        }

        public void UseForCompletion(EventPredicate<TEvent, TData> predicate)
        {
            _completionPredicate = predicate;
            CompletesProcess = true;
        }
    }

    public interface IEventActivitySet<TData>
        where TData : struct
    {
        Type EventType { get; }
        bool CompletesProcess { get; }
        bool StartsProcess { get; }
        EventPredicate<IEvent, TData> WhenPredicate { get; }
        EventPredicate<IEvent, TData> CompletionPredicate { get; }
        IEnumerable<EffectHandler<IEvent, TData>> GetEffectHandlers();
        IEnumerable<StateHandler<IEvent, TData>> GetStateHandlers();
    }
}