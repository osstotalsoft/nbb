using System;
using System.Threading;
using System.Threading.Tasks;
using NBB.Messaging.DataContracts;

namespace NBB.Messaging.Abstractions
{
    public interface IMessageBusPublisher
    {
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default(CancellationToken), Action<MessagingEnvelope> envelopeCustomizer = null, string topicName = null);
    }
}
