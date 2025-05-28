// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace NBB.Messaging.Kafka.Internal
{
    public class KafkaConnectionProvider : IAsyncDisposable
    {
        private readonly IOptions<KafkaOptions> _options;
        private readonly ILogger<KafkaConnectionProvider> _logger;
        private IProducer<Null, byte[]> _producer;
        private IConsumer<Ignore, byte[]> _consumer;
        private static readonly object InstanceLocker = new();

        public KafkaConnectionProvider(IOptions<KafkaOptions> options, ILogger<KafkaConnectionProvider> logger)
        {
            _options = options;
            _logger = logger;
        }

        public IProducer<Null, byte[]> GetProducer()
        {
            if (_producer == null)
                lock (InstanceLocker)
                    if (_producer == null)
                        _producer = CreateProducer();
            return _producer;
        }

        public IConsumer<Ignore, byte[]> GetConsumer()
        {
            if (_consumer == null)
                lock (InstanceLocker)
                    if (_consumer == null)
                        _consumer = CreateConsumer();
            return _consumer;
        }

        private IProducer<Null, byte[]> CreateProducer()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _options.Value.BootstrapServers,
                ClientId = _options.Value.ClientId
            };
            var producer = new ProducerBuilder<Null, byte[]>(config).Build();
            _logger.LogInformation($"Kafka producer connected to {_options.Value.BootstrapServers}");
            return producer;
        }

        private IConsumer<Ignore, byte[]> CreateConsumer()
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _options.Value.BootstrapServers,
                GroupId = _options.Value.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                ClientId = _options.Value.ClientId
            };
            var consumer = new ConsumerBuilder<Ignore, byte[]>(config).Build();
            _logger.LogInformation($"Kafka consumer connected to {_options.Value.BootstrapServers}");
            return consumer;
        }

        public async ValueTask DisposeAsync()
        {
            _producer?.Flush(TimeSpan.FromSeconds(5));
            _producer?.Dispose();
            _consumer?.Close();
            _consumer?.Dispose();
            await Task.CompletedTask;
        }
    }
}
