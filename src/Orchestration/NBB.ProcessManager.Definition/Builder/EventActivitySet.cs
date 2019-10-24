using NBB.Core.Abstractions;
using System;
using System.Collections.Generic;
using MediatR;
using NBB.ProcessManager.Definition.Effects;

namespace NBB.ProcessManager.Definition.Builder
{

    public class EventActivitySet<TEvent, TData> : IEventActivitySet<TData>
        where TEvent : IEvent
        where TData : struct
    {
        public EffectFunc<IEvent, TData> EffectFunc { get; set; }
        public SetStateFunc<IEvent, TData> SetStateFunc { get; set; }

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

        public void AddEffectHandler(EffectFunc<TEvent, TData> func)
        {
            if (EffectFunc == null)
                EffectFunc = (@event, data) => func((TEvent) @event, data);
            else
                EffectFunc = (@event, data) =>
                {
                    var ef1 = EffectFunc(@event, data);
                    var ef2 = func((TEvent) @event, data);

                    return new CombinedEffect(ef1, ef2);
                };
        }

        public void AddSetStateHandler(SetStateFunc<TEvent, TData> func)
        {
            SetStateFunc = (@event, data) => func((TEvent) @event, data);
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
        EffectFunc<IEvent, TData> EffectFunc { get; }
        SetStateFunc<IEvent, TData> SetStateFunc { get; }
    }
}