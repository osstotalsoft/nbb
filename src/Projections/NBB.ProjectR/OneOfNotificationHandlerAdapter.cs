using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace NBB.ProjectR
{
    class OneOfNotificationHandlerAdapter<TSumType, TEvent> : INotificationHandler<TEvent> 
        where TEvent : INotification
    {
        private readonly IMediator _mediator;

        public OneOfNotificationHandlerAdapter(IMediator mediator)
        {
            _mediator = mediator;
        }
        public Task Handle(TEvent notification, CancellationToken cancellationToken)
        {
            var wrappedEv =  (TSumType)typeof(TSumType).GetConstructors().First().Invoke(new object[] {notification});
            return _mediator.Publish(wrappedEv, cancellationToken);
        }
    }
}
