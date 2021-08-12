// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore.Abstractions
{
    public interface IEventStore
    {
        Task AppendEventsToStreamAsync(string stream, IEnumerable<object> events, int? expectedVersion, CancellationToken cancellationToken = default);
        Task<List<object>> GetEventsFromStreamAsync(string stream, int? startFromVersion, CancellationToken cancellationToken = default);
    }
}
