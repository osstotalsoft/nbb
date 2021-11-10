// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public interface IMessageBusPublisher
    {
        Task PublishAsync<T>(T message, MessagingPublisherOptions publisherOptions = null,
            CancellationToken cancellationToken = default);

        Task PublishAsync<T>(T message, CancellationToken cancellationToken) =>
            PublishAsync(message, null, cancellationToken);
    }
}
