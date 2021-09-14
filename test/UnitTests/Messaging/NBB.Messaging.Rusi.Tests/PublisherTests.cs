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

    }
}
