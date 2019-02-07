using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;

namespace NBB.EventStore.Abstractions
{
    public interface IEventStore
    {
        Task AppendEventsToStreamAsync(string stream, IEnumerable<IEvent> events, int? expectedVersion, CancellationToken cancellationToken = default);
        Task<List<IEvent>> GetEventsFromStreamAsync(string stream, int? startFromVersion, CancellationToken cancellationToken = default);
    }
}
