using MediatR;
using System;
using System.Collections.Generic;

namespace NBB.Core.Abstractions
{
    public interface IEvent : INotification
    {
        Guid EventId { get; }
    }
}
