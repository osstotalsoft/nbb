// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    /// <summary>
    /// A pipeline middleware that validates outgoing message's schema.
    /// </summary>
    /// <seealso cref="NBB.Core.Pipeline.IPipelineMiddleware{MessagingEnvelope}" />
    public class SchemaMessageValidatorMiddleware : IPipelineMiddleware<MessagingContext>
    {
        public async Task Invoke(MessagingContext context, CancellationToken cancellationToken, Func<Task> next)
        {
            if (!context.MessagingEnvelope.Headers.TryGetValue(MessagingHeaders.MessageType, out var _))
                throw new Exception($"Message of type {context.MessagingEnvelope.Payload.GetType().GetPrettyName()} does not contain {MessagingHeaders.MessageType} header.");

            if (!context.MessagingEnvelope.Headers.TryGetValue(MessagingHeaders.CorrelationId, out var _))
                throw new Exception($"Message of type {context.MessagingEnvelope.Payload.GetType().GetPrettyName()} does not contain {MessagingHeaders.CorrelationId} header.");

            if (!context.MessagingEnvelope.Headers.TryGetValue(MessagingHeaders.MessageId, out var _))
                throw new Exception($"Message of type {context.MessagingEnvelope.Payload.GetType().GetPrettyName()} does not contain {MessagingHeaders.MessageId} header.");

            if (!context.MessagingEnvelope.Headers.TryGetValue(MessagingHeaders.PublishTime, out var _))
                throw new Exception($"Message of type {context.MessagingEnvelope.Payload.GetType().GetPrettyName()} does not contain {MessagingHeaders.PublishTime} header.");

            if (!context.MessagingEnvelope.Headers.TryGetValue(MessagingHeaders.Source, out var _))
                throw new Exception($"Message of type {context.MessagingEnvelope.Payload.GetType().GetPrettyName()} does not contain {MessagingHeaders.Source} header.");

            await next();
        }
    }
}
