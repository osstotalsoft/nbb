using MediatR;
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

        public Task Handle(TimeoutOccured notification, CancellationToken cancellationToken)
            => _busPublisher.PublishAsync(notification.Message, cancellationToken);
    }
}