using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.InProcessMessaging.Internal
{
    public interface IStorage
    {
        void Enqueue(string msg, string topic);
        Task AddSubscription(string topic, Func<string, Task> handler, CancellationToken cancellationToken = default);
    }
}
