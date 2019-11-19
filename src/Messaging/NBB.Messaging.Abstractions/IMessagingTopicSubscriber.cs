using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public interface IMessagingTopicSubscriber
    {
        Task SubscribeAsync(string topic, Func<string, Task> handler, CancellationToken cancellationToken = default, MessagingSubscriberOptions options = null);
        Task UnSubscribeAsync(CancellationToken cancellationToken = default);
    }
}
