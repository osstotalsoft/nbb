// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;

namespace ProcessManagerSample.MessageMiddlewares
{
    public class SubscriberLoggingMiddleware : IPipelineMiddleware<MessagingContext>
    {
        private readonly ILogger<SubscriberLoggingMiddleware> _logger;

        public SubscriberLoggingMiddleware(ILogger<SubscriberLoggingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(MessagingContext context, CancellationToken cancellationToken, Func<Task> next)
        {
            _logger.LogDebug("Message {@Message} was received.", context.MessagingEnvelope.Payload);
            await next();
        }
    }
}
