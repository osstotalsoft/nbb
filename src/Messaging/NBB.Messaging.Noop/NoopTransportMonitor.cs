// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Messaging.Abstractions;
using System;

namespace NBB.Messaging.Noop
{
    public class NoopTransportMonitor : ITransportMonitor, IDisposable
    {
        private bool disposedValue;

        public event TransportErrorHandler OnError ;
        public NoopTransportMonitor()
        {
            OnError += NoopTransportMonitor_OnError;
        }

        private void NoopTransportMonitor_OnError(Exception ex)
        {
            // Method intentionally left empty.
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    OnError -= NoopTransportMonitor_OnError;
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
