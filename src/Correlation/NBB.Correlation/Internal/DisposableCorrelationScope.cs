// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.Correlation.Internal
{
    public class DisposableCorrelationScope : IDisposable
    {
        public Guid CorrelationId { get; }

        internal DisposableCorrelationScope(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public void Dispose()
        {
            CorrelationManager.ClearCorrelationId();
        }

        public static implicit operator Guid(DisposableCorrelationScope disposableCorrelationScope)
        {
            return disposableCorrelationScope.CorrelationId;
        }
    }
}
