using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NBB.Messaging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Kafka
{
    public class KafkaMessagingTopicPublisher : IMessagingTopicPublisher, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly Producer<string, string> _producer;
        private readonly ILogger<KafkaMessagingTopicPublisher> _logger;

        public KafkaMessagingTopicPublisher(IConfiguration configuration, ILogger<KafkaMessagingTopicPublisher> logger)
        {
            _configuration = configuration;
            _producer = GetProducer();
            _logger = logger;
        }

        public async Task PublishAsync(string topic, string key, string message, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            await _producer.ProduceAsync(topic, key, message);
            stopWatch.Stop();
            _logger.LogDebug("Kafka message produced to topic {TopicName} in {ElapsedMilliseconds} ms", topic, stopWatch.ElapsedMilliseconds);

            //https://github.com/confluentinc/confluent-kafka-dotnet/issues/92
            await Task.Yield();
        }

        private Producer<string, string> GetProducer()
        {
            var kafkaServers = _configuration.GetSection("Messaging").GetSection("Kafka")["bootstrap_servers"];
            var config = new Dictionary<string, object>
            {
                {"bootstrap.servers", kafkaServers},
                {"socket.blocking.max.ms", 50}
            };
            var producer = new Producer<string, string>(config, new StringSerializer(Encoding.UTF8),
                new StringSerializer(Encoding.UTF8));

            return producer;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _producer?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}