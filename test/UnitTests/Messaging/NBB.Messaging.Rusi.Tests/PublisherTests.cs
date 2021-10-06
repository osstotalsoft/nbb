// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
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
            var payloadString = "{\"TestProp\":\"test1\"}";
            var topic = "topic";
            var rusiClient = Mock.Of<Proto.V1.Rusi.RusiClient>();
            var publisher = new RusiMessagingTransport(rusiClient,
                new OptionsWrapper<RusiOptions>(new RusiOptions() { PubsubName = "pubsub1" }), Mock.Of<ITransportMonitor>(), null);

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

            var sendContext = new TransportSendContext(
                PayloadBytesAccessor: () => (Google.Protobuf.ByteString.CopyFromUtf8(payloadString).ToByteArray(), new Dictionary<string, string>() { ["h1"] = "v1" }),
                EnvelopeBytesAccessor: () => null,
                HeadersAccessor: () => new Dictionary<string, string>() { ["h2"] = "v2" });

            //Act     
            await publisher.PublishAsync(topic, sendContext);

            //Assert
            publishRequest.Data.ToStringUtf8().Should().Be(payloadString);
            publishRequest.Topic.Should().Be(topic);
            publishRequest.Metadata.Should().ContainKey("h1");
            publishRequest.Metadata.Should().ContainKey("h2");
        }
    }
}
