// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;

namespace NBB.EventStore.InMemory
{
    public class InMemoryRepository : IEventRepository
    {
        private readonly ConcurrentDictionary<string, EventDescriptorStream> _storage =
            new();

        public Task<IList<EventDescriptor>> GetEventsFromStreamAsync(string stream, int? startFromVersion, CancellationToken cancellationToken = default)
        {
            IList<EventDescriptor> list = _storage.GetValueOrDefault(stream, new EventDescriptorStream(startFromVersion ?? 0, ImmutableList<EventDescriptor>.Empty)).EventDescriptors;
            return Task.FromResult(list);
        }

        public Task AppendEventsToStreamAsync(string stream, IList<EventDescriptor> eventDescriptors,
            int? expectedVersion,
            CancellationToken cancellationToken = default)
        {
            _storage.AddOrUpdate(stream,
                key =>
                {
                    CheckVersion(expectedVersion, 0);

                    return new EventDescriptorStream(0, eventDescriptors.ToImmutableList());
                },
                (key, value) =>
                {
                    CheckVersion(expectedVersion, value.Version);

                    return value with
                    {
                        LoadedAtVersion = value.Version,
                        EventDescriptors = value.EventDescriptors.AddRange(eventDescriptors)
                    };
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

        private record EventDescriptorStream(int LoadedAtVersion, ImmutableList<EventDescriptor> EventDescriptors)
        {
            public int Version => EventDescriptors.Count + LoadedAtVersion;
        }
    }
}
