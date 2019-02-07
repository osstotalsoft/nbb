using System;

namespace NBB.EventStore
{
    public class EventDescriptor
    {
        public Guid EventId { get; }
        public string EventType { get; }
        public string EventData { get; }
        public string StreamId { get; }

        public Guid? CorrelationId { get; }

        public EventDescriptor(Guid eventId, string eventType, string eventData, string streamId, Guid? correlationId)
        {
            EventId = eventId;
            EventType = eventType;
            EventData = eventData;
            StreamId = streamId;
            CorrelationId = correlationId;
        }
    }
}
