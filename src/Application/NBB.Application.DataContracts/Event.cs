using NBB.Core.Abstractions;
using NBB.Messaging.DataContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace NBB.Application.DataContracts
{
    public abstract class Event : IEvent, IMetadataProvider<ApplicationMetadata>
    {
        public Guid EventId { get; }
        public ApplicationMetadata Metadata { get; }


        protected Event(Guid eventId, ApplicationMetadata metadata)
        {
            EventId = eventId;
            Metadata = metadata ?? new ApplicationMetadata();
        }
    }
}
