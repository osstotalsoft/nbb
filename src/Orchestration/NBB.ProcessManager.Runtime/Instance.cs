// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.ProcessManager.Definition;
using NBB.ProcessManager.Runtime.Events;
using System;
using System.Collections.Generic;
using NBB.Core.Effects;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using Newtonsoft.Json;

namespace NBB.ProcessManager.Runtime
{
    public class Instance<TData>
        where TData : IEquatable<TData>, new()
    {
        private readonly IDefinition<TData> _definition;
        private readonly ILogger<Instance<TData>> _logger;

        public TData Data { get; private set; }
        public InstanceStates State { get; private set; }
        public object InstanceId { get; private set; }

        private readonly List<object> _changes = new();
        private Effect<Unit> _effect = Effect.Pure();
        public int Version { get; internal set; }

        public IEnumerable<object> GetUncommittedChanges() => _changes;
        public Effect<Unit> GetUncommittedEffect() => _effect;

        public Instance(IDefinition<TData> definition, ILogger<Instance<TData>> logger)
        {
            _definition = definition;
            _logger = logger;
            Data = new TData();
        }

        private void StartProcess<TEvent>(TEvent @event)
        {
            var eventType = typeof(TEvent);

            var starter = _definition.GetStarterPredicate<TEvent>()(@event, GetInstanceData());
            if (!starter)
                throw new Exception($"Definition {_definition.GetType().GetLongPrettyName()} does not accept eventType {eventType.GetLongPrettyName()} as a starter event.");

            var idSelector = _definition.GetCorrelationFilter<TEvent>();
            if (idSelector == null)
                throw new ArgumentNullException($"No correlation defined for eventType {eventType.GetLongPrettyName()} in definition {_definition.GetType().GetLongPrettyName()}");

            if (State != InstanceStates.NotStarted)
                throw new Exception($"Cannot start an instance which is in state {State}");

            var identity = idSelector(@event);
            Emit(new ProcessStarted(identity));
        }

        private bool IsObsoleteProcess() => Attribute.GetCustomAttribute(_definition.GetType(), typeof(ObsoleteAttribute)) != null;

        public void ProcessEvent<TEvent>(TEvent @event)
        {
            var starter = _definition.GetStarterPredicate<TEvent>()(@event, GetInstanceData());

            if (State == InstanceStates.NotStarted && starter)
            {
                if (IsObsoleteProcess())
                {
                    _logger.LogWarning($"Definition {_definition.GetType().GetLongPrettyName()} is obsolete and new process instances cannot be started.");
                    return;
                }

                StartProcess(@event);
            }

            switch (State)
            {
                case InstanceStates.NotStarted:
                    return;
                case InstanceStates.Completed:
                case InstanceStates.Aborted:
                    throw new Exception($"Cannot accept a new event. Instance is {State}");
            }

            _effect = _effect.Then(_definition.GetEffectFunc<TEvent>()(@event, GetInstanceData()));

            Emit(new EventReceived(@event, @event.GetType().GetLongPrettyName()));

            var newData = _definition.GetSetStateFunc<TEvent>()(@event, GetInstanceData());
            if (!newData.Equals(Data))
            {
                Emit(new StateUpdated<TData>(newData));
            }

            if (_definition.GetCompletionPredicate<TEvent>()(@event, GetInstanceData()))
                Emit(new ProcessCompleted());
        }

        private void Apply(EventReceived _)
        {
            ;
        }

        private void Apply(StateUpdated<TData> @event)
        {
            Data = @event.State;
        }

        private void Apply(ProcessStarted @event)
        {
            State = InstanceStates.Started;
            InstanceId = @event.InstanceId;
        }

        private void Apply(ProcessCompleted _)
        {
            State = InstanceStates.Completed;
        }

        private void ApplyChanges(object @event, bool isNew)
        {
            switch (@event)
            {
                case ProcessStarted processStarted:
                    Apply(processStarted);
                    break;
                case EventReceived eventReceived:
                    Apply(eventReceived);
                    break;
                case StateUpdated<TData> stateUpdated:
                    Apply(stateUpdated);
                    break;
                case ProcessCompleted processCompleted:
                    Apply(processCompleted);
                    break;
                default:
                    throw new Exception($"Event of type {@event.GetType().GetLongPrettyName()} could not be processed");
            }

            if (isNew)
            {
                _changes.Add(@event);
            }
            else
                Version++;
        }

        private void Emit(object @event)
        {
            ApplyChanges(@event, true);
        }

        public string GetStreamFor(object identity)
        {
            return $"{_definition.GetType().FullName}:{Data.GetType().FullName}:{JsonConvert.SerializeObject(identity)}";
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

        private InstanceData<TData> GetInstanceData()
        {
            return new InstanceData<TData>(InstanceId, Data);
        }

        public void MarkChangesAsCommitted()
        {
            Version += _changes.Count;
            _changes.Clear();
            _effect = Effect.Pure();
        }
    }
}
