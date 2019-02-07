using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NBB.Messaging.DataContracts;

namespace NBB.Messaging.Abstractions
{
    public interface IMessagingTopicSubscriber
    {
        Task SubscribeAsync(string topic, Func<string, Task> handler, CancellationToken token, MessagingSubscriberOptions options = null);
        Task UnSubscribeAsync(CancellationToken token);
    }

}
