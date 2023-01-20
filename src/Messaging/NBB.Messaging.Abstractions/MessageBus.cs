// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public class MessageBus : IMessageBus
    {
        private readonly IMessageBusSubscriber _busSubscriber;
        private readonly IMessageBusPublisher _busPublisher;

        public MessageBus(IMessageBusSubscriber busSubscriber, IMessageBusPublisher busPublisher)
        {
            _busSubscriber = busSubscriber;
            _busPublisher = busPublisher;
        }

        public Task<IDisposable> SubscribeAsync<TMessage>(Func<MessagingEnvelope<TMessage>, Task> handler,
            MessagingSubscriberOptions options = null,
            CancellationToken cancellationToken = default)
            => _busSubscriber.SubscribeAsync(handler, options, cancellationToken);

        public Task PublishAsync<T>(T message, MessagingPublisherOptions options = null,
            CancellationToken cancellationToken = default)
            => _busPublisher.PublishAsync(message, options, cancellationToken);

        public async Task<MessagingEnvelope<TMessage>> WhenMessage<TMessage>(
            Func<MessagingEnvelope<TMessage>, bool> predicate,
            CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<MessagingEnvelope<TMessage>>();
            var tokenS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            Task HandleMessage(MessagingEnvelope<TMessage> msg)
            {
                if (predicate(msg))
                {
                    tcs.SetResult(msg);

                    //cancel ack task continuation
                    tokenS.Cancel();
                }

                return Task.CompletedTask;
            }

            using var subscription = await SubscribeAsync<TMessage>(HandleMessage,
                new MessagingSubscriberOptions {Transport = SubscriptionTransportOptions.RequestReply},
                tokenS.Token);

            return await tcs.Task;
        }
    }
}
