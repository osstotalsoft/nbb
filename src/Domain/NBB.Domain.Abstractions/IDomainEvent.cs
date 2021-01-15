using System;

namespace NBB.Domain.Abstractions
{
    public interface IDomainEvent
    {
        Guid EventId { get; }
    }
}
