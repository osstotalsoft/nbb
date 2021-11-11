using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Noop.Internal
{
    public class NoopMessagingTopicSubscriber : IMessagingTopicSubscriber
    {
        public Task SubscribeAsync(string topic, Func<string, Task> handler, CancellationToken cancellationToken = default, MessagingSubscriberOptions options = null)
        {
            return Task.CompletedTask;
        }

        public Task UnSubscribeAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
