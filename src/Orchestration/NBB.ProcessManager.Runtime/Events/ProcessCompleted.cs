using NBB.Core.Abstractions;
using System;

namespace NBB.ProcessManager.Runtime.Events
{
    public class ProcessCompleted<TEvent> : IEvent
    {
        public TEvent ReceivedEvent { get; }
        public Guid EventId { get; }

        public ProcessCompleted(TEvent receivedEvent, Guid? eventId = null)
        {
            ReceivedEvent = receivedEvent;
            EventId = eventId ?? Guid.NewGuid();
        }
    }
}