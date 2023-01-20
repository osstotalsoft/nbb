// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public interface IMessageBus : IMessageBusSubscriber, IMessageBusPublisher
    {
        Task<MessagingEnvelope<TMessage>> WhenMessage<TMessage>(
            Func<MessagingEnvelope<TMessage>, bool> predicate,
            CancellationToken cancellationToken = default);
    }
}
