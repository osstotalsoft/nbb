// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Messaging.Abstractions;
using System;

namespace NBB.Messaging.Noop
{
    public class NoopTransportMonitor : ITransportMonitor, IDisposable
    {
        private bool disposedValue;
        public event TransportErrorHandler OnError
        {
            add { }
            remove { }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
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
