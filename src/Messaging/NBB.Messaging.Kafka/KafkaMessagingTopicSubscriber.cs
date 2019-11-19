using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NBB.Messaging.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Kafka
{
    public class KafkaMessagingTopicSubscriber: IMessagingTopicSubscriber, IDisposable
    {
        private Consumer<string, string> _consumer;
        private readonly ILogger<KafkaMessagingTopicSubscriber> _logger;
        private readonly string _consumerGroup;
        private readonly string _kafkaServers;
        private bool _subscribedToTopic;
        private MessagingSubscriberOptions _subscriberOptions;

        public KafkaMessagingTopicSubscriber(IConfiguration configuration, ILogger<KafkaMessagingTopicSubscriber> logger)
        {
            _logger = logger;
            _consumerGroup = configuration.GetSection("Messaging").GetSection("Kafka")["group_id"];
            _kafkaServers = configuration.GetSection("Messaging").GetSection("Kafka")["bootstrap_servers"];
        }

        public Task SubscribeAsync(string topicName, Func<string, Task> handler, CancellationToken cancellationToken = default, MessagingSubscriberOptions options = null)
        {
            if (!_subscribedToTopic)
            {
                lock (this)
                {
                    if (!_subscribedToTopic)
                    {
                        _subscribedToTopic = true;
                        _subscriberOptions = options ?? new MessagingSubscriberOptions();
                        _consumer = GetConsumer();
                        return SubscribeToTopicAsync(topicName, handler, cancellationToken);
                    }
                }
            }

            throw new Exception("Already subscribed to this topic: " + topicName);
        }

        private async Task SubscribeToTopicAsync(string topicName, Func<string, Task> handler, CancellationToken cancellationToken = default)
        {
            

            _consumer.OnPartitionsAssigned += (_, partitions) =>
            {
                _logger.LogDebug("Kafka _consumer {ConsumerGroup} assigned partitions: {Partitions}", _consumerGroup,
                    string.Join(", ", partitions));
                _consumer.Assign(partitions);

            };
            _consumer.OnPartitionsRevoked += (_, partitions) =>
            {
                _logger.LogDebug("Kafka _consumer {ConsumerGroup} revoked partitions: {Partitions}", _consumerGroup,
                    string.Join(", ", partitions));
                _consumer.Unassign();
            };

            await Task.Yield();
            _consumer.Subscribe(topicName);

            var pollTask = Task.Run(async () => await Poll(topicName, handler, _consumer, cancellationToken), cancellationToken); //start polling on another thread

        }

        public Task UnSubscribeAsync(CancellationToken cancellationToken = default)
        {
            //_consumer.Unsubscribe();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _consumer?.Dispose();
        }


        private async Task Poll(string topicName, Func<string, Task> handler,Consumer<string, string> consumer, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (consumer.Consume(out Message<string, string> message, TimeSpan.FromMilliseconds(100)))
                {
                    _logger.LogDebug("Kafka _consumer {ConsumerGroup} received message from topic {TopicName}",
                        _consumerGroup, topicName);

                    try
                    {
                        if (_subscriberOptions.HandlerStrategy == MessagingHandlerStrategy.Serial)
                        {
                            await handler(message.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            "Kafka _consumer {ConsumerGroup} encountered an error when handling a message from topic {TopicName}.\n {Error}",
                            _consumerGroup, topicName, ex);
                        //TODO: push to DLQ
                    }
                }

                if (_subscriberOptions.AcknowledgeStrategy == MessagingAcknowledgeStrategy.Serial)
                {
                    await consumer.CommitAsync();
                }
            }
        }
        private Consumer<string, string> GetConsumer()
        {
            var isAutoCommit = _subscriberOptions.AcknowledgeStrategy == MessagingAcknowledgeStrategy.Auto;
            var config = new Dictionary<string, object>
            {
                {"bootstrap.servers", _kafkaServers},
                {"enable.auto.commit", isAutoCommit ? "true" : "false"},
                {"auto.offset.reset", "earliest"},
                {"group.id", _consumerGroup}
                //{"queued.min.messages", "1000" }
            };

            var isCollaborativeConsumer = _subscriberOptions.ConsumerType == MessagingConsumerType.CollaborativeConsumer;
            if (isCollaborativeConsumer)
            {
                config["group.id"] += Guid.NewGuid().ToString(); // Data for new group id kept for the default retention time?
                config["auto.offset.reset"] = "latest"; // Needed to process only new messages for the new group id. Otherwise it processes all messages fond on topic
            }

            var consumer = new Consumer<string, string>(config, new StringDeserializer(Encoding.UTF8),
                new StringDeserializer(Encoding.UTF8));

            return consumer;
        }
    }
}