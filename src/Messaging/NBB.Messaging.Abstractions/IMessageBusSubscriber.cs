using NBB.Messaging.DataContracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public interface IMessageBusSubscriber<TMessage>
    {
        Task SubscribeAsync(Func<MessagingEnvelope, Task> handler, CancellationToken token = default, string topicName = null, MessagingSubscriberOptions options = null);
        Task UnSubscribeAsync(Func<MessagingEnvelope, Task> handler, CancellationToken token = default);
    }


    public static class MessageBusSubscriberExtensions
    {
        public static async Task<MessagingEnvelope> WaitForMessage<TMessage>(this IMessageBusSubscriber<TMessage> subscriber,
            Func<MessagingEnvelope, bool> predicate,
            CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<MessagingEnvelope>();

            async Task HandleMessage(MessagingEnvelope msg)
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
