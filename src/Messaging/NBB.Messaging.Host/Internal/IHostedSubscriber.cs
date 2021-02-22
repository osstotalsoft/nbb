using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host.Internal
{
    internal interface IHostedSubscriber
    {
        Task<HostedSubscription> SubscribeAsync(PipelineDelegate<MessagingContext> pipeline,
            MessagingSubscriberOptions options = null,
            CancellationToken cancellationToken = default);
    }
}
