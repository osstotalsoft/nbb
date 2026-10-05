// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Kafka;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NBB.Messaging.Kafka.Tests
{
    public class PublisherTests
    {
        [Fact]
        public async Task Test_publish_envelope_bytes_and_stream_id_key()
        {
            //Arrange
            var envelopeBytes = new byte[] { 1, 2, 3 };
            var producer = Mock.Of<IProducer<byte[], byte[]>>();
            string producedTopic = null;
            Message<byte[], byte[]> produced = null;
            Mock.Get(producer)
                .Setup(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<byte[], byte[]>>(), It.IsAny<CancellationToken>()))
                .Callback((string topic, Message<byte[], byte[]> message, CancellationToken token) =>
                {
                    producedTopic = topic;
                    produced = message;
                })
                .Returns(() => Task.FromResult(new DeliveryResult<byte[], byte[]>()));

            var transport = new KafkaMessagingTransport(producer,
                new KafkaConsumerFactoryImpl(new OptionsWrapper<KafkaOptions>(new KafkaOptions())),
                new OptionsWrapper<KafkaOptions>(new KafkaOptions()));

            var sendContext = new TransportSendContext(
                PayloadBytesAccessor: () => (null, null),
                EnvelopeBytesAccessor: () => envelopeBytes,
                HeadersAccessor: () => new Dictionary<string, string> { [MessagingHeaders.StreamId] = "stream-1" });

            //Act
            await transport.PublishAsync("topic", sendContext);

            //Assert
            produced.Value.Should().Equal(envelopeBytes);
            produced.Key.Should().Equal(System.Text.Encoding.UTF8.GetBytes("stream-1"));
            producedTopic.Should().Be("topic");
        }

        [Fact]
        public async Task Test_publish_without_stream_id_has_null_key()
        {
            //Arrange
            var envelopeBytes = new byte[] { 1, 2, 3 };
            var producer = Mock.Of<IProducer<byte[], byte[]>>();
            Message<byte[], byte[]> produced = null;
            Mock.Get(producer)
                .Setup(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<byte[], byte[]>>(), It.IsAny<CancellationToken>()))
                .Callback((string topic, Message<byte[], byte[]> message, CancellationToken token) => produced = message)
                .Returns(() => Task.FromResult(new DeliveryResult<byte[], byte[]>()));

            var transport = new KafkaMessagingTransport(producer,
                new KafkaConsumerFactoryImpl(new OptionsWrapper<KafkaOptions>(new KafkaOptions())),
                new OptionsWrapper<KafkaOptions>(new KafkaOptions()));

            var sendContext = new TransportSendContext(
                PayloadBytesAccessor: () => (null, null),
                EnvelopeBytesAccessor: () => envelopeBytes,
                HeadersAccessor: () => new Dictionary<string, string>());

            //Act
            await transport.PublishAsync("topic", sendContext);

            //Assert
            produced.Key.Should().BeNull();
        }

        [Fact]
        public async Task Test_publish_sanitizes_topic()
        {
            //Arrange
            var producer = Mock.Of<IProducer<byte[], byte[]>>();
            string producedTopic = null;
            Mock.Get(producer)
                .Setup(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<byte[], byte[]>>(), It.IsAny<CancellationToken>()))
                .Callback((string topic, Message<byte[], byte[]> message, CancellationToken token) => producedTopic = topic)
                .Returns(() => Task.FromResult(new DeliveryResult<byte[], byte[]>()));

            var transport = new KafkaMessagingTransport(producer,
                new KafkaConsumerFactoryImpl(new OptionsWrapper<KafkaOptions>(new KafkaOptions())),
                new OptionsWrapper<KafkaOptions>(new KafkaOptions()));

            var sendContext = new TransportSendContext(
                PayloadBytesAccessor: () => (null, null),
                EnvelopeBytesAccessor: () => new byte[] { 1 },
                HeadersAccessor: () => new Dictionary<string, string>());

            //Act
            await transport.PublishAsync("spa ce/!", sendContext);

            //Assert
            producedTopic.Should().Be("spa_ce__");
        }

        [Fact]
        public void Test_kafka_options_binds_sample_keys()
        {
            //Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Messaging:Kafka:bootstrap_servers"] = "localhost:9092",
                    ["Messaging:Kafka:group_id"] = "g1",
                })
                .Build();

            //Act
            var services = new ServiceCollection().AddKafkaTransport(configuration);
            var options = services.BuildServiceProvider().GetRequiredService<IOptions<KafkaOptions>>().Value;

            //Assert
            options.BootstrapServers.Should().Be("localhost:9092");
            options.GroupId.Should().Be("g1");
        }
    }
}
