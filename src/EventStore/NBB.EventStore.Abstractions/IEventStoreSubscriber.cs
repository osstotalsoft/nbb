using System;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;

namespace NBB.EventStore.Abstractions
{
    public interface IEventStoreSubscriber
    {
        Task SubscribeToAllAsync(Func<IEvent, Task> handler, CancellationToken token);
    }
}
