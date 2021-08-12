// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using System;

namespace NBB.Application.DataContracts
{
    [Obsolete("Ensures NBB4 compatibility. Use INotification instead.")]
    public abstract class Event : INotification
    {
        public EventMetadata Metadata { get; }

        protected Event(EventMetadata metadata = null)
        {
            Metadata = metadata ?? EventMetadata.Default();
        }

        public Guid EventId => Metadata.EventId;
    }

    [Obsolete("Ensures NBB4 compatibility")]
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
