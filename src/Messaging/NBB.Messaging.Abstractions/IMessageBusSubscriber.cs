using System;
using System.Threading;
using System.Threading.Tasks;
using NBB.Messaging.DataContracts;

namespace NBB.Messaging.Abstractions
{
    public interface IMessageBusSubscriber
    {
        Task<IDisposable> SubscribeAsync(Type messageType, Func<MessagingEnvelope, Task> handler, CancellationToken token, string topicName = null,
            MessagingSubscriberOptions options = null);
    }


    public static class MessageBusSubscriberExtensions
    {
        public static async Task<MessagingEnvelope<TMessage>> WaitForMessage<TMessage>(this IMessageBusSubscriber subscriber, Func<MessagingEnvelope<TMessage>, bool> predicate,
            CancellationToken token)
        {
            var tcs = new TaskCompletionSource<MessagingEnvelope<TMessage>>();
            IDisposable subs = null;

            Task HandleMessage(MessagingEnvelope<TMessage> msg)
            {
                if (predicate(msg))
                {
                    tcs.SetResult(msg);
                    subs.Dispose();
                }

                return Task.CompletedTask;
            }

            subs = await subscriber.SubscribeAsync<TMessage>(HandleMessage, token);
            return await tcs.Task;
        }


        public static Task<IDisposable> SubscribeAsync<TMessage>(this IMessageBusSubscriber subscriber, Func<MessagingEnvelope<TMessage>, Task> handler, CancellationToken token,
            string topicName = null, MessagingSubscriberOptions options = null)
        {
            return subscriber.SubscribeAsync(typeof(TMessage), envelope => handler((MessagingEnvelope<TMessage>) envelope), token, topicName, options);
        }
    }
}