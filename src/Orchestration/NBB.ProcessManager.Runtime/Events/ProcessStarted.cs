using NBB.Core.Abstractions;
using System;

namespace NBB.ProcessManager.Runtime.Events
{
    public class ProcessStarted: IEvent
    {
        public object CorrelationId { get; }
        public Guid EventId { get; }

        public ProcessStarted(object correlationId, Guid? eventId = null)
        {
            CorrelationId = correlationId;
            EventId = eventId ?? Guid.NewGuid();
        }
    }
}