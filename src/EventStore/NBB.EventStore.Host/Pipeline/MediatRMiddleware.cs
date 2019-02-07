using MediatR;
using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore.Host.Pipeline
{
    public class MediatRMiddleware : IPipelineMiddleware<IEvent>
    {
        private readonly IMediator _mediator;

        public MediatRMiddleware(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Invoke(IEvent @event, CancellationToken cancellationToken, Func<Task> next)
        {
            await _mediator.Publish(@event, cancellationToken);

            await next();
        }
    }
}
