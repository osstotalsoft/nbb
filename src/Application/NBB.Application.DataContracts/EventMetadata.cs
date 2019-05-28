using System;

namespace NBB.Application.DataContracts
{
    public class EventMetadata
    {
        public Guid EventId { get; }
        public DateTime CreationDate { get; }

        public EventMetadata(Guid eventId, DateTime creationDate)
        {
            EventId = eventId;
            CreationDate = creationDate;
        }

        public static EventMetadata Default() => new EventMetadata(Guid.NewGuid(), DateTime.UtcNow);
    }
}
