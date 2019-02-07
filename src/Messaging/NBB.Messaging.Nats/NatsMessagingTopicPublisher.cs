using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using NBB.Messaging.Nats.Internal;

namespace NBB.Messaging.Nats
{
    public class NatsMessagingTopicPublisher : IMessagingTopicPublisher
    {
        private readonly StanConnectionProvider _stanConnectionProvider;
        private readonly ILogger<NatsMessagingTopicPublisher> _logger;

        public NatsMessagingTopicPublisher(StanConnectionProvider stanConnectionProvider, ILogger<NatsMessagingTopicPublisher> logger)
        {
            _stanConnectionProvider = stanConnectionProvider;
            _logger = logger;
        }

        public async Task PublishAsync(string topic, string key, string message, CancellationToken cancellationToken = default(CancellationToken))
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var connection = _stanConnectionProvider.GetConnection();
            var result = await connection.PublishAsync(topic, System.Text.Encoding.UTF8.GetBytes(message));
            stopWatch.Stop();

            _logger.LogDebug("Nats message published to subject {Subject} in {ElapsedMilliseconds} ms", topic, stopWatch.ElapsedMilliseconds);
        }
    }
}