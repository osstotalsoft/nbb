// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Correlation.Internal;
using System;
using System.Threading;

namespace NBB.Correlation
{
    public static class CorrelationManager
    {
        private static readonly AsyncLocal<Guid?> CorrelationId = new();

        public static IDisposable NewCorrelationId(Guid? correlationId = null)
        {
            if(CorrelationId.Value.HasValue)
                throw new Exception("CorrelationId has already been set");

            var uuid = correlationId.HasValue && correlationId.Value!= default ? correlationId.Value : Guid.NewGuid();
            CorrelationId.Value = uuid;
            return new DisposableCorrelationScope(uuid);
        }

        public static Guid? GetCorrelationId()
        {

            return CorrelationId.Value;
        }

        internal static void ClearCorrelationId()
        {
            if (!CorrelationId.Value.HasValue)
                throw new Exception("CorrelationId has never been set");

            CorrelationId.Value = null;
        }
    }
}
