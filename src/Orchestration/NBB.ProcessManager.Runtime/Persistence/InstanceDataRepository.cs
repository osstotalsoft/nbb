using NBB.Core.Abstractions;
using NBB.EventStore.Abstractions;
using NBB.ProcessManager.Definition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NBB.ProcessManager.Definition.Effects;

namespace NBB.ProcessManager.Runtime.Persistence
{
    public class InstanceDataRepository : IInstanceDataRepository
    {
        private readonly IEventStore _eventStore;
        private readonly IEffectRunner _effectRunner;

        public InstanceDataRepository(IEventStore eventStore, IEffectRunner effectRunner)
        {
            _eventStore = eventStore;
            _effectRunner = effectRunner;
        }

        public async Task Save<TData>(Instance<TData> instance, CancellationToken cancellationToken = default)
            where TData : struct
        {
            var events = instance.GetUncommittedChanges().Cast<IEvent>().ToList();
            var effects = instance.GetUncommittedEffects().ToList();
            var streamId = instance.GetStream();
            var aggregateLoadedAtVersion = instance.Version;
            instance.MarkChangesAsCommitted();

            await _eventStore.AppendEventsToStreamAsync(streamId, events, aggregateLoadedAtVersion, cancellationToken);
            foreach (var effect in effects)
                await effect.Computation(_effectRunner);
        }

        public async Task<Instance<TData>> Get<TData>(IDefinition<TData> definition, object identity, CancellationToken cancellationToken = default)
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