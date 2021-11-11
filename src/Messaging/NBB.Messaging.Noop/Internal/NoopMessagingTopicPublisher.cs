using NBB.Messaging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Noop.Internal
{
    public class NoopMessagingTopicPublisher : IMessagingTopicPublisher
    {
        public Task PublishAsync(string topic, string key, string message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}