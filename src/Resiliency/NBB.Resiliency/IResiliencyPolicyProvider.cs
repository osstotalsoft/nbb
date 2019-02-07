using System;
using Polly;

namespace NBB.Resiliency
{
    public interface IResiliencyPolicyProvider
    {
        Policy GetOutOfOrderPolicy(Action<int> onRetry);
        Policy GetConcurencyExceptionPolicy(Action<Exception> onRetry);
    }
}
