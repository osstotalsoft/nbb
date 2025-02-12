// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public interface IMessageBusSubscriber
    {
        Task<IDisposable> SubscribeAsync<TMessage>(Func<MessagingEnvelope<TMessage>, Task> handler,
            MessagingSubscriberOptions options = null, CancellationToken cancellationToken = default);


        Task<IDisposable> SubscribeAsync(Func<MessagingEnvelope, Task> handler,
            MessagingSubscriberOptions options = null, CancellationToken cancellationToken = default)
            => SubscribeAsync<object>(handler, options, cancellationToken);
    }
}
