using NBB.Core.Abstractions;
using NBB.ProcessManager.Definition;
using NBB.ProcessManager.Runtime.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NBB.ProcessManager.Runtime
{
    public class Instance<TData>
        where TData : struct
    {
        private readonly IDefinition<TData> _definition;
        public InstanceData<TData> InstanceData { get; }
        public InstanceStates State { get; private set; }

        private readonly List<IEvent> _changes = new List<IEvent>();
        private readonly List<IEffect> _effects = new List<IEffect>();
        public int Version { get; internal set; }

        public Instance(IDefinition<TData> definition)
        {
            _definition = definition;
            InstanceData = new InstanceData<TData>();
        }

        private void StartProcess<TEvent>(TEvent @event)
            where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            var starter = _definition.GetStarterPredicate<TEvent>()(@event, InstanceData);
            if (!starter)
                throw new Exception($"Definition {_definition.GetType()} does not accept eventType {eventType} as a starter event.");

            var idSelector = _definition.GetCorrelationFilter<TEvent>();
            if (idSelector == null)
                throw new ArgumentNullException($"No correlation defined for eventType {eventType} in definition {_definition.GetType()}");

            if (State != InstanceStates.NotStarted)
                throw new Exception($"Cannot start an instance which is in state {State}");

            var correlationId = idSelector(@event);
            Emit(new ProcessStarted(correlationId));
        }

        public void ProcessEvent<TEvent>(TEvent @event)
            where TEvent : IEvent
        {
            var eventType = typeof(TEvent);
            var starter = _definition.GetStarterPredicate<TEvent>()(@event, InstanceData);

            if (State == InstanceStates.NotStarted && starter)
                StartProcess(@event);

            if (State == InstanceStates.Completed || State == InstanceStates.Aborted)
                throw new Exception($"Cannot accept a new event. Instance is {State}");

            if (State != InstanceStates.Started)
                return;

            var effectHandlers = _definition.GetEffectHandlers(eventType);
            foreach (var (pred, handlers) in effectHandlers)
            {
                if (pred != null && !pred(@event, InstanceData))
                    continue;

                foreach (var handler in handlers)
                {
                    var effect = handler(@event, InstanceData);
                    _effects.Add(effect);
                }
            }

            Emit(new EventReceived<TEvent>(@event));

            if (_definition.GetCompletionPredicates<TEvent>().Any(x => x(@event, InstanceData)))
                Emit(new ProcessCompleted<TEvent>(@event));
        }

        private void Apply<TEvent>(EventReceived<TEvent> @event)
        {
            var stateHandlers = _definition.GetStateHandlers(@event.ReceivedEvent.GetType());
            foreach (var (pred, handlers) in stateHandlers)
            {
                if (pred != null && !pred((IEvent) @event.ReceivedEvent, InstanceData))
                    continue;

                foreach (var handler in handlers)
                    InstanceData.Data = handler((IEvent) @event.ReceivedEvent, InstanceData);
            }
        }

        private void Apply(ProcessStarted @event)
        {
            State = InstanceStates.Started;
            InstanceData.CorrelationId = @event.CorrelationId;
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
            return _definition.GetType().FullName + ":" + InstanceData.Data.GetType().FullName + ":" + identity;
        }

        public string GetStream()
        {
            return GetStreamFor(InstanceData.CorrelationId);
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