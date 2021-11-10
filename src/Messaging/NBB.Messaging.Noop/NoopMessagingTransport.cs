// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Noop
{
    public class NoopMessagingTransport : IMessagingTransport
    {
        public Task<IDisposable> SubscribeAsync(string topic, Func<TransportReceiveContext, Task> handler,
            SubscriptionTransportOptions options = null,
            CancellationToken cancellationToken = default)
        {
            IDisposable subscription = new NoopDisposable();
            return Task.FromResult(subscription);
        }

        public Task PublishAsync(string topic, TransportSendContext sendContext, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
