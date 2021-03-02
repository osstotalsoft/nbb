using MediatR;
using System;

namespace NBB.Application.DataContracts
{
    public abstract class Event : INotification
    {
        public EventMetadata Metadata { get; }

        protected Event(EventMetadata metadata = null)
        {
            Metadata = metadata ?? EventMetadata.Default();
        }

        public Guid EventId => Metadata.EventId;
    }

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
