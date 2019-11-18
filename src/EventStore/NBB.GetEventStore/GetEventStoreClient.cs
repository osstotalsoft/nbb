using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.EventStore.Abstractions;
using NBB.GetEventStore.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CorrelationManager = NBB.Correlation.CorrelationManager;

namespace NBB.GetEventStore
{
    public class GetEventStoreClient : IEventStore
    {
        private readonly ISerDes _serDes;
        private readonly ILogger<GetEventStoreClient> _logger;

        public GetEventStoreClient(ISerDes serDes, ILogger<GetEventStoreClient> logger)
        {
            _serDes = serDes;
            _logger = logger;
        }

        public async Task AppendEventsToStreamAsync(string stream, IEnumerable<IEvent> events, int? expectedVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var gregsEvents = new List<EventData>();
            foreach (var e in events)
            {
                var metadata = _serDes.Serialize(new EventMetadata(e.GetType(), CorrelationManager.GetCorrelationId()));
                var data = _serDes.Serialize(e);
                var ge = new EventData(e.EventId, GetFullTypeName(e.GetType()), true, data, metadata);
                gregsEvents.Add(ge);
            }

            using (var connection = await GetConnectionAsync())
            {
                await connection.AppendToStreamAsync(stream, expectedVersion ?? ExpectedVersion.Any, gregsEvents.ToArray());
            }

            stopWatch.Stop();
            _logger.LogDebug("GetEventStoreClient.AppendEventsToStreamAsync for {Stream} took {ElapsedMilliseconds} ms", stream, stopWatch.ElapsedMilliseconds);
        }

        public async Task<List<IEvent>> GetEventsFromStreamAsync(string stream, int? startFromVersion, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var result = new List<IEvent>();
            var gregsEvents = new List<ResolvedEvent>();

            using (var connection = await GetConnectionAsync())
            {
                StreamEventsSlice currentSlice;
                long nextSliceStart = StreamPosition.Start;
                do
                {
                    currentSlice = await connection.ReadStreamEventsForwardAsync(stream, nextSliceStart, 200, false);

                    nextSliceStart = currentSlice.NextEventNumber;

                    gregsEvents.AddRange(currentSlice.Events);
                } while (!currentSlice.IsEndOfStream);
            }

            foreach (var resolvedEvent in gregsEvents)
            {
                var metadata = _serDes.Deserialize<EventMetadata>(resolvedEvent.Event.Metadata);
                var eventType = metadata.GetEventType();
                var @event = _serDes.Deserialize(resolvedEvent.Event.Data, eventType) as IEvent;
                result.Add(@event);
            }

            stopWatch.Stop();
            _logger.LogDebug("GetEventStoreClient.GetEventsFromStreamAsync for {Stream} took {ElapsedMilliseconds} ms", stream, stopWatch.ElapsedMilliseconds);

            return result;
        }

        private async Task<IEventStoreConnection> GetConnectionAsync()
        {
            var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            await connection.ConnectAsync();

            return connection;

        }

        private static string GetFullTypeName(Type type)
        {
            var result = type.FullName + ", " + type.Assembly.GetName().Name;
            return result;
        }
    }
}
