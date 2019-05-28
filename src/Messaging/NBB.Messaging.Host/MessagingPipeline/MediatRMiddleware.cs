using MediatR;
using NBB.Core.Pipeline;
using NBB.Messaging.DataContracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host.MessagingPipeline
{
    /// <summary>
    /// A pipeline middleware that forwards messages that contain requests or events to mediatR.
    /// </summary>
    /// <seealso cref="IPipelineMiddleware{MessagingEnvelope}" />
    public class MediatRMiddleware : IPipelineMiddleware<MessagingEnvelope>
    {
        private readonly IMediator _mediator;

        public MediatRMiddleware(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Invoke(MessagingEnvelope message, CancellationToken cancellationToken, Func<Task> next)
        {
            if (message.Payload is INotification @event)
            {
                await _mediator.Publish(@event, cancellationToken);
            }
            else if (message.Payload is IRequest request)
            {
                await _mediator.Send(request, cancellationToken);
            }
            else
            {
                throw new ApplicationException($"Message type {message.Payload.GetType()} cannot be handled by mediatR");
            }

            await next();
        }
    }
}
