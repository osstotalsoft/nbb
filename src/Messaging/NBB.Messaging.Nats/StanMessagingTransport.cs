// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Options;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Nats.Internal;
using STAN.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Nats
{
    public class StanMessagingTransport : IMessagingTransport
    {
        private readonly StanConnectionProvider _stanConnectionManager;
        private readonly IOptions<NatsOptions> _natsOptions;

        public StanMessagingTransport(StanConnectionProvider stanConnectionManager, IOptions<NatsOptions> natsOptions)
        {
            _stanConnectionManager = stanConnectionManager;
            _natsOptions = natsOptions;
        }

        public Task<IDisposable> SubscribeAsync(string topic, Func<TransportReceiveContext, Task> handler,
            SubscriptionTransportOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var opts = StanSubscriptionOptions.GetDefaultOptions();
            var subscriberOptions = options ?? SubscriptionTransportOptions.Default;
            if (subscriberOptions.IsDurable)
            {
                opts.DurableName = _natsOptions.Value.DurableName;
                opts.LeaveOpen = true;
            }

            if (!subscriberOptions.DeliverNewMessagesOnly)
            {
                opts.DeliverAllAvailable();
            }

            // https://github.com/nats-io/stan.go#subscriber-rate-limiting
            opts.MaxInflight = subscriberOptions.MaxConcurrentMessages;
            opts.AckWait = subscriberOptions.AckWait ?? _natsOptions.Value.AckWait ?? 50000;
            opts.ManualAcks = true;

            //CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            async void StanMsgHandler(object obj, StanMsgHandlerArgs args)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var receiveContext = new TransportReceiveContext(new TransportReceivedData.EnvelopeBytes(args.Message.Data));

                await handler(receiveContext);
                
                args.Message.Ack();
            }

            IDisposable subscription = null;
            _stanConnectionManager.Execute(stanConnection =>
            {
                subscription = subscriberOptions.UseGroup
                    ? stanConnection.Subscribe(topic, _natsOptions.Value.QGroup, opts, StanMsgHandler)
                    : stanConnection.Subscribe(topic, opts, StanMsgHandler);
            });

            return Task.FromResult(subscription);
        }

        public Task PublishAsync(string topic, TransportSendContext sendContext, CancellationToken cancellationToken = default)
        {
            var envelopeData = sendContext.EnvelopeBytesAccessor.Invoke();

            return _stanConnectionManager.ExecuteAsync(
                async connection => await connection.PublishAsync(topic, envelopeData));
        }
    }
}
