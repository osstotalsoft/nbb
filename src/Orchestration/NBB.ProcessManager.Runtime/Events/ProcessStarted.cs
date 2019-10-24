using NBB.Core.Abstractions;
using System;

namespace NBB.ProcessManager.Runtime.Events
{
    public class ProcessStarted: IEvent
    {
        public object InstanceId { get; }
        public Guid EventId { get; }

        public ProcessStarted(object instanceId, Guid? eventId = null)
        {
            InstanceId = instanceId;
            EventId = eventId ?? Guid.NewGuid();
        }
    }
}