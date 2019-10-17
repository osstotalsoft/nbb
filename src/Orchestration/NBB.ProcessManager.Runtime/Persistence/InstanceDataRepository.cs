using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;
using NBB.EventStore.Abstractions;
using NBB.ProcessManager.Definition;
using NBB.ProcessManager.Runtime.EffectRunners;

namespace NBB.ProcessManager.Runtime.Persistence
{
    public class InstanceDataRepository : IInstanceDataRepository
    {
        private readonly IEventStore _eventStore;
        private readonly EffectRunnerFactory _effectRunnerFactory;

        public InstanceDataRepository(IEventStore eventStore, EffectRunnerFactory effectRunnerFactory)
        {
            _eventStore = eventStore;
            _effectRunnerFactory = effectRunnerFactory;
        }

        public async Task Save<TData>(Instance<TData> instance, CancellationToken cancellationToken)
            where TData : struct
        {
            var events = instance.GetUncommittedChanges().Cast<IEvent>().ToList();
            var effects = instance.GetUncommittedEffects().ToList();
            var streamId = instance.GetStream();
            var aggregateLoadedAtVersion = instance.Version;
            instance.MarkChangesAsCommitted();

            await _eventStore.AppendEventsToStreamAsync(streamId, events, aggregateLoadedAtVersion, cancellationToken);
            foreach (var effect in effects)
                await _effectRunnerFactory(effect.GetType())(effect);
        }

        public async Task<Instance<TData>> Get<TData>(IDefinition<TData> definition, object identity, CancellationToken cancellationToken)
            where TData : struct
        {
            var instance = new Instance<TData>(definition);
            var streamId = instance.GetStreamFor(identity);
            var events = await _eventStore.GetEventsFromStreamAsync(streamId, instance.Version + 1, cancellationToken);
            if (events.Any())
                instance.LoadFromHistory(events);

            return instance;
        }
    }
}