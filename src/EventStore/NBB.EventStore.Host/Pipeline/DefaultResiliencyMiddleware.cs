using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using NBB.Resiliency;
using Polly;
using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;


namespace NBB.EventStore.Host.Pipeline
{
    public class DefaultResiliencyMiddleware : IPipelineMiddleware<object>
    {
        private readonly IResiliencyPolicyProvider _resiliencyPolicyProvider;
        private readonly ILogger<DefaultResiliencyMiddleware> _logger;

        public DefaultResiliencyMiddleware(IResiliencyPolicyProvider resiliencyPolicyProvider, ILogger<DefaultResiliencyMiddleware> logger)
        {
            _resiliencyPolicyProvider = resiliencyPolicyProvider;
            _logger = logger;
        }

        public async Task Invoke(object @event, CancellationToken cancellationToken, Func<Task> next)
        {
            var outOfOrderPolicy = _resiliencyPolicyProvider.GetOutOfOrderPolicy(retryCount => _logger.LogWarning(
                "Event of type {EventType} could not be processed due to OutOfOrderMessageException. Retry count is {RetryCount}.",
                @event.GetType().GetPrettyName(), retryCount));

            var concurencyException = _resiliencyPolicyProvider.GetConcurencyExceptionPolicy(ex => _logger.LogWarning(
                "Event of type {EventType} could not be processed due to concurency exception. The system will automatically retry it.",
                @event.GetType().GetPrettyName()));

            var policies = Policy.WrapAsync(outOfOrderPolicy, concurencyException);

            var result = await policies.ExecuteAndCaptureAsync(async (_) =>
            {
                await next();
            }, cancellationToken);

            if (result.Outcome == OutcomeType.Failure)
            {
                ExceptionDispatchInfo.Capture(result.FinalException).Throw();
            }
        }
    }
}
