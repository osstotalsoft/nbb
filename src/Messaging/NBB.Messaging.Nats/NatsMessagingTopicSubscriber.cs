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
    public class NatsMessagingTopicSubscriber : IMessagingTopicSubscriber, IDisposable
    {
        private readonly StanConnectionProvider _stanConnectionProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NatsMessagingTopicSubscriber> _logger;

        private IStanConnection _stanConnection;
        private bool _subscribedToTopic;

        public NatsMessagingTopicSubscriber(StanConnectionProvider stanConnectionProvider, IConfiguration configuration, 
            ILogger<NatsMessagingTopicSubscriber> logger)
        {
            _stanConnectionProvider = stanConnectionProvider;
            _configuration = configuration;
            _logger = logger;
        }

        public Task SubscribeAsync(string topic, Func<string, Task> handler, CancellationToken token, MessagingSubscriberOptions options = null)
        {
            if (!_subscribedToTopic)
            {
                lock (this)
                {
                    if (!_subscribedToTopic)
                    {
                        _subscribedToTopic = true;
                        return SubscribeToTopicAsync(topic, handler, token, options);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task UnSubscribeAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        private Task SubscribeToTopicAsync(string subject, Func<string, Task> handler, CancellationToken token, MessagingSubscriberOptions options = null)
        {
            var opts = StanSubscriptionOptions.GetDefaultOptions();
            opts.DurableName = _configuration.GetSection("Messaging").GetSection("Nats")["durableName"];
            _stanConnection = _stanConnectionProvider.GetConnection();
            var qGroup = _configuration.GetSection("Messaging").GetSection("Nats")["qGroup"];
            var _subscriberOptions = options ?? new MessagingSubscriberOptions();
            opts.ManualAcks = _subscriberOptions.AcknowledgeStrategy != MessagingAcknowledgeStrategy.Auto;
            
            //https://github.com/nats-io/go-nats-streaming#subscriber-rate-limiting
            opts.MaxInflight = 1;
            opts.AckWait = 50000;
            
            void StanMsgHandler(object obj, StanMsgHandlerArgs args)
            {
                _logger.LogDebug("Nats subscriber {QGroup} received message from subject {Subject}", qGroup,
                    subject);

                var json = System.Text.Encoding.UTF8.GetString(args.Message.Data);

                try
                {
                    if (_subscriberOptions.HandlerStrategy == MessagingHandlerStrategy.Serial)
                    {
                        handler(json).Wait(token);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        "Nats consumer {QGroup} encountered an error when handling a message from subject {Subject}.\n {Error}",
                        qGroup, subject, ex);
                    //TODO: push to DLQ
                }

                if (_subscriberOptions.AcknowledgeStrategy == MessagingAcknowledgeStrategy.Serial)
                {
                    args.Message.Ack();
                }
            }

            var s = _subscriberOptions.ConsumerType == MessagingConsumerType.CollaborativeConsumer 
                    ? _stanConnection.Subscribe(subject, opts, StanMsgHandler)
                    : _stanConnection.Subscribe(subject, qGroup, opts, StanMsgHandler);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _stanConnection.Dispose();
        }
    }
}
