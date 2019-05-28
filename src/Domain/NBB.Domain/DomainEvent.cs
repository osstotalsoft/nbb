using System;
using NBB.Core.Abstractions;
using NBB.Domain.Abstractions;

namespace NBB.Domain
{
    public abstract class DomainEvent : IDomainEvent, IMetadataProvider<DomainEventMetadata>
    {
        public DomainEventMetadata Metadata { get; }

        protected DomainEvent(DomainEventMetadata metadata)
        {
            Metadata = metadata ?? DomainEventMetadata.Default();
        }

        Guid IEvent.EventId => Metadata.EventId;
    }
}
