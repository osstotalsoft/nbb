using NBB.Core.Abstractions;
using System;

namespace NBB.ProcessManager.Runtime.Events
{
    public class ProcessTimeout : IEvent
    {
        public Guid EventId { get; }

        public ProcessTimeout(Guid? eventId = null)
        {
            EventId = eventId ?? Guid.NewGuid();
        }
    }
}