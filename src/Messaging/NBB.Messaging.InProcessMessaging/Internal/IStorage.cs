using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.InProcessMessaging.Internal
{
    public interface IStorage
    {
        void Enqueue(byte[] msg, string topic);
        Task AddSubscription(string topic, Func<byte[], Task> handler, CancellationToken cancellationToken = default);
    }
}
