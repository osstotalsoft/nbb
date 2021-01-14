using System;
using MediatR;
using NBB.Core.Abstractions;
using NBB.Domain.Abstractions;

namespace NBB.Domain
{
    public abstract class DomainEvent : IDomainEvent, INotification
    {
        public DomainEventMetadata Metadata { get; }

        protected DomainEvent(DomainEventMetadata metadata = null)
        {
            Metadata = metadata ?? DomainEventMetadata.Default();
        }

        Guid IDomainEvent.EventId => Metadata.EventId;
    }
}
