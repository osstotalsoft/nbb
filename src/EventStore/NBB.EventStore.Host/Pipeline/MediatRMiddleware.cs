using MediatR;
using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore.Host.Pipeline
{
    public class MediatRMiddleware : IPipelineMiddleware<object>
    {
        private readonly IMediator _mediator;

        public MediatRMiddleware(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Invoke(object @event, CancellationToken cancellationToken, Func<Task> next)
        {
            if (@event is INotification notification)
            {
                await _mediator.Publish(notification, cancellationToken);
            }

            await next();
        }
    }
}
