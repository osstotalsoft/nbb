using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NBB.Messaging.Abstractions
{
    public class MessagingTopicPublisher : IMessagingTopicPublisher
    {
        private readonly IMessagingTransport _messagingTransport;
        private readonly ILogger<MessagingTopicPublisher> _logger;

        public MessagingTopicPublisher(IMessagingTransport messagingTransport,
            ILogger<MessagingTopicPublisher> logger)
        {
            _messagingTransport = messagingTransport;
            _logger = logger;
        }

        public async Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            await _messagingTransport.PublishAsync(topic, System.Text.Encoding.UTF8.GetBytes(message),
                cancellationToken);
            stopWatch.Stop();

            _logger.LogDebug("Nats message published to subject {Subject} in {ElapsedMilliseconds} ms", topic,
                stopWatch.ElapsedMilliseconds);
        }
    }
}