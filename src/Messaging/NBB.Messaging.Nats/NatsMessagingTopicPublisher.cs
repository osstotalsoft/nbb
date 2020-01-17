using Microsoft.Extensions.Logging;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Nats.Internal;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Nats
{
    public class NatsMessagingTopicPublisher : IMessagingTopicPublisher
    {
        private readonly StanConnectionProvider _stanConnectionManager;
        private readonly ILogger<NatsMessagingTopicPublisher> _logger;

        public NatsMessagingTopicPublisher(StanConnectionProvider stanConnectionManager,
            ILogger<NatsMessagingTopicPublisher> logger)
        {
            _stanConnectionManager = stanConnectionManager;
            _logger = logger;
        }

        public async Task PublishAsync(string topic, string key, string message,
            CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            await _stanConnectionManager.ExecuteAsync(async connection =>
                await connection.PublishAsync(topic, System.Text.Encoding.UTF8.GetBytes(message)));
            stopWatch.Stop();

            _logger.LogInformation("Nats message published to subject {Subject} in {ElapsedMilliseconds} ms", topic,
                stopWatch.ElapsedMilliseconds);
        }
    }
}