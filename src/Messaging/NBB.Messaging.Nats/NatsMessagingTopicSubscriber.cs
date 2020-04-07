using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Nats.Internal;
using STAN.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Nats
{
    public class NatsMessagingTopicSubscriber : IMessagingTopicSubscriber
    {
        private readonly StanConnectionProvider _stanConnectionManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NatsMessagingTopicSubscriber> _logger;

        private bool _subscribedToTopic;

        public NatsMessagingTopicSubscriber(StanConnectionProvider stanConnectionManager, IConfiguration configuration, 
            ILogger<NatsMessagingTopicSubscriber> logger)
        {
            _stanConnectionManager = stanConnectionManager;
            _configuration = configuration;
            _logger = logger;
        }

        public Task SubscribeAsync(string topic, Func<string, Task> handler, CancellationToken cancellationToken = default, MessagingSubscriberOptions options = null)
        {
            if (!_subscribedToTopic)
            {
                lock (this)
                {
                    if (!_subscribedToTopic)
                    {
                        _subscribedToTopic = true;
                        return SubscribeToTopicAsync(topic, handler, cancellationToken, options);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task UnSubscribeAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        private Task SubscribeToTopicAsync(string subject, Func<string, Task> handler, CancellationToken cancellationToken = default, MessagingSubscriberOptions options = null)
        {
            var opts = StanSubscriptionOptions.GetDefaultOptions();
            opts.DurableName = _configuration.GetSection("Messaging").GetSection("Nats")["durableName"];
            var qGroup = _configuration.GetSection("Messaging").GetSection("Nats")["qGroup"];
            var subscriberOptions = options ?? new MessagingSubscriberOptions();
            opts.ManualAcks = subscriberOptions.AcknowledgeStrategy != MessagingAcknowledgeStrategy.Auto;
            
            //https://github.com/nats-io/go-nats-streaming#subscriber-rate-limiting
            opts.MaxInflight = options.MaxInFlight;
            opts.AckWait = _configuration.GetSection("Messaging").GetSection("Nats").GetValue<int?>("ackWait") ?? 50000;
            
            void StanMsgHandler(object obj, StanMsgHandlerArgs args)
            {
                _logger.LogDebug("Nats subscriber {QGroup} received message from subject {Subject}", qGroup,
                    subject);

                var json = System.Text.Encoding.UTF8.GetString(args.Message.Data);

                async Task Handler()
                {
                    try {
                        await handler(json);

                        if (subscriberOptions.AcknowledgeStrategy == MessagingAcknowledgeStrategy.Manual)
                        {
                            args.Message.Ack();
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(
                            "Nats consumer {QGroup} encountered an error when handling a message from subject {Subject}.\n {Error}",
                            qGroup, subject, ex);
                    }
                }

                if (subscriberOptions.HandlerStrategy == MessagingHandlerStrategy.Serial) 
                {
                    Handler().Wait(cancellationToken);
                }
                else
                {
                    Task.Run(Handler);
                }
            }

            _stanConnectionManager.Execute(stanConnection =>
            {
                var _ = subscriberOptions.ConsumerType == MessagingConsumerType.CollaborativeConsumer
                    ? stanConnection.Subscribe(subject, opts, StanMsgHandler)
                    : stanConnection.Subscribe(subject, qGroup, opts, StanMsgHandler);
            });
            

            return Task.CompletedTask;
        }
    }
}
