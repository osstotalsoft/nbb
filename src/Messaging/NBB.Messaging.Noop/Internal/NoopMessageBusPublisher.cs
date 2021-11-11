using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Noop.Internal
{
    public class NoopMessageBusPublisher : IMessageBusPublisher
    {
        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default, Action<MessagingEnvelope> envelopeCustomizer = null, string topicName = null)
        {
            return Task.CompletedTask;
        }
    }
}
