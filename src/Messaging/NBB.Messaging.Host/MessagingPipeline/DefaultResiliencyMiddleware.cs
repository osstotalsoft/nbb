using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using NBB.Messaging.DataContracts;
using NBB.Resiliency;
using Polly;
using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host.MessagingPipeline
{
    /// <summary>
    /// A pipeline middleware that offers resiliency policies for "out of order" and concurrency exceptions.
    /// </summary>
    /// <seealso cref="NBB.Core.Pipeline.IPipelineMiddleware{NBB.Messaging.DataContracts.MessagingEnvelope}" />
    public class DefaultResiliencyMiddleware : IPipelineMiddleware<MessagingEnvelope>
    {
        private readonly IResiliencyPolicyProvider _resiliencyPolicyProvider;
        private readonly ILogger<DefaultResiliencyMiddleware> _logger;

        public DefaultResiliencyMiddleware(IResiliencyPolicyProvider resiliencyPolicyProvider, ILogger<DefaultResiliencyMiddleware> logger)
        {
            _resiliencyPolicyProvider = resiliencyPolicyProvider;
            _logger = logger;
        }

        public async Task Invoke(MessagingEnvelope message, CancellationToken cancellationToken, Func<Task> next)
        {
            var outOfOrderPolicy = _resiliencyPolicyProvider.GetOutOfOrderPolicy(retryCount => _logger.LogWarning(
                  "Message of type {MessageType} could not be processed due to OutOfOrderMessageException. Retry count is {RetryCount}.",
                  message.Payload.GetType().GetPrettyName(), retryCount));

            var concurrencyException = _resiliencyPolicyProvider.GetConcurencyExceptionPolicy(ex =>
                _logger.LogWarning(
                    "Message of type {MessageType} could not be processed due to concurrency exception. The system will automatically retry it.",
                    message.Payload.GetType().GetPrettyName()));

            var policies = Policy.WrapAsync(outOfOrderPolicy, concurrencyException);

            var result = await policies.ExecuteAndCaptureAsync(async () =>
            {
                await next();
            });

            if (result.Outcome == OutcomeType.Failure)
            {
                ExceptionDispatchInfo.Capture(result.FinalException).Throw();
            }
        }
    }
}
