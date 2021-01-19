using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public interface IMessageBusPublisher
    {
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default, Action<MessagingEnvelope> envelopeCustomizer = null, string topicName = null);
    }
}
