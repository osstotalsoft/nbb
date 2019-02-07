using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;

namespace NBB.EventStore.InMemory
{
    public class InMemoryRepository : IEventRepository
    {
        private readonly ConcurrentDictionary<string, EventDescriptorCollection> _storage =
            new ConcurrentDictionary<string, EventDescriptorCollection>();

        public Task<IList<EventDescriptor>> GetEventsFromStreamAsync(string stream, int? startFromVersion, CancellationToken cancellationToken)
        {
            IList<EventDescriptor> list = _storage.GetValueOrDefault(stream, new EventDescriptorCollection(startFromVersion ?? 0));
            return Task.FromResult(list);
        }

        public Task AppendEventsToStreamAsync(string stream, IList<EventDescriptor> eventDescriptors,
            int? expectedVersion,
            CancellationToken cancellationToken)
        {
            _storage.AddOrUpdate(stream,
                key =>
                {
                    CheckVersion(expectedVersion, 0);

                    return new EventDescriptorCollection(0, eventDescriptors);
                },
                (key, value) =>
                {
                    CheckVersion(expectedVersion, value.Version);

                    value.AddRange(eventDescriptors);

                    return value;
                });

            return Task.CompletedTask;
        }

        private static void CheckVersion(int? expectedVersion, int actualVersion)
        {
            if (!expectedVersion.HasValue || actualVersion == expectedVersion) return;

            if (expectedVersion == 0)
            {
                throw new ConcurrencyUnrecoverableException("EventStore unrecoverable concurrency exception");
            }

            throw new ConcurrencyException("EventStore concurrency exception");
        }

        private class EventDescriptorCollection : List<EventDescriptor>
        {
            private readonly int _loadedAtVersion;

            public EventDescriptorCollection(int loadedAtVersion)
            {
                _loadedAtVersion = loadedAtVersion;
            }

            public EventDescriptorCollection(int loadedAtVersion, IEnumerable<EventDescriptor> collection)
                :base(collection)
            {
                _loadedAtVersion = loadedAtVersion;
            }

            public int Version => Count + _loadedAtVersion;
        }
    }
}