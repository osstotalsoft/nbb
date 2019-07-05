using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.Data.Abstractions;
using NBB.Data.EventSourcing.Infrastructure;
using NBB.Domain.Abstractions;
using NBB.EventStore.Abstractions;

namespace NBB.Data.EventSourcing
{
    public class EventSourcedRepository<TAggregateRoot> : IEventSourcedRepository<TAggregateRoot>
        where TAggregateRoot : class, IEventSourcedAggregateRoot, new()  //shortcut you can do as you see fit with new()
    {
        private readonly IEventStore _eventStore;
        private readonly ISnapshotStore _snapshotStore;
        private readonly IMediator _mediator;
        private readonly EventSourcingOptions _eventSourcingOptions;
        private readonly ILogger<EventSourcedRepository<TAggregateRoot>> _logger;

        public EventSourcedRepository(IEventStore eventStore, ISnapshotStore snapshotStore, IMediator mediator, EventSourcingOptions eventSourcingOptions, ILogger<EventSourcedRepository<TAggregateRoot>> logger)
        {
            _eventStore = eventStore;
            _snapshotStore = snapshotStore;
            _mediator = mediator;
            _eventSourcingOptions = eventSourcingOptions;
            _logger = logger;
        }

        public async Task SaveAsync(TAggregateRoot aggregate, CancellationToken cancellationToken = default(CancellationToken))
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var events = aggregate.GetUncommittedChanges().ToList();
            var streamId = aggregate.GetStream();
            var aggregateLoadedAtVersion = aggregate.Version;
            aggregate.MarkChangesAsCommitted();

            await _eventStore.AppendEventsToStreamAsync(streamId, events, aggregateLoadedAtVersion, cancellationToken);

            if (aggregate is ISnapshotableEntity snapshotableAggregate)
            {
                var snapshotVersionFrequency =
                    snapshotableAggregate.SnapshotVersionFrequency ?? 
                    _eventSourcingOptions?.DefaultSnapshotVersionFrequency ??
                    new EventSourcingOptions().DefaultSnapshotVersionFrequency;

                if (aggregate.Version - snapshotableAggregate.SnapshotVersion >= snapshotVersionFrequency)
                {
                    var (snapshot, snapshotVersion) = snapshotableAggregate.TakeSnapshot();
                    await _snapshotStore.StoreSnapshotAsync(new SnapshotEnvelope(snapshot, snapshotVersion, streamId), cancellationToken);
                }
            }

            await PublishEventsAsync(events, cancellationToken);

            stopWatch.Stop();
            _logger.LogDebug("EventSourcedRepository.SaveAsync for {AggregateType} took {ElapsedMilliseconds} ms.", typeof(TAggregateRoot).Name, stopWatch.ElapsedMilliseconds);
        }

        public async Task<TAggregateRoot> GetByIdAsync(object id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var aggregateLoaded = false;
            var aggregate = new TAggregateRoot();//lots of ways to do this
            var streamId = aggregate.GetStreamFor(id);
            if (aggregate is ISnapshotableEntity snapshotableAggregate)
            {
                var snapshotEnvelope = await _snapshotStore.LoadSnapshotAsync(streamId, cancellationToken);
                if (snapshotEnvelope != null)
                {
                    snapshotableAggregate.ApplySnapshot(snapshotEnvelope.Snapshot, snapshotEnvelope.AggregateVersion);
                    aggregateLoaded = true;
                }
            }

            var e = await _eventStore.GetEventsFromStreamAsync(streamId, aggregate.Version + 1, cancellationToken);
            if (e.Any())
            {
                var events = e.Cast<IDomainEvent>();          
                aggregate.LoadFromHistory(events);
                aggregateLoaded = true;
            }
         

            stopWatch.Stop();
            _logger.LogDebug("EventSourcedRepository.GetByIdAsync for {AggregateRootType} took {ElapsedMilliseconds} ms.", typeof(TAggregateRoot).Name, stopWatch.ElapsedMilliseconds);
            return aggregateLoaded ? aggregate : null;
        }


        private async Task PublishEventsAsync(List<IEvent> events, CancellationToken cancellationToken = default(CancellationToken))
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var @event in events.OfType<INotification>())
            {
                await _mediator.Publish(@event, cancellationToken);
            }

            stopWatch.Stop();
            _logger.LogDebug("EventSourcedRepository.PublishEventsAsync for {AggregateType} took {ElapsedMilliseconds} ms.", typeof(TAggregateRoot).Name, stopWatch.ElapsedMilliseconds);
        }

    }
}
