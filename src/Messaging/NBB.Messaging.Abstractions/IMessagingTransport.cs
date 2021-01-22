﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public interface IMessagingTransport
    {
        /// <summary>
        /// Subscribes a message handler to the given topic
        /// </summary>
        /// <param name="topic">The topic/channel to subscribe to</param>
        /// <param name="handler">The message handler</param>
        /// <param name="options">Subscription options</param>
        /// <param name="cancellationToken"></param>
        /// <returns>An object that when disposed unsubscribes the handler from the topic</returns>
        Task<IDisposable> SubscribeAsync(string topic, Func<byte[], Task> handler,
            SubscriptionTransportOptions options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Publishes a message to a topic
        /// </summary>
        /// <param name="topic">The topic/channel to publish to</param>
        /// <param name="message">The message</param>
        /// <param name="cancellationToken"></param>
        Task PublishAsync(string topic, byte[] message, CancellationToken cancellationToken = default);
    }
}