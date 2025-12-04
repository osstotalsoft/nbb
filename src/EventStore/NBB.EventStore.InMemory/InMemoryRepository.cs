// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;

namespace NBB.EventStore.InMemory
{
    public class InMemoryRepository : IEventRepository
    {
        private readonly ConcurrentDictionary<string, ImmutableList<EventDescriptor>> _storage =
            new();

        public Task<IList<EventDescriptor>> GetEventsFromStreamAsync(string stream, int? startFromVersion, CancellationToken cancellationToken = default)
        {
            IList<EventDescriptor> list = _storage.GetValueOrDefault(stream, ImmutableList<EventDescriptor>.Empty);

            if (startFromVersion.HasValue)
            {
                list = list.Skip(startFromVersion.Value - 1).ToList();
            }   

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

                    return eventDescriptors.ToImmutableList();
                },
                (key, value) =>
                {
                    CheckVersion(expectedVersion, value.Count);

                    return value.AddRange(eventDescriptors);                    
                });

            return Task.CompletedTask;
        }

        public Task DeleteStreamAsync(string stream, CancellationToken cancellationToken = default)
        {
            _storage.TryRemove(stream, out _);
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
    }
}
