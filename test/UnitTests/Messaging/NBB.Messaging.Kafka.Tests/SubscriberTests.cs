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
    internal class MockedConsumerFactory(IConsumer<byte[], byte[]> consumer) : KafkaConsumerFactory
    {
        public SubscriptionTransportOptions CapturedOptions { get; set; } = null;

        public IConsumer<byte[], byte[]> Create(string topic, SubscriptionTransportOptions options)
        {
            CapturedOptions = options;
            return consumer;
        }
    }

    public class SubscriberTests
    {
        private static ConsumeResult<byte[], byte[]> Result(byte[] payload, long offset, int partition = 0) =>
            new ConsumeResult<byte[], byte[]> { Topic = "topic", Partition = new Partition(partition), Offset = new Offset(offset),
                Message = new Message<byte[], byte[]> { Value = payload } };

        private static ConsumeResult<byte[], byte[]> Eof(int partition = 0) =>
            new ConsumeResult<byte[], byte[]> { Topic = "topic", Partition = new Partition(partition), IsPartitionEOF = true };

        private static KafkaMessagingTransport Transport(MockedConsumerFactory factory) =>
            new(null, factory);

        [Fact]
        public async Task TestSubscriber()
        {
            // happy path: 3 messages, handler gets EnvelopeBytes, commit per partition, dispose closes
            var payloadBytes = new byte[] { 1, 2, 3 };
            var consumer = Mock.Of<IConsumer<byte[], byte[]>>();
            Mock.Get(consumer).SetupSequence(c => c.Consume(It.IsAny<System.TimeSpan>()))
                .Returns(Result(payloadBytes, 0)).Returns(Result(payloadBytes, 1)).Returns(Result(payloadBytes, 2))
                .Returns(Eof());

            var handler = Mock.Of<Func<TransportReceiveContext, Task>>();
            var transport = Transport(new MockedConsumerFactory(consumer));

            //Act
            var subscription = await transport.SubscribeAsync("topic", handler,
                new SubscriptionTransportOptions { MaxConcurrentMessages = 3 });
            await Task.Delay(100); // Rusi convention: let poll loop run
            subscription.Dispose();

            //Assert
            Mock.Get(handler).Verify(h => h(It.Is<TransportReceiveContext>(m =>
                ((TransportReceivedData.EnvelopeBytes)m.ReceivedData).Bytes == payloadBytes)), Times.Exactly(3));
            Mock.Get(consumer).Verify(c => c.Commit(It.IsAny<ConsumeResult<byte[], byte[]>>()), Times.Once);
            Mock.Get(consumer).Verify(c => c.Close(), Times.Once);
        }

        [Fact]
        public async Task Test_subscribe_sanitizes_topic()
        {
            var consumer = Mock.Of<IConsumer<byte[], byte[]>>();
            var subscription = await Transport(new MockedConsumerFactory(consumer))
                .SubscribeAsync("spa ce/!", Mock.Of<Func<TransportReceiveContext, Task>>());
            subscription.Dispose();
            Mock.Get(consumer).Verify(c => c.Subscribe("spa_ce__"));
        }

        [Fact]
        public async Task Test_subscribe_skips_partition_eof()
        {
            var consumer = Mock.Of<IConsumer<byte[], byte[]>>();
            Mock.Get(consumer).SetupSequence(c => c.Consume(It.IsAny<System.TimeSpan>()))
                .Returns(Eof());
            var handler = Mock.Of<Func<TransportReceiveContext, Task>>();
            var subscription = await Transport(new MockedConsumerFactory(consumer)).SubscribeAsync("topic", handler);
            await Task.Delay(100);
            subscription.Dispose();
            Mock.Get(handler).Verify(h => h(It.IsAny<TransportReceiveContext>()), Times.Never);
            Mock.Get(consumer).Verify(c => c.Commit(It.IsAny<ConsumeResult<byte[], byte[]>>()), Times.Never);
        }

        [Fact]
        public async Task Test_subscribe_default_options_batch_size_one()
        {
            var consumer = Mock.Of<IConsumer<byte[], byte[]>>();
            Mock.Get(consumer).SetupSequence(c => c.Consume(It.IsAny<System.TimeSpan>()))
                .Returns(Eof());
            var factory = new MockedConsumerFactory(consumer);
            var transport = new KafkaMessagingTransport(null, factory);

            var subscription = await transport.SubscribeAsync("topic", Mock.Of<Func<TransportReceiveContext, Task>>());
            await Task.Delay(100);
            subscription.Dispose();

            factory.CapturedOptions.Should().NotBeNull();
            factory.CapturedOptions.MaxConcurrentMessages.Should().Be(1);
            Mock.Get(consumer).Verify(c => c.Consume(It.IsAny<System.TimeSpan>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Test_subscribe_dispose_closes_consumer()
        {
            var consumer = Mock.Of<IConsumer<byte[], byte[]>>();
            Mock.Get(consumer).SetupSequence(c => c.Consume(It.IsAny<System.TimeSpan>()))
                .Returns(Eof());
            var subscription = await Transport(new MockedConsumerFactory(consumer))
                .SubscribeAsync("topic", Mock.Of<Func<TransportReceiveContext, Task>>());
            subscription.Dispose();
            Mock.Get(consumer).Verify(c => c.Close(), Times.Once);
        }

        [Fact]
        public async Task Test_subscribe_commits_per_partition()
        {
            // results on partitions 0 and 1: Commit called twice, once per TopicPartition
            var consumer = Mock.Of<IConsumer<byte[], byte[]>>();
            Mock.Get(consumer).SetupSequence(c => c.Consume(It.IsAny<System.TimeSpan>()))
                .Returns(Result(new byte[] { 1 }, 0, 0)).Returns(Result(new byte[] { 2 }, 0, 1))
                .Returns(Eof());
            var subscription = await Transport(new MockedConsumerFactory(consumer))
                .SubscribeAsync("topic", Mock.Of<Func<TransportReceiveContext, Task>>());
            await Task.Delay(100);
            subscription.Dispose();
            Mock.Get(consumer).Verify(c => c.Commit(It.IsAny<ConsumeResult<byte[], byte[]>>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Test_subscribe_nonfatal_consume_error_continues()
        {
            var payloadBytes = new byte[] { 7 };
            var consumer = Mock.Of<IConsumer<byte[], byte[]>>();
            Mock.Get(consumer).SetupSequence(c => c.Consume(It.IsAny<System.TimeSpan>()))
                .Throws(new ConsumeException(Eof(), new Error(ErrorCode.Local_InvalidArg, "non-fatal boom")))
                .Returns(Result(payloadBytes, 0)).Returns(Eof());
            var handler = Mock.Of<Func<TransportReceiveContext, Task>>();
            var transport = Transport(new MockedConsumerFactory(consumer));
            var errored = 0;
            transport.OnError += _ => errored++;
            var subscription = await transport.SubscribeAsync("topic", handler);
            await Task.Delay(100);
            subscription.Dispose();
            errored.Should().Be(1);
            Mock.Get(handler).Verify(h => h(It.IsAny<TransportReceiveContext>()), Times.Once);
        }

        [Fact]
        public async Task Test_subscribe_fatal_consume_error_stops_loop()
        {
            var consumer = Mock.Of<IConsumer<byte[], byte[]>>();
            Mock.Get(consumer).SetupSequence(c => c.Consume(It.IsAny<System.TimeSpan>()))
                .Throws(new ConsumeException(Eof(), new Error(ErrorCode.Local_Fatal, "fatal boom", true)));
            var handler = Mock.Of<Func<TransportReceiveContext, Task>>();
            var transport = Transport(new MockedConsumerFactory(consumer));
            var errored = 0;
            transport.OnError += _ => errored++;
            var subscription = await transport.SubscribeAsync("topic", handler);
            await Task.Delay(100);
            subscription.Dispose();
            errored.Should().Be(1);
            Mock.Get(handler).Verify(h => h(It.IsAny<TransportReceiveContext>()), Times.Never);
        }

        [Fact]
        public async Task Test_subscribe_handler_failure_reported()
        {
            // handler throws on first result -> OnError fires, loop continues, second result delivered
            var consumer = Mock.Of<IConsumer<byte[], byte[]>>();
            Mock.Get(consumer).SetupSequence(c => c.Consume(It.IsAny<System.TimeSpan>()))
                .Returns(Result(new byte[] { 1 }, 0)).Returns(Result(new byte[] { 2 }, 1))
                .Returns(Eof());
            var handler = Mock.Of<Func<TransportReceiveContext, Task>>();
            Mock.Get(handler).SetupSequence(h => h(It.IsAny<TransportReceiveContext>()))
                .Throws(new Exception("handler boom")).Returns(Task.CompletedTask);
            var transport = Transport(new MockedConsumerFactory(consumer));
            var errored = 0;
            transport.OnError += _ => errored++;
            var subscription = await transport.SubscribeAsync("topic", handler);
            await Task.Delay(100);
            subscription.Dispose();
            errored.Should().Be(1);
        }

        [Fact]
        public void Test_add_kafka_transport_di_wiring()
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
            // resolve KafkaConsumerFactory only: its provider constructs KafkaConsumerFactoryImpl (no native
            // ProducerBuilder.Build(); full IMessagingTransport resolution needs librdkafka, deferred to integration tests)
            var factory = new ServiceCollection().AddKafkaTransport(configuration)
                .BuildServiceProvider().GetRequiredService<KafkaConsumerFactory>();

            //Assert
            factory.Should().NotBeNull();
        }

        [Fact]
        public void Test_add_kafka_transport_missing_bootstrap_servers_fails_validation()
        {
            //Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Messaging:Kafka:group_id"] = "g1",
                })
                .Build();

            //Act
            // options validation is lazy: it fires on .Value read, which Create does before any native call
            var factory = new ServiceCollection().AddKafkaTransport(configuration)
                .BuildServiceProvider().GetRequiredService<KafkaConsumerFactory>();
            var failed = false;
            try {
                _ = factory.Create("topic", SubscriptionTransportOptions.Default);
            }
            catch (Exception) {
                failed = true;
            }

            //Assert
            failed.Should().BeTrue();
        }

        [Fact]
        public async Task Test_subscribe_mid_batch_consume_error_keeps_collected_results()
        {
            // finding 1: result1 is collected, then a non-fatal ConsumeException aborts the collect loop:
            // result1 was already popped from the librdkafka buffer, so it must still be handled and committed
            var payloadBytes = new byte[] { 5 };
            var consumer = Mock.Of<IConsumer<byte[], byte[]>>();
            Mock.Get(consumer).SetupSequence(c => c.Consume(It.IsAny<System.TimeSpan>()))
                .Returns(Result(payloadBytes, 0))
                .Throws(new ConsumeException(Eof(), new Error(ErrorCode.Local_InvalidArg, "non-fatal boom")))
                .Returns(Result(payloadBytes, 1)).Returns(Eof());
            var handler = Mock.Of<Func<TransportReceiveContext, Task>>();
            var transport = Transport(new MockedConsumerFactory(consumer));
            var errored = 0;
            transport.OnError += _ => errored++;

            //Act
            var subscription = await transport.SubscribeAsync("topic", handler,
                new SubscriptionTransportOptions { MaxConcurrentMessages = 3 });
            await Task.Delay(100); // Rusi convention: let poll loop run
            subscription.Dispose();

            //Assert
            errored.Should().Be(1);
            Mock.Get(handler).Verify(h => h(It.IsAny<TransportReceiveContext>()), Times.Exactly(2));
            Mock.Get(consumer).Verify(c => c.Commit(It.IsAny<ConsumeResult<byte[], byte[]>>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Test_subscribe_commit_fault_is_nonfatal()
        {
            // regression: a transient Commit throw must not end the poll task. Pre-fix the Commit fault reached
            // the outer catch and killed the subscription (batch 2 never consumed). With the per-batch guard the
            // single Commit fault is reported via OnError and polling continues, so both batches are handled+committed.
            var consumer = Mock.Of<IConsumer<byte[], byte[]>>();
            Mock.Get(consumer).SetupSequence(c => c.Consume(It.IsAny<System.TimeSpan>()))
                .Returns(Result(new byte[] { 1 }, 0)).Returns(Result(new byte[] { 2 }, 1)).Returns(Eof());
            Mock.Get(consumer).SetupSequence(c => c.Commit(It.IsAny<ConsumeResult<byte[], byte[]>>()))
                .Throws(new Exception("commit boom"));
            var handler = Mock.Of<Func<TransportReceiveContext, Task>>();
            var transport = Transport(new MockedConsumerFactory(consumer));
            var errored = 0;
            transport.OnError += _ => errored++;
            var subscription = await transport.SubscribeAsync("topic", handler,
                new SubscriptionTransportOptions { MaxConcurrentMessages = 1, UseGroup = true });
            await Task.Delay(100); // Rusi convention: let poll loop run
            subscription.Dispose();

            //Assert
            errored.Should().Be(1); // the Commit fault is reported, poll loop kept running
            Mock.Get(handler).Verify(h => h(It.IsAny<TransportReceiveContext>()), Times.Exactly(2));
            Mock.Get(consumer).Verify(c => c.Commit(It.IsAny<ConsumeResult<byte[], byte[]>>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Test_subscribe_already_cancelled_token_skips_consumption()
        {
            // finding 2: the caller token is observed by the poll loop
            var consumer = Mock.Of<IConsumer<byte[], byte[]>>();
            Mock.Get(consumer).SetupSequence(c => c.Consume(It.IsAny<System.TimeSpan>()))
                .Returns(Result(new byte[] { 1 }, 0)).Returns(Eof());
            var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            var subscription = await Transport(new MockedConsumerFactory(consumer))
                .SubscribeAsync("topic", Mock.Of<Func<TransportReceiveContext, Task>>(),
                    SubscriptionTransportOptions.Default, cancelled.Token);
            subscription.Dispose();
            Mock.Get(consumer).Verify(c => c.Consume(It.IsAny<System.TimeSpan>()), Times.Never);
            Mock.Get(consumer).Verify(c => c.Close(), Times.Once);
        }

        [Fact]
        public async Task Test_subscribe_request_reply_skips_commit()
        {
            // finding 3: UseGroup=false (RequestReply) => ephemeral group, Commit never called
            var consumer = Mock.Of<IConsumer<byte[], byte[]>>();
            Mock.Get(consumer).SetupSequence(c => c.Consume(It.IsAny<System.TimeSpan>()))
                .Returns(Result(new byte[] { 1 }, 0)).Returns(Eof());
            var subscription = await Transport(new MockedConsumerFactory(consumer))
                .SubscribeAsync("topic", Mock.Of<Func<TransportReceiveContext, Task>>(), SubscriptionTransportOptions.RequestReply);
            await Task.Delay(100);
            subscription.Dispose();
            Mock.Get(consumer).Verify(c => c.Commit(It.IsAny<ConsumeResult<byte[], byte[]>>()), Times.Never);
        }

        [Fact]
        public void Test_consumer_group_name_nonce_when_use_group_false()
        {
            // finding 3: stable group name with UseGroup=true, unique nonce-suffixed name without
            var factory = new KafkaConsumerFactoryImpl(new OptionsWrapper<KafkaOptions>(new KafkaOptions { GroupId = "g1" }));
            factory.GroupName("topic", SubscriptionTransportOptions.Default).Should().Be("g1__topic");
            var ephemeral1 = factory.GroupName("topic", SubscriptionTransportOptions.RequestReply);
            var ephemeral2 = factory.GroupName("topic", SubscriptionTransportOptions.RequestReply);
            ephemeral1.Should().NotBe(ephemeral2);
            ephemeral1.Should().StartWith("g1__topic__");
        }
    }
}
