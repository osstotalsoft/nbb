// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NBB.Messaging.Abstractions;
using Proto.V1;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace NBB.Messaging.Rusi.Tests
{
    public class PublisherTests
    {
        [Fact]
        public async Task Test_publish_request()
        {
            //Arrange
            var config = new ConfigurationBuilder().Build();
            var message = new TestMessage { TestProp = "test1" };
            var rusiClient = Mock.Of<Proto.V1.Rusi.RusiClient>();
            var topicReg = new DefaultTopicRegistry(config);
            var serdes = new NewtonsoftJsonMessageSerDes(new DefaultMessageTypeRegistry());
            var publisher = new RusiMessageBusPublisher(rusiClient,
                new OptionsWrapper<RusiOptions>(new RusiOptions() { PubsubName = "pubsub1" }),
                serdes, topicReg, new NullLogger<RusiMessageBusPublisher>(),
                config);

            PublishRequest publishRequest = null;

            Mock.Get(rusiClient)
                .Setup(x =>
                    x.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<Metadata>(), null, CancellationToken.None))
                .Callback((PublishRequest req, Metadata metadata, DateTime? deadline, CancellationToken token) =>
                {
                    publishRequest = req;
                })
                .Returns(() => new AsyncUnaryCall<Empty>(Task.FromResult(new Empty()),
                    null, null, null, null, null));


            //Act     
            await publisher.PublishAsync(message, MessagingPublisherOptions.Default);

            //Assert
            publishRequest.Data.ToStringUtf8().Should().Be("{\"TestProp\":\"test1\"}");
            publishRequest.Topic.Should().Be(topicReg.GetTopicForMessageType(message.GetType()));
            publishRequest.Metadata.Should().ContainKey(MessagingHeaders.MessageType);
            publishRequest.Metadata.Should().ContainKey(MessagingHeaders.CorrelationId);

        }



        public class TestMessage
        {
            public string TestProp { set; get; }
        }

        private class AsyncStreamReader<T> : IAsyncStreamReader<T>
        {
            private readonly IAsyncEnumerator<T> _enumerator;

            public AsyncStreamReader(IAsyncEnumerator<T> enumerator)
            {
                this._enumerator = enumerator;
            }

            public Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                return _enumerator.MoveNextAsync().AsTask();
            }

            public T Current => _enumerator.Current;
        }

        public class MockClient : Proto.V1.Rusi.RusiClient
        {
            public PublishRequest LastPublishRequest { set; get; }
            public SubscribeRequest LastSubscribeRequest { set; get; }

            private async IAsyncEnumerable<ReceivedMessage> GetEnumerable()
            {
                yield return new ReceivedMessage()
                {
                    Data = LastPublishRequest.Data,
                    Metadata = { LastPublishRequest.Metadata }
                };
            }

            public override AsyncUnaryCall<Empty> PublishAsync(PublishRequest request,
                Metadata headers = null,
                DateTime? deadline = null,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                LastPublishRequest = request;
                return null;
            }

            public override AsyncServerStreamingCall<ReceivedMessage> Subscribe(SubscribeRequest request,
                Metadata headers = null, DateTime? deadline = null,
                CancellationToken cancellationToken = default(CancellationToken))
            {

                LastSubscribeRequest = request;
                return new AsyncServerStreamingCall<ReceivedMessage>(
                    new AsyncStreamReader<ReceivedMessage>(GetEnumerable().GetAsyncEnumerator()),
                    o => Task.FromResult(new Metadata()),
                    o => Status.DefaultSuccess,
                    o => new Metadata(),
                    o => { }, null
                );
            }
        }
    }
}
