// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.Messaging.Noop
{
    public class NoopDisposable : IDisposable
    {
        private bool disposedValue;
        private object _state = new();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _state = null;
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
