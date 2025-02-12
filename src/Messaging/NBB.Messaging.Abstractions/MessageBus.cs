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
            int millisecondsTimeout = 0,
            CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<MessagingEnvelope<TMessage>>();
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            Task HandleMessage(MessagingEnvelope<TMessage> msg)
            {
                if (predicate(msg))
                {
                    tcs.SetResult(msg);

                    //cancel ack task continuation
                    tokenSource.Cancel();
                }

                return Task.CompletedTask;
            }

            using var subscription = await SubscribeAsync<TMessage>(HandleMessage,
                new MessagingSubscriberOptions { Transport = SubscriptionTransportOptions.RequestReply },
                tokenSource.Token);

            if (millisecondsTimeout > 0)
#if NET6_0_OR_GREATER
                return await tcs.Task.WaitAsync(TimeSpan.FromMilliseconds(millisecondsTimeout));
#else
                return await Task.WhenAny(tcs.Task, Task.Delay(millisecondsTimeout)) == tcs.Task ? await tcs.Task : throw new TimeoutException();
#endif

            return await tcs.Task;
        }
    }
}
