﻿// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
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
        Task<IDisposable> SubscribeAsync(string topic, Func<TransportReceiveContext, Task> handler,
            SubscriptionTransportOptions options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Publishes a message to a topic
        /// </summary>
        /// <param name="topic">The topic/channel to publish to</param>
        /// <param name="message">The message</param>
        /// <param name="cancellationToken"></param>
        Task PublishAsync(string topic, TransportSendContext message, CancellationToken cancellationToken = default);
    }


    public interface ITransportMonitor
    {
        public void OnError(Exception error);
    }

    public interface ITransportMonitorHandler
    {
        public void OnError(Exception error);
    }

    public class DefaultTransportMonitor : ITransportMonitor
    {
        private readonly IEnumerable<ITransportMonitorHandler> _hadlers;

        public DefaultTransportMonitor(IEnumerable<ITransportMonitorHandler> hadlers)
        {
            _hadlers = hadlers;
        }

        public void OnError(Exception error)
        {
            foreach (var handler in _hadlers)
            {
                handler.OnError(error);
            }
        }
    }

    public record TransportSendContext(
        Func<(byte[] payloadData, IDictionary<string, string> additionalHeaders)> PayloadBytesAccessor,
        Func<byte[]> EnvelopeBytesAccessor,
        Func<IDictionary<string, string>> HeadersAccessor);

    public record TransportReceiveContext(TransportReceivedData ReceivedData);

    
    public abstract record TransportReceivedData
    {
        public record EnvelopeBytes(byte[] Bytes) : TransportReceivedData;
        public record PayloadBytesAndHeaders(byte[] PayloadBytes, IDictionary<string, string> headers) : TransportReceivedData;
    }

}
