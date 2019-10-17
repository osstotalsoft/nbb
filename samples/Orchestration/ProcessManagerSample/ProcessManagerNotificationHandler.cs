using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Core.Abstractions;
using NBB.Messaging.Abstractions;
using NBB.ProcessManager.Definition;
using NBB.ProcessManager.Runtime;
using NBB.ProcessManager.Runtime.Timeouts;

namespace ProcessManagerSample
{
    public class ProcessManagerNotificationHandler<TDefinition, TData, TEvent> : INotificationHandler<TEvent>
        where TDefinition : IDefinition<TData> 
        where TData : struct
        where TEvent : INotification, IEvent
    {
        private readonly ProcessExecutionCoordinator _pec;

        public ProcessManagerNotificationHandler(ProcessExecutionCoordinator pec)
        {
            _pec = pec;
        }

        public async Task Handle(TEvent notification, CancellationToken cancellationToken)
        {
            await _pec.Invoke<TDefinition, TData, TEvent>(notification , cancellationToken);
        }
    }

    public class TimeoutOccuredHandler : INotificationHandler<TimeoutOccured>
    {
        private readonly IMessageBusPublisher _busPublisher;

        public TimeoutOccuredHandler(IMessageBusPublisher busPublisher)
        {
            _busPublisher = busPublisher;
        }

        public Task Handle(TimeoutOccured notification, CancellationToken cancellationToken)
        {
            return _busPublisher.PublishAsync(notification.Message, cancellationToken);
        }
    }
}