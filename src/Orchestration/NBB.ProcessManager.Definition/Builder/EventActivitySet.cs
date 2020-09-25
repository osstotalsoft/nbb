using NBB.Core.Abstractions;
using System;
using NBB.Core.Effects;

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
        private static readonly EffectFunc<IEvent, TData> NoEffectFunc = (@event, data) => Effect.Pure();


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
            IEffect<Unit> NewFunc(IEvent @event, InstanceData<TData> data)
            {
                if (_starterPredicate != null && !_starterPredicate((TEvent) @event, data))
                    return Effect.Pure();
                return func((TEvent) @event, data);
            }

            EffectFunc = EffectFuncs.Sequential(EffectFunc, NewFunc);
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
                    if (_completionPredicate != null)
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