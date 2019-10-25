using NBB.Core.Abstractions;
using NBB.ProcessManager.Definition;
using NBB.ProcessManager.Runtime.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using NBB.ProcessManager.Definition.Effects;

namespace NBB.ProcessManager.Runtime
{
    public class Instance<TData>
        where TData : struct
    {
        private readonly IDefinition<TData> _definition;
        public TData Data { get; private set; }
        public InstanceStates State { get; private set; }
        public object InstanceId { get; private set; }


        private readonly List<IEvent> _changes = new List<IEvent>();
        private readonly List<IEffect> _effects = new List<IEffect>();
        public int Version { get; internal set; }

        public Instance(IDefinition<TData> definition)
        {
            _definition = definition;
        }

        private void StartProcess<TEvent>(TEvent @event)
            where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            var starter = _definition.GetStarterPredicate<TEvent>()(@event, GetInstanceData());
            if (!starter)
                throw new Exception($"Definition {_definition.GetType()} does not accept eventType {eventType} as a starter event.");

            var idSelector = _definition.GetCorrelationFilter<TEvent>();
            if (idSelector == null)
                throw new ArgumentNullException($"No correlation defined for eventType {eventType} in definition {_definition.GetType()}");

            if (State != InstanceStates.NotStarted)
                throw new Exception($"Cannot start an instance which is in state {State}");

            var identity = idSelector(@event);
            Emit(new ProcessStarted(identity));
        }

        public void ProcessEvent<TEvent>(TEvent @event)
            where TEvent : IEvent
        {
            var starter = _definition.GetStarterPredicate<TEvent>()(@event, GetInstanceData());

            if (State == InstanceStates.NotStarted && starter)
                StartProcess(@event);

            switch (State)
            {
                case InstanceStates.NotStarted:
                    return;
                case InstanceStates.Completed:
                case InstanceStates.Aborted:
                    throw new Exception($"Cannot accept a new event. Instance is {State}");
            }

            var effect = _definition.GetEffectFunc<TEvent>()(@event, GetInstanceData());
            _effects.Add(effect);

            Emit(new EventReceived<TEvent>(@event));

            if (_definition.GetCompletionPredicate<TEvent>()(@event, GetInstanceData()))
                Emit(new ProcessCompleted<TEvent>(@event));
        }

        private void Apply<TEvent>(EventReceived<TEvent> @event)
        {
            Data = _definition.GetSetStateFunc<TEvent>()(@event.ReceivedEvent, GetInstanceData());
        }

        private InstanceData<TData> GetInstanceData()
        {
            return new InstanceData<TData>(InstanceId, Data);
        }

        private void Apply(ProcessStarted @event)
        {
            State = InstanceStates.Started;
            InstanceId = @event.InstanceId;
        }

        private void Apply<TEvent>(ProcessCompleted<TEvent> @event)
        {
            State = InstanceStates.Completed;
        }

        private void ApplyChanges(dynamic @event, bool isNew)
        {
            Apply(@event);

            if (isNew)
            {
                _changes.Add(@event);
            }
            else
                Version++;
        }

        private void Emit(IEvent @event)
        {
            ApplyChanges(@event, true);
        }

        public string GetStreamFor(object identity)
        {
            return _definition.GetType().FullName + ":" + Data.GetType().FullName + ":" + identity;
        }

        public string GetStream()
        {
            return GetStreamFor(InstanceId);
        }

        public void LoadFromHistory(IEnumerable<object> events)
        {
            foreach (var @event in events)
            {
                ApplyChanges(@event, false);
            }
        }

        public IEnumerable<object> GetUncommittedChanges() => _changes;
        public IEnumerable<IEffect> GetUncommittedEffects() => _effects;

        public void MarkChangesAsCommitted()
        {
            Version += _changes.Count;
            _changes.Clear();
            _effects.Clear();
        }
    }
}