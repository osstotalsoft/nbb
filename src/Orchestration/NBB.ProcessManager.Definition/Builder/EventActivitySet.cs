using NBB.Core.Abstractions;
using NBB.ProcessManager.Definition.Effects;
using System;

namespace NBB.ProcessManager.Definition.Builder
{

    public class EventActivitySet<TEvent, TData> : IEventActivitySet<TData>
        where TEvent : IEvent
        where TData : struct
    {
        public EffectFunc<IEvent, TData> EffectFunc { get; set; }
        public SetStateFunc<IEvent, TData> SetStateFunc { get; set; }
        public bool StartsProcess { get; set; }
        public bool CompletesProcess { get; set; }

        private readonly EventPredicate<TEvent, TData> _starterPredicate;
        private EventPredicate<TEvent, TData> _completionPredicate;

        private static readonly SetStateFunc<IEvent, TData> NoSetStateFunc = (@event, data) => data.Data;
        private static readonly EffectFunc<IEvent, TData> NoEffectFunc = (@event, data) => NoEffect.Instance;


        public EventActivitySet(bool startsProcess, EventPredicate<TEvent, TData> starterPredicate = null)
        {
            StartsProcess = startsProcess;
            _starterPredicate = starterPredicate;
            EffectFunc = NoEffectFunc;
            SetStateFunc = NoSetStateFunc;
        }

        public Type EventType => typeof(TEvent);

        public void AddEffectHandler(EffectFunc<TEvent, TData> func)
        {
            IEffect newFunc(IEvent @event, InstanceData<TData> data)
            {
                if (_starterPredicate != null && !_starterPredicate((TEvent) @event, data))
                    return NoEffect.Instance;
                return func((TEvent) @event, data);
            }

            EffectFunc = EffectHelpers.Sequential(EffectFunc, newFunc);
        }

        public void AddSetStateHandler(SetStateFunc<TEvent, TData> func)
        {
            SetStateFunc = (@event, data) =>
            {
                var newData = data.Data;
                if (_starterPredicate == null || _starterPredicate((TEvent) @event, data))
                    newData = func((TEvent) @event, data);

                return newData;
            };
        }

        public EventPredicate<IEvent, TData> StarterPredicate
        {
            get
            {
                if (_starterPredicate == null)
                    return null;
                return (@event, data) => _starterPredicate((TEvent) @event, data);
            }
        }

        public EventPredicate<IEvent, TData> CompletionPredicate
        {
            get
            {
                return (@event, data) =>
                {
                    if (_completionPredicate != null && _starterPredicate != null)
                        return _completionPredicate((TEvent) @event, data) && _starterPredicate((TEvent) @event, data);
                    if (_completionPredicate == null && _starterPredicate != null)
                        return _starterPredicate((TEvent) @event, data);
                    if (_completionPredicate != null && _starterPredicate == null)
                        return _completionPredicate((TEvent) @event, data);

                    return true;
                };
            }
        }


        public void UseForCompletion(EventPredicate<TEvent, TData> predicate = null)
        {
            _completionPredicate = predicate;
            CompletesProcess = true;
        }
    }

    public static class EffectHelpers
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
            return Aggregate(func1, func2, (effect1, effect2) => new SequentialEffect(effect1, effect2));
        }
    }

    public interface IEventActivitySet<TData>
        where TData : struct
    {
        Type EventType { get; }
        bool CompletesProcess { get; }
        bool StartsProcess { get; }
        EventPredicate<IEvent, TData> StarterPredicate { get; }
        EventPredicate<IEvent, TData> CompletionPredicate { get; }
        EffectFunc<IEvent, TData> EffectFunc { get; }
        SetStateFunc<IEvent, TData> SetStateFunc { get; }
    }
}