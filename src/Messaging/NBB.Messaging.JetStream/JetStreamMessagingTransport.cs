// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Options;
using NATS.Client;
using NATS.Client.JetStream;
using NBB.Messaging.Abstractions;
using NBB.Messaging.JetStream.Internal;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.JetStream
{
    public class JetStreamMessagingTransport : IMessagingTransport
    {
        private readonly IOptions<JetStreamOptions> _natsOptions;
        private readonly JetStreamConnectionProvider _natsConnectionManager;

        public JetStreamMessagingTransport(IOptions<JetStreamOptions> natsOptions, JetStreamConnectionProvider natsConnectionManager)
        {
            _natsOptions = natsOptions;
            _natsConnectionManager = natsConnectionManager;
        }

        public Task PublishAsync(string topic, TransportSendContext sendContext, CancellationToken cancellationToken = default)
        {
            var envelopeData = sendContext.EnvelopeBytesAccessor.Invoke();

            return _natsConnectionManager.ExecuteAsync(con =>
            {
                IJetStream js = con.CreateJetStreamContext();
                return js.PublishAsync(topic, envelopeData);
            });
        }

        public Task<IDisposable> SubscribeAsync(string topic,
            Func<TransportReceiveContext, Task> handler,
            SubscriptionTransportOptions options = null,
            CancellationToken cancellationToken = default)
        {

            IDisposable consumer = null;

            _natsConnectionManager.Execute(con =>
            {
                IJetStream js = con.CreateJetStreamContext();

                // set's up the stream
                var isCommand = topic.ToLower().Contains("commands.");

                var stream = isCommand ? _natsOptions.Value.CommandsStream : _natsOptions.Value.EventsStream;
                var jsm = con.CreateJetStreamManagementContext();
                jsm.GetStreamInfo(stream);

                // get stream context, create consumer and get the consumer context
                var streamContext = con.GetStreamContext(stream);

                var subscriberOptions = options ?? SubscriptionTransportOptions.Default;
                var ccb = ConsumerConfiguration.Builder();

                if (subscriberOptions.IsDurable)
                {
                    var clientId = (_natsOptions.Value.ClientId + topic).Replace(".", "_");
                    ccb.WithDurable(clientId);
                }

                if (subscriberOptions.DeliverNewMessagesOnly)
                    ccb.WithDeliverPolicy(DeliverPolicy.New);
                else
                    ccb.WithDeliverPolicy(DeliverPolicy.All);

                ccb.WithAckWait(subscriberOptions.AckWait ?? _natsOptions.Value.AckWait ?? 50000);

                //https://docs.nats.io/nats-concepts/jetstream/consumers#maxackpending
                ccb.WithMaxAckPending(subscriberOptions.MaxConcurrentMessages);
                ccb.WithFilterSubject(topic);

                var consumerContext = streamContext.CreateOrUpdateConsumer(ccb.Build());

                void NatsMsgHandler(object obj, MsgHandlerEventArgs args)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    var receiveContext = new TransportReceiveContext(new TransportReceivedData.EnvelopeBytes(args.Message.Data));

                    // Fire and forget
                    _ = handler(receiveContext).ContinueWith(_ => args.Message.Ack(), cancellationToken);
                }
                consumer = consumerContext.Consume(NatsMsgHandler);

            });
            return Task.FromResult(consumer);
        }
    }
}
