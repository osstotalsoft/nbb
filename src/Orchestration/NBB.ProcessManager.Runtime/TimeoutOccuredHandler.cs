// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Mediator;
using NBB.Messaging.Abstractions;
using NBB.ProcessManager.Runtime.Timeouts;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.ProcessManager.Runtime
{
    public class TimeoutOccuredHandler : INotificationHandler<TimeoutOccured>
    {
        private readonly IMessageBusPublisher _busPublisher;

        public TimeoutOccuredHandler(IMessageBusPublisher busPublisher)
        {
            _busPublisher = busPublisher;
        }

        public ValueTask Handle(TimeoutOccured notification, CancellationToken cancellationToken)
            => new(_busPublisher.PublishAsync(notification.Message, cancellationToken));
    }
}