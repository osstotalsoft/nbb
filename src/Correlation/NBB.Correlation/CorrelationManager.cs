using System;
using System.Threading;
using NBB.Correlation.Internal;

namespace NBB.Correlation
{
    public static class CorrelationManager
    {
        private static readonly AsyncLocal<Guid?> _correlationId = new AsyncLocal<Guid?>();

        public static IDisposable NewCorrelationId(Guid? correlationId = null)
        {
            if(_correlationId.Value.HasValue)
                throw new Exception("CorrelationId has already been set");

            var uuid = correlationId.HasValue && correlationId.Value!= default(Guid) ? correlationId.Value : Guid.NewGuid();
            _correlationId.Value = uuid;
            return new DisposableCorrelationScope(uuid);
        }

        public static Guid? GetCorrelationId()
        {

            return _correlationId.Value;
        }

        internal static void ClearCorrelationId()
        {
            if (!_correlationId.Value.HasValue)
                throw new Exception("CorrelationId has never been set");

            _correlationId.Value = null;
        }
    }
}
