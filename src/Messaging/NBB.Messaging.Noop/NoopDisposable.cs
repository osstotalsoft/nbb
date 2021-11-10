// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.Messaging.Noop
{
    public class NoopDisposable : IDisposable
    {
        protected virtual void Dispose(bool disposing)
        {
            
        }
       
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
