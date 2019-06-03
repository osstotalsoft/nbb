using System;
using NBB.Core.Abstractions;

namespace NBB.Application.DataContracts
{
    public abstract class Event : IEvent, IMetadataProvider<EventMetadata>
    {
        public EventMetadata Metadata { get; }

        protected Event(EventMetadata metadata = null)
        {
            Metadata = metadata ?? EventMetadata.Default();
        }

        Guid IEvent.EventId => Metadata.EventId;
    }
}
