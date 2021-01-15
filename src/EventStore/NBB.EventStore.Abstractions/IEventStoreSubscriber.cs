using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore.Abstractions
{
    public interface IEventStoreSubscriber
    {
        Task SubscribeToAllAsync(Func<object, Task> handler, CancellationToken cancellationToken = default);
    }
}
