// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CorrelationManager = NBB.Correlation.CorrelationManager;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    /// <summary>
    /// Pipeline middleware used to create a correlation scope from the correlation id received in the message headers.
    /// </summary>
    /// <seealso cref="IPipelineMiddleware{MessagingContext}" />
    public class CorrelationMiddleware : IPipelineMiddleware<MessagingContext>
    {
        public async Task Invoke(MessagingContext context, CancellationToken cancellationToken, Func<Task> next)
        {
            using (CorrelationManager.NewCorrelationId(context.MessagingEnvelope.GetCorrelationId()))
            {
                Activity.Current?.SetTag(TracingTags.CorrelationId, CorrelationManager.GetCorrelationId()?.ToString());

                await next();
            }
        }
    }
}
