using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Noop.Internal
{
    public class NoopMessageBusSubscriber<TMessage> : IMessageBusSubscriber<TMessage>
    {
        public Task SubscribeAsync(Func<MessagingEnvelope<TMessage>, Task> handler, CancellationToken cancellationToken = default, string topicName = null, MessagingSubscriberOptions options = null)
        {
            return Task.CompletedTask;
        }

        public Task UnSubscribeAsync(Func<MessagingEnvelope<TMessage>, Task> handler, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
