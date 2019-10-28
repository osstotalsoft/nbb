using NBB.Core.Abstractions;
using NBB.EventStore.Abstractions;
using NBB.ProcessManager.Definition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.ProcessManager.Runtime.Persistence
{
    public class InstanceDataRepository : IInstanceDataRepository
    {
        private readonly IEventStore _eventStore;
        private readonly IEffectVisitor _effectVisitor;

        public InstanceDataRepository(IEventStore eventStore, IEffectVisitor effectVisitor)
        {
            _eventStore = eventStore;
            _effectVisitor = effectVisitor;
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
                await effect.Accept(_effectVisitor);
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