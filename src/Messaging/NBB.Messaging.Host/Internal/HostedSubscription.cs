using Microsoft.Extensions.Logging;
using System;

namespace NBB.Messaging.Host.Internal
{
    internal class HostedSubscription : IDisposable
    {
        private readonly IDisposable _subscription;
        private readonly string _topicName;
        private readonly ILogger<MessagingHost> _logger;

        public HostedSubscription(IDisposable subscription, string topicName, ILogger<MessagingHost> logger)
        {
            _subscription = subscription;
            _topicName = topicName;
            _logger = logger;
        }

        public void Dispose()
        {
            _logger.LogInformation("Messaging subscriber for topic {TopicName} is stopping", _topicName);

            _subscription.Dispose();
        }
    }
}
