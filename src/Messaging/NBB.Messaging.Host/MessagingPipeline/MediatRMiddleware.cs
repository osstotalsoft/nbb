// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using MediatR;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    /// <summary>
    /// A pipeline middleware that forwards messages that contain requests or events to mediatR.
    /// </summary>
    /// <seealso cref="IPipelineMiddleware{MessagingEnvelope}" />
    public class MediatRMiddleware : IPipelineMiddleware<MessagingContext>
    {
        private readonly IMediator _mediator;

        public MediatRMiddleware(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Invoke(MessagingContext context, CancellationToken cancellationToken, Func<Task> next)
        {
            if (context.MessagingEnvelope.Payload is INotification @event)
            {
                await _mediator.Publish(@event, cancellationToken);
            }
            else if (context.MessagingEnvelope.Payload is IRequest request)
            {
                await _mediator.Send(request, cancellationToken);
            }
            else
            {
                throw new ApplicationException($"Message type {context.MessagingEnvelope.Payload.GetType()} cannot be handled by mediatR");
            }

            await next();
        }
    }
}
