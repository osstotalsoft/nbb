using NBB.Core.Abstractions;
using System;

namespace NBB.Domain
{
    public class DomainEventMetadata {
        public DateTime CreationDate { get; set; }
        public int SequenceNumber { get; set; }
    }
}