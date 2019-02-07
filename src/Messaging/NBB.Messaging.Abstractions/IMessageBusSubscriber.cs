using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NBB.Messaging.DataContracts;

namespace NBB.Messaging.Abstractions
{
    public interface IMessageBusSubscriber<TMessage>
    {
        Task SubscribeAsync(Func<MessagingEnvelope<TMessage>, Task> handler, CancellationToken token, string topicName = null, MessagingSubscriberOptions options = null);
        Task UnSubscribeAsync(Func<MessagingEnvelope<TMessage>, Task> handler, CancellationToken token);
    }


    public static class MessageBusSubscriberExtensions
    {
        public static async Task<MessagingEnvelope<TMessage>> WaitForMessage<TMessage>(this IMessageBusSubscriber<TMessage> subscriber, Func<MessagingEnvelope<TMessage>, bool> predicate, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<MessagingEnvelope<TMessage>>();

            async Task HandleMessage(MessagingEnvelope<TMessage> msg)
            {
                if (predicate(msg))
                {
                    tcs.SetResult(msg);
                    await subscriber.UnSubscribeAsync(HandleMessage, token);
                }
            }

            await subscriber.SubscribeAsync(HandleMessage, token);

            return await tcs.Task;
        }
    }
}
