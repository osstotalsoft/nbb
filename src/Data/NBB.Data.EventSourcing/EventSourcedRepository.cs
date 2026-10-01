// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.Data.Abstractions;
using NBB.Data.EventSourcing.Infrastructure;
using NBB.Domain.Abstractions;
using NBB.EventStore.Abstractions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Data.EventSourcing
{
    public class EventSourcedRepository<TAggregateRoot>(
        IEventStore eventStore,
        ISnapshotStore snapshotStore,
        IEventPublisher eventPublisher,
        EventSourcingOptions eventSourcingOptions,
        ILogger<EventSourcedRepository<TAggregateRoot>> logger) : IEventSourcedRepository<TAggregateRoot>
        where TAggregateRoot : class, IEventSourcedAggregateRoot, new()  //shortcut you can do as you see fit with new()
    {
        public async Task SaveAsync(TAggregateRoot aggregate, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var events = aggregate.GetUncommittedChanges().ToList();
            var streamId = aggregate.GetStream();
            var aggregateLoadedAtVersion = aggregate.Version;
            aggregate.MarkChangesAsCommitted();

            await eventStore.AppendEventsToStreamAsync(streamId, events, aggregateLoadedAtVersion, cancellationToken);

            if (aggregate is ISnapshotableEntity snapshotableAggregate)
            {
                var snapshotVersionFrequency =
                    snapshotableAggregate.SnapshotVersionFrequency ?? 
                    eventSourcingOptions?.DefaultSnapshotVersionFrequency ??
                    new EventSourcingOptions().DefaultSnapshotVersionFrequency;

                if (aggregate.Version - snapshotableAggregate.SnapshotVersion >= snapshotVersionFrequency)
                {
                    var (snapshot, snapshotVersion) = snapshotableAggregate.TakeSnapshot();
                    await snapshotStore.StoreSnapshotAsync(new SnapshotEnvelope(snapshot, snapshotVersion, streamId), cancellationToken);
                }
            }

            await PublishEventsAsync(events, cancellationToken);

            stopWatch.Stop();
            logger.LogDebug("EventSourcedRepository.SaveAsync for {AggregateType} took {ElapsedMilliseconds} ms.", typeof(TAggregateRoot).Name, stopWatch.ElapsedMilliseconds);
        }

        public async Task<TAggregateRoot> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var aggregateLoaded = false;
            var aggregate = new TAggregateRoot();//lots of ways to do this
            var streamId = aggregate.GetStreamFor(id);
            if (aggregate is ISnapshotableEntity snapshotableAggregate)
            {
                var snapshotEnvelope = await snapshotStore.LoadSnapshotAsync(streamId, cancellationToken);
                if (snapshotEnvelope != null)
                {
                    snapshotableAggregate.ApplySnapshot(snapshotEnvelope.Snapshot, snapshotEnvelope.AggregateVersion);
                    aggregateLoaded = true;
                }
            }

            var e = await eventStore.GetEventsFromStreamAsync(streamId, aggregate.Version + 1, cancellationToken);
            if (e.Any())
            {
                var events = e;          
                aggregate.LoadFromHistory(events);
                aggregateLoaded = true;
            }
         

            stopWatch.Stop();
            logger.LogDebug("EventSourcedRepository.GetByIdAsync for {AggregateRootType} took {ElapsedMilliseconds} ms.", typeof(TAggregateRoot).Name, stopWatch.ElapsedMilliseconds);
            return aggregateLoaded ? aggregate : null;
        }


        private async Task PublishEventsAsync(List<object> events, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            await eventPublisher.PublishAsync(events, cancellationToken);

            stopWatch.Stop();
            logger.LogDebug("EventSourcedRepository.PublishEventsAsync for {AggregateType} took {ElapsedMilliseconds} ms.", typeof(TAggregateRoot).Name, stopWatch.ElapsedMilliseconds);
        }

    }
}
