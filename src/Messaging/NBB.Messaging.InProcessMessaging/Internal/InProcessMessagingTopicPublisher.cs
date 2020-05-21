using Microsoft.Extensions.Logging;
using NBB.Messaging.Abstractions;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.InProcessMessaging.Internal
{
    public class InProcessMessagingTopicPublisher : IMessagingTopicPublisher
    {
        private readonly IStorage _storage;
        private readonly ILogger<InProcessMessagingTopicPublisher> _logger;

        public InProcessMessagingTopicPublisher(IStorage storage, ILogger<InProcessMessagingTopicPublisher> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public Task PublishAsync(string topic, string key, string message, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            _storage.Enqueue(message, topic);
            stopWatch.Stop();

            _logger.LogDebug("InProcess message published to topic {Topic} in {ElapsedMilliseconds} ms", topic, stopWatch.ElapsedMilliseconds);

            return Task.CompletedTask;
        }
    }
}