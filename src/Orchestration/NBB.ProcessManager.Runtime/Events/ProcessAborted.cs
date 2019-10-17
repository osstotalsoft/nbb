using NBB.Core.Abstractions;
using System;

namespace NBB.ProcessManager.Runtime.Events
{
    public class ProcessAborted : IEvent
    {
        public Guid EventId { get; }

        public ProcessAborted(Guid? eventId = null)
        {
            EventId = eventId ?? Guid.NewGuid();
        }
    }
}