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

#if NETCOREAPP3_0_OR_GREATER
        Task<IDisposable> SubscribeAsync(Func<MessagingEnvelope, Task> handler,
            MessagingSubscriberOptions options = null, CancellationToken cancellationToken = default)
            => SubscribeAsync<object>(handler, options, cancellationToken);
#endif
    }
}
