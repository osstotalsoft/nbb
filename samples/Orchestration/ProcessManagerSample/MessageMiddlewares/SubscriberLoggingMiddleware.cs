using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;

namespace ProcessManagerSample.MessageMiddlewares
{
    public class SubscriberLoggingMiddleware : IPipelineMiddleware<MessagingEnvelope>
    {
        private readonly ILogger<SubscriberLoggingMiddleware> _logger;

        public SubscriberLoggingMiddleware(ILogger<SubscriberLoggingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(MessagingEnvelope messageEnvelope, CancellationToken cancellationToken, Func<Task> next)
        {
            _logger.LogDebug("Message {@Message} was received.", messageEnvelope.Payload);
            await next();
        }
    }
}