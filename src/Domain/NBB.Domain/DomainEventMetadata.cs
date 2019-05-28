using System;

namespace NBB.Domain
{
    public class DomainEventMetadata {
        public Guid EventId { get; }
        public DateTime CreationDate { get; }

        public DomainEventMetadata(Guid eventId, DateTime creationDate)
        {
            EventId = eventId;
            CreationDate = creationDate;
        }

        public static DomainEventMetadata Default() => new DomainEventMetadata(Guid.NewGuid(), DateTime.UtcNow);
    }
}