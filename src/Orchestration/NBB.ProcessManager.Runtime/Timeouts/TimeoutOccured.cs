using NBB.Core.Abstractions;
using System;
using MediatR;

namespace NBB.ProcessManager.Runtime.Timeouts
{
    public class TimeoutOccured : IEvent, INotification
    {
        public Guid EventId { get; }
        public string ProcessManagerInstanceId { get; }
        public object Message { get; }

        public TimeoutOccured(string processManagerInstanceId, object message, Guid? eventId = null)
        {
            ProcessManagerInstanceId = processManagerInstanceId;
            Message = message;
            EventId = eventId ?? Guid.NewGuid();
        }
    }
}