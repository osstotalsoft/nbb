using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
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
    /// <seealso cref="NBB.Core.Pipeline.IPipelineMiddleware{MessagingEnvelope}" />
    public class DefaultResiliencyMiddleware : IPipelineMiddleware<MessagingContext>
    {
        private readonly ILogger<DefaultResiliencyMiddleware> _logger;

        public DefaultResiliencyMiddleware(ILogger<DefaultResiliencyMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(MessagingContext context, CancellationToken cancellationToken, Func<Task> next)
        {
            var outOfOrderPolicy = GetOutOfOrderPolicy(retryCount => _logger.LogWarning(
                "Message of type {MessageType} could not be processed due to OutOfOrderMessageException. Retry count is {RetryCount}.",
                context.MessagingEnvelope.Payload.GetType().GetPrettyName(), retryCount));

            var concurrencyException = GetConcurrencyExceptionPolicy(_ =>
                _logger.LogWarning(
                    "Message of type {MessageType} could not be processed due to concurrency exception. The system will automatically retry it.",
                    context.MessagingEnvelope.Payload.GetType().GetPrettyName()));

            var policies = Policy.WrapAsync(outOfOrderPolicy, concurrencyException);

            var result = await policies.ExecuteAndCaptureAsync(async (_) => { await next(); }, cancellationToken);

            if (result.Outcome == OutcomeType.Failure)
            {
                ExceptionDispatchInfo.Capture(result.FinalException).Throw();
            }
        }

        private AsyncPolicy GetOutOfOrderPolicy(Action<int> onRetry)
        {
            var policy = Policy
                .Handle<OutOfOrderMessageException>()
                .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(Math.Pow(i, 2)),
                    (_, _, retryCount, _) => { onRetry(retryCount); });

            return policy;
        }

        private AsyncPolicy GetConcurrencyExceptionPolicy(Action<Exception> onRetry)
        {
            var policy = Policy
                .Handle<ConcurrencyException>()
                .RetryForeverAsync(onRetry);

            return policy;
        }
    }
}