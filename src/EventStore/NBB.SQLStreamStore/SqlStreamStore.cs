using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.EventStore.Abstractions;
using NBB.SQLStreamStore.Internal;
using SqlStreamStore;
using SqlStreamStore.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.SQLStreamStore
{
    public class SqlStreamStore : IEventStore
    {
        private readonly IStreamStore _streamStore;
        private readonly ISerDes _serDes;
        private readonly ILogger<SqlStreamStore> _logger;

        public SqlStreamStore(IStreamStore streamStore, ISerDes serDes, ILogger<SqlStreamStore> logger)
        {
            _streamStore = streamStore;
            _serDes = serDes;
            _logger = logger;
        }

        public async Task AppendEventsToStreamAsync(string stream, IEnumerable<IEvent> events, int? expectedVersion, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var adaptedEvents = new List<NewStreamMessage>();
            foreach (var e in events)
            {
                var metadata = _serDes.Serialize(new EventMetadata(e.GetType(), Correlation.CorrelationManager.GetCorrelationId()));
                var data = _serDes.Serialize(e);
                var ae = new NewStreamMessage(e.EventId, GetFullTypeName(e.GetType()), data, metadata);
                adaptedEvents.Add(ae);
            }

            await _streamStore.AppendToStream(stream, expectedVersion ?? ExpectedVersion.Any, adaptedEvents.ToArray(), cancellationToken);

            stopWatch.Stop();
            _logger.LogDebug("SqlStreamStore.AppendEventsToStreamAsync for {Stream} took {ElapsedMilliseconds} ms", stream, stopWatch.ElapsedMilliseconds);
        }

        public async Task<List<IEvent>> GetEventsFromStreamAsync(string stream, int? startFromVersion, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            const int _PAGE_SIZE = 10;

            var page = await _streamStore.ReadAllForwards(Position.Start, _PAGE_SIZE, cancellationToken: cancellationToken);
            var messages = new List<StreamMessage>(page.Messages);
            while (!page.IsEnd) //should not take more than 20 iterations.
            {
                page = await page.ReadNext(cancellationToken);
                messages.AddRange(page.Messages);
            }

            var result = new List<IEvent>();
            foreach (var sm in messages)
            {
                var metadata = _serDes.Deserialize<EventMetadata>(sm.JsonMetadata);
                var eventType = metadata.GetEventType();
                var data = await sm.GetJsonData(cancellationToken);
                var @event = _serDes.Deserialize(data, eventType) as IEvent;
                result.Add(@event);
            }

            stopWatch.Stop();
            _logger.LogDebug("SqlStreamStore.GetEventsFromStreamAsync for {Stream} took {ElapsedMilliseconds} ms", stream, stopWatch.ElapsedMilliseconds);

            return result;
        }


        private static string GetFullTypeName(Type type)
        {
            var result = type.FullName + ", " + type.Assembly.GetName().Name;
            return result;
        }
    }
}
