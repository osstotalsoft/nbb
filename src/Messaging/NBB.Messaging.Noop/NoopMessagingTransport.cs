// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Noop
{
    public class NoopMessagingTransport : IMessagingTransport, ITransportMonitor, IDisposable
    {
        private bool disposedValue;
#pragma warning disable 414
        public event TransportErrorHandler OnError;
#pragma warning restore 414
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    OnError = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
