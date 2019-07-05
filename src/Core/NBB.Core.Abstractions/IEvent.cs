using System;

namespace NBB.Core.Abstractions
{
    public interface IEvent
    {
        Guid EventId { get; }
    }
}
