#if AddSamples
using Microsoft.Extensions.Logging;
using NBB.Core.Pipeline;
using NBB.Messaging.DataContracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Worker.Messaging
{
    public class SampleSubscriberPipelineMiddleware: IPipelineMiddleware<MessagingEnvelope>
    {
        private readonly ILogger<SampleSubscriberPipelineMiddleware> _logger;

        public SampleSubscriberPipelineMiddleware(ILogger<SampleSubscriberPipelineMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(MessagingEnvelope message, CancellationToken cancellationToken, Func<Task> next)
        {
            _logger.LogDebug("Message {@Message} was received.", message.Payload);
            await next();
        }
    }
}
#endif