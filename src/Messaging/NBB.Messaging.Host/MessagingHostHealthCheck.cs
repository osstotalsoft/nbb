// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host
{
    public class MessagingHostHealthCheck : IHealthCheck
    {
        private readonly IMessagingHost _messagingHost;

        public MessagingHostHealthCheck(IMessagingHost messagingHost)
        {
            _messagingHost = messagingHost;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (_messagingHost.IsRunning())
            {
                return Task.FromResult(HealthCheckResult.Healthy("MessagingHost is running."));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("MessagingHost is not running."));
        }
    }
}
