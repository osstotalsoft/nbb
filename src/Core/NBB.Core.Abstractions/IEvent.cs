using System;
using MediatR;

namespace NBB.Core.Abstractions
{
    public interface IEvent : INotification
    {
        Guid EventId { get; }
    }
}
