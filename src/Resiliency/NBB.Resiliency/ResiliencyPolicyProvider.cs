using System;
using NBB.Core.Abstractions;
using Polly;

namespace NBB.Resiliency
{
    public class ResiliencyPolicyProvider : IResiliencyPolicyProvider
    {
        public Policy GetOutOfOrderPolicy(Action<int> onRetry)
        {
            var policy = Policy
                .Handle<OutOfOrderMessageException>()
                .WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(Math.Pow(i, 2)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        onRetry(retryCount);
                    });

            return policy;
        }

        public Policy GetConcurencyExceptionPolicy(Action<Exception> onRetry)
        {
            var policy = Policy
                .Handle<ConcurrencyException>()
                .RetryForeverAsync(onRetry);

            return policy;
        }
    }
}
