using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore
{
    public interface IEventRepository
    {
        Task<IList<EventDescriptor>> GetEventsFromStreamAsync(string stream, int? startFromVersion, CancellationToken cancellationToken);
        Task AppendEventsToStreamAsync(string stream, IList<EventDescriptor> eventDescriptors, int? expectedVersion, CancellationToken cancellationToken);
    }
}