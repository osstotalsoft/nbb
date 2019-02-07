using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NBB.Messaging.DataContracts;

namespace NBB.Messaging.Abstractions
{
    public interface IMessagingTopicPublisher
    {
        Task PublishAsync(string topic, string key, string message, CancellationToken cancellationToken = default(CancellationToken));
    }
}
