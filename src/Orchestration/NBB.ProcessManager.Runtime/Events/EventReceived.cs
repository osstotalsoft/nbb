using NBB.Core.Abstractions;
using System;

namespace NBB.ProcessManager.Runtime.Events
{
    public class EventReceived<TEvent> : IEvent
    {
        public TEvent ReceivedEvent { get; }
        public Guid EventId { get; }

        public EventReceived(TEvent receivedEvent, Guid? eventId = null)
        {
            ReceivedEvent = receivedEvent;
            EventId = eventId ?? Guid.NewGuid();
        }
    }
}