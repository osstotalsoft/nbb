﻿using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public interface IMessagingTopicPublisher
    {
        Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default);
    }
}
