using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public interface IMessageBus : IMessageBusSubscriber, IMessageBusPublisher
    {
        Task<MessagingEnvelope<TMessage>> SubscriptionMessage<TMessage>(
            Func<MessagingEnvelope<TMessage>, bool> predicate,
            CancellationToken cancellationToken = default);
    }
}