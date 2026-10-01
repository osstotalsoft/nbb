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
    public class MediatRMiddleware(IMediator mediator) : IPipelineMiddleware<MessagingContext>
    {
        public async Task Invoke(MessagingContext context, CancellationToken cancellationToken, Func<Task> next)
        {
            if (context.MessagingEnvelope.Payload is INotification @event)
            {
                await mediator.Publish(@event, cancellationToken);
            }
            else if (context.MessagingEnvelope.Payload is IRequest request)
            {
                await mediator.Send(request, cancellationToken);
            }
            else
            {
                throw new ApplicationException($"Message type {context.MessagingEnvelope.Payload.GetType()} cannot be handled by mediatR");
            }

            await next();
        }
    }

    public static class MediatRMessagingPipelineExtensions
    {
        /// <summary>
        /// Adds to the pipeline a middleware that sends/publishes messages that are events or commands to MediatR.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        /// <returns>The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.</returns>
        public static IPipelineBuilder<MessagingContext> UseMediatRMiddleware(this IPipelineBuilder<MessagingContext> pipelineBuilder)
            => pipelineBuilder.UseMiddleware<MediatRMiddleware>();
    }
}
