using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public interface IMessageBusPublisher
    {
        Task PublishAsync<T>(T message, MessagingPublisherOptions options = null,
            CancellationToken cancellationToken = default);

        Task PublishAsync<T>(T message, CancellationToken cancellationToken) =>
            PublishAsync(message, null, cancellationToken);
    }
}