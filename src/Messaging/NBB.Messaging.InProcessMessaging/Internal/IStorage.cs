using System;
using System.Threading;
using System.Threading.Tasks;
using NBB.Messaging.DataContracts;

namespace NBB.Messaging.InProcessMessaging.Internal
{
    public interface IStorage
    {
        void Enqueue(string msg, string topic);
        Task AddSubscription(string topic, Func<string, Task> handler, CancellationToken cancellationToken = default);
    }
}
