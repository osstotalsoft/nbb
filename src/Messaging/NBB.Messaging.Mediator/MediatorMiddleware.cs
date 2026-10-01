// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    /// <summary>
    /// A pipeline middleware that forwards messages that contain notifications, requests, commands or queries to Mediator.
    /// </summary>
    /// <seealso cref="IPipelineMiddleware{MessagingEnvelope}" />
    public class MediatorMiddleware(IMediator mediator) : IPipelineMiddleware<MessagingContext>
    {
        public async Task Invoke(MessagingContext context, CancellationToken cancellationToken, Func<Task> next)
        {
            switch (context.MessagingEnvelope.Payload)
            {
                case INotification notification:
                    await mediator.Publish(notification, cancellationToken);
                    break;
                case IMessage message:
                    await mediator.Send(message, cancellationToken);
                    break;
                default:
                    throw new ApplicationException($"Message type {context.MessagingEnvelope.Payload?.GetType()} cannot be handled by Mediator");
            }

            await next();
        }
    }

    public static class MediatorMessagingPipelineExtensions
    {
        /// <summary>
        /// Adds to the pipeline a middleware that sends/publishes messages that are events, commands or queries to Mediator.
        /// </summary>
        /// <param name="pipelineBuilder">The pipeline builder.</param>
        /// <returns>The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.</returns>
        public static IPipelineBuilder<MessagingContext> UseMediatorMiddleware(this IPipelineBuilder<MessagingContext> pipelineBuilder)
            => pipelineBuilder.UseMiddleware<MediatorMiddleware>();
    }
}
