using System;
using Polly;

namespace NBB.Resiliency
{
    public interface IResiliencyPolicyProvider
    {
        AsyncPolicy GetOutOfOrderPolicy(Action<int> onRetry);
        AsyncPolicy GetConcurencyExceptionPolicy(Action<Exception> onRetry);
    }
}
