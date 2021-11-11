using NBB.Messaging.DataContracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public interface IMessageBusSubscriber<TMessage>
    {
        Task SubscribeAsync(Func<MessagingEnvelope<TMessage>, Task> handler, CancellationToken cancellationToken = default, string topicName = null, MessagingSubscriberOptions options = null);
        Task UnSubscribeAsync(Func<MessagingEnvelope<TMessage>, Task> handler, CancellationToken cancellationToken = default);
    }


    public static class MessageBusSubscriberExtensions
    {
        public static async Task<MessagingEnvelope<TMessage>> WaitForMessage<TMessage>(this IMessageBusSubscriber<TMessage> subscriber,
            Func<MessagingEnvelope<TMessage>, bool> predicate,
            CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<MessagingEnvelope<TMessage>>();

            async Task HandleMessage(MessagingEnvelope<TMessage> msg)
            {
                if (predicate(msg))
                {
                    tcs.SetResult(msg);
                    await subscriber.UnSubscribeAsync(HandleMessage, cancellationToken);
                }
            }

            await subscriber.SubscribeAsync(HandleMessage, cancellationToken);

            return await tcs.Task;
        }
    }
}
