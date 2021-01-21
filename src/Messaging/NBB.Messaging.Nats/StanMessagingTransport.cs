using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Nats.Internal;
using STAN.Client;

namespace NBB.Messaging.Nats
{
    public class StanMessagingTransport : IMessagingTransport
    {
        private readonly StanConnectionProvider _stanConnectionManager;
        private readonly IConfiguration _configuration;

        public StanMessagingTransport(StanConnectionProvider stanConnectionManager, IConfiguration configuration)
        {
            _stanConnectionManager = stanConnectionManager;
            _configuration = configuration;
        }

        public Task<IDisposable> SubscribeAsync(string topic, Func<byte[], Task> handler,
            SubscriptionTransportOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var opts = StanSubscriptionOptions.GetDefaultOptions();
            var subscriberOptions = options ?? SubscriptionTransportOptions.Default;
            if (subscriberOptions.IsDurable)
            {
                opts.DurableName = _configuration.GetSection("Messaging").GetSection("Nats")["durableName"];
            }

            var qGroup = _configuration.GetSection("Messaging").GetSection("Nats")["qGroup"];

            // https://github.com/nats-io/stan.go#subscriber-rate-limiting
            opts.MaxInflight = subscriberOptions.UseBlockingHandler ? 1 : subscriberOptions.MaxParallelMessages;
            opts.AckWait = _configuration.GetSection("Messaging").GetSection("Nats").GetValue<int?>("ackWait") ?? 50000;
            opts.ManualAcks = subscriberOptions.UseManualAck;

            void StanMsgHandler(object obj, StanMsgHandlerArgs args)
            {
                async Task Handler()
                {
                    await handler(args.Message.Data);

                    if (subscriberOptions.UseManualAck)
                    {
                        args.Message.Ack();
                    }
                }

                if (subscriberOptions.UseBlockingHandler)
                {
                    Handler().Wait(cancellationToken);
                }
                else
                {
                    Task.Run(Handler, cancellationToken);
                }
            }

            IDisposable subscription = null;
            _stanConnectionManager.Execute(stanConnection =>
            {
                subscription = subscriberOptions.UseGroup
                    ? stanConnection.Subscribe(topic, qGroup, opts, StanMsgHandler)
                    : stanConnection.Subscribe(topic, opts, StanMsgHandler);
            });

            return Task.FromResult(subscription);
        }

        public Task PublishAsync(string topic, byte[] message, CancellationToken cancellationToken = default)
        {
            return _stanConnectionManager.ExecuteAsync(async connection =>
                await connection.PublishAsync(topic, message));
        }
    }
}