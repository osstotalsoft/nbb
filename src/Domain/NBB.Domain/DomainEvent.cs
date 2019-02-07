using NBB.Core.Abstractions;
using NBB.Domain.Abstractions;
using System;
using System.Collections.Generic;

namespace NBB.Domain
{
    public abstract class DomainEvent : IDomainEvent, IMetadataProvider<DomainEventMetadata>
    {
        public Guid EventId { get; }
        public DomainEventMetadata Metadata { get; }
        int IDomainEvent.SequenceNumber
        {
            get => Metadata.SequenceNumber;
            set => Metadata.SequenceNumber = value;
        }

        protected DomainEvent(Guid eventId, DomainEventMetadata metadata)
        {
            EventId = eventId;
            Metadata = metadata ?? new DomainEventMetadata();
        }
    }
}
