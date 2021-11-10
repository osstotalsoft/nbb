// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
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
