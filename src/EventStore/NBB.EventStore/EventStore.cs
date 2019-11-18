using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.EventStore.Abstractions;
using NBB.EventStore.Internal;
using CorrelationManager = NBB.Correlation.CorrelationManager;

namespace NBB.EventStore
{
    public class EventStore : IEventStore
    {
        private readonly IEventRepository _eventRepository;
        private readonly IEventStoreSerDes _eventStoreSerDes;
        private readonly ILogger<EventStore> _logger;

        public EventStore(IEventRepository eventRepository, IEventStoreSerDes eventStoreSerDes, ILogger<EventStore> logger)
        {
            _eventRepository = eventRepository;
            _eventStoreSerDes = eventStoreSerDes;
            _logger = logger;
        }


        public async Task AppendEventsToStreamAsync(string stream, IEnumerable<IEvent> events, int? expectedVersion,
            CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();


            var eventDescriptors = SerializeEvents(stream, events);
            await _eventRepository.AppendEventsToStreamAsync(stream, eventDescriptors, expectedVersion, cancellationToken);

            stopWatch.Stop();
            _logger.LogDebug("EventStore.AppendEventsToStreamAsync for {Stream} took {ElapsedMilliseconds} ms", stream, stopWatch.ElapsedMilliseconds);
        }

        // collect all processed events for given aggregate and return them as a list
        // used to build up an aggregate from its history (Domain.LoadsFromHistory)
        public async Task<List<IEvent>> GetEventsFromStreamAsync(string stream, int? startFromVersion, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var eventDescriptors = await _eventRepository.GetEventsFromStreamAsync(stream, startFromVersion, cancellationToken);
            var evDescriptors = eventDescriptors.ToList();
            var results = DeserializeEvents(stream, evDescriptors);

            stopWatch.Stop();
            _logger.LogDebug("EventStore.GetEventsFromStreamAsync for {Stream} took {ElapsedMilliseconds} ms", stream, stopWatch.ElapsedMilliseconds);

            return results;
        }

        private static string GetFullTypeName(Type type)
        {
            var result = type.FullName + ", " + type.Assembly.GetName().Name;
            return result;
        }

        private List<IEvent> DeserializeEvents(string stream, List<EventDescriptor> eventDescriptors)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var results = eventDescriptors.Select(desc => _eventStoreSerDes.Deserialize(desc.EventData, Type.GetType(desc.EventType)) as IEvent).ToList();

            stopWatch.Stop();
            _logger.LogDebug("EventStore.DeserializeEvents for {Stream} took {ElapsedMilliseconds} ms", stream, stopWatch.ElapsedMilliseconds);

            return results;
        }

        private IList<EventDescriptor> SerializeEvents(string streamId, IEnumerable<IEvent> events)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var results = new List<EventDescriptor>();
            foreach (var @event in events)
            {
                var eventDescriptor = new EventDescriptor(@event.EventId, GetFullTypeName(@event.GetType()), _eventStoreSerDes.Serialize(@event), streamId, CorrelationManager.GetCorrelationId());
                results.Add(eventDescriptor);
            }

            stopWatch.Stop();
            _logger.LogDebug("EventStore.SerializeEvents for {Stream} took {ElapsedMilliseconds} ms", streamId, stopWatch.ElapsedMilliseconds);

            return results;
        }
    }
}
